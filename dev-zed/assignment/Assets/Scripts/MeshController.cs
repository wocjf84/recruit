using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MeshController : MonoBehaviour
{
	[SerializeField]
	public BuildingData data;

	[SerializeField]
	public GameObject root;

	[SerializeField]
	public MeshRenderer meshRenderer { get => GetComponent<MeshRenderer>(); }

	public Mesh mesh;

	public Vector3 forward;

	public Vector3 position;

	public void SetData(BuildingData data)
	{
		this.data = data;

		List<Vector3> positions = new List<Vector3>();
		foreach (var roomType in data.roomtypes)
		{
			if (roomType.vertices.Count == 0)
				roomType.CreatePositions();
			foreach (var vertice in roomType.vertices)
			{
				positions.AddRange(vertice.FindAll(obj => obj.y == 0));
			}
		}
		Vector3 position = Vector3.zero;
		foreach(var pos in positions)
		{
			position += pos;
		}
		position /= positions.Count;
		this.position = position;
	}

	public List<Vector3> GetPoints(out List<Vector3> secondPoints)
	{
		secondPoints = new List<Vector3>();
		List<Vector3> points = new List<Vector3>();
		int idx = 0;
		foreach (var roomType in data.roomtypes)
		{
			if (roomType.vertices.Count == 0)
				roomType.CreatePositions();
			foreach (var vertice in roomType.vertices)
			{
				if (idx == 0)
					points.AddRange(vertice);
				else
					secondPoints.AddRange(vertice);
			}
			idx++;
		}
		return points;
	}

	public void CreatePoints()
	{
		List<Vector3> secondPoints;
		var points = GetPoints(out secondPoints);
		for (int i = 0; i < points.Count; i++)
		{
			CreatePoint(i, points[i]);
		}
	}

	void CreatePoint(int idx, Vector3 pos)
	{
		var point = Instantiate(Resources.Load("Point")) as GameObject;
		point.name = "Point" + idx;
		point.transform.parent = transform;
		point.transform.position = pos;
	}

	public Vector3 GetFoward()
	{
		var minAngle = 1000f;
		var wallVertices = data.roomtypes[0].vertices[1];
		if (this.forward == Vector3.zero)
		{
			Dictionary<Vector3, List<int>> normalIdxs = new Dictionary<Vector3, List<int>>();
			for (int i = 0; i < wallVertices.Count; i += 3)
			{
				var normal = Math.Normal(wallVertices[i], wallVertices[i + 1], wallVertices[i + 2]);
				var keyList = new List<Vector3>(normalIdxs.Keys);
				var sameKey = keyList.Find(obj => Vector3.Angle(normal, obj) < 10);
				var key = sameKey == Vector3.zero ? normal : sameKey;
				if (!normalIdxs.ContainsKey(key)) normalIdxs.Add(key, new List<int>());
				normalIdxs[key].Add(i);
			}
			var keys = new List<Vector3>(normalIdxs.Keys);
			var minCount = int.MaxValue;
			foreach (var key in keys)
			{
				minCount = Mathf.Min(minCount, normalIdxs[key].Count);
			}
			foreach (var key in keys)
			{
				if (normalIdxs[key].Count == minCount) continue;
				var angle = Vector3.Angle(transform.forward, key);
				minAngle = Mathf.Min(minAngle, angle);
				if (minAngle == angle) this.forward = key;
			}
		}
		return this.forward;
	}
}
