using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ProBuilder;

public static class Math
{
    public static Vector3 Normal(ProBuilderMesh mesh, Face face)
    {
        if (mesh == null || face == null)
            throw new ArgumentNullException("mesh");

        var positions = mesh.positions;

        // if the face is just a quad, use the first
        // triangle normal.
        // otherwise it's not safe to assume that the face
        // has even generally uniform normals
        Vector3 nrm = Normal(
                positions[face.indexes[0]],
                positions[face.indexes[1]],
                positions[face.indexes[2]]);

        if (face.indexes.Count > 6)
        {
            Vector3 prj = Projection.FindBestPlane(positions, face.distinctIndexes).normal;

            if (Vector3.Dot(nrm, prj) < 0f)
            {
                nrm.x = -prj.x;
                nrm.y = -prj.y;
                nrm.z = -prj.z;
            }
            else
            {
                nrm.x = prj.x;
                nrm.y = prj.y;
                nrm.z = prj.z;
            }
        }

        return nrm;
    }

    public static Vector3 Normal(Vector3 p0, Vector3 p1, Vector3 p2)
    {
        float ax = p1.x - p0.x,
                ay = p1.y - p0.y,
                az = p1.z - p0.z,
                bx = p2.x - p0.x,
                by = p2.y - p0.y,
                bz = p2.z - p0.z;

        Vector3 cross = Vector3.zero;

        Cross(ax, ay, az, bx, by, bz, ref cross.x, ref cross.y, ref cross.z);

        if (cross.magnitude < Mathf.Epsilon)
        {
            return new Vector3(0f, 0f, 0f); // bad triangle
        }
        else
        {
            cross.Normalize();
            return cross;
        }
    }

    internal static void Cross(float ax, float ay, float az, float bx, float by, float bz, ref float x, ref float y, ref float z)
    {
        x = ay * bz - az * by;
        y = az * bx - ax * bz;
        z = ax * by - ay * bx;
    }
    public static float ContAngle(Vector3 fwd, Vector3 targetDir)
    {
        float angle = Vector3.Angle(fwd, targetDir);

        if (AngleDir(fwd, targetDir, Vector3.up) == -1)
        {
            angle = 360.0f - angle;
            if (angle > 359.9999f)
                angle -= 360.0f;
            return angle;
        }
        else
            return angle;
    }
    public static int AngleDir(Vector3 fwd, Vector3 targetDir, Vector3 up)
    {
        Vector3 perp = Vector3.Cross(fwd, targetDir);
        float dir = Vector3.Dot(perp, up);

        if (dir > 0.0)
            return 1;
        else if (dir < 0.0)
            return -1;
        else
            return 0;
    }

}