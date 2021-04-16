using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MyFramework : MonoSingleton<MyFramework>
{
	public List<MeshController> meshes = new List<MeshController>();

	private void Start()
	{
		DataManager.Instance.LoadJson();
	}

	public void CreateMesh(BuildingData data)
	{
		if (!meshes.Exists(obj => obj.data == data))
		{
			ResourceManager.Instance.CreateObject("MeshController", CreateCallback, data);
			//var meshController = Instantiate<MeshController>(Resources.Load<MeshController>("MeshController"));
		}
	}

	public void CreateCallback(GameObject resource, BuildingData data)
	{
		var meshController = resource.GetComponent<MeshController>();
		meshes.Add(meshController);
		meshController.SetData(data);
		StartCoroutine(Create(meshController));
	}

	public IEnumerator Create(MeshController mc)
	{
		yield return null;
		//Mesh, 변수 생성 및 초기화
		mc.mesh = new Mesh();
		mc.root.AddComponent<MeshFilter>().mesh = mc.mesh;
		mc.mesh.Clear();
		var renderer = mc.root.AddComponent<MeshRenderer>();

		var points = new List<Vector3>();
		var minY = float.MaxValue;
		var maxY = float.MinValue;

		//모든 정점 좌표를 받아온다. (천장, 바닥, 벽면)
		//층이 다른 건물이 있을 경우 하나 더 만든다.
		List<Vector3> secondPoints;
		points = mc.GetPoints(out secondPoints);

		List<int> triangles = new List<int>();

		//정점 좌표를 근거로 3개씩 묶어 면을 만든다.
		triangles = CreateCeilTriangle(points);
		Vector3[] vertices = points.ToArray();

		//건물 높이 값 계산 (가장 높은 값과 가장 낮은 값을 구해 높이를 구한다.
		foreach (var pos in points)
		{
			minY = Mathf.Min(pos.y, minY);
			maxY = Mathf.Max(pos.y, maxY);
		}

		var height = Mathf.Abs(maxY - minY);

		mc.root.transform.parent = DataManager.Instance.complexParent;
		mc.root.transform.localPosition = new Vector3(0, mc.data.meta.height, 0);
		mc.root.name = mc.data.meta.name;
		

		if (triangles.Count > 0)
		{
			mc.mesh.vertices = vertices;
			mc.mesh.triangles = triangles.ToArray();
			mc.mesh.uv = new Vector2[vertices.Length];
			SetUV(mc);
			mc.CreatePoints();
			renderer.sharedMaterial = DataManager.Instance.CreateMaterial();
			renderer.sharedMaterial.SetTextureScale("_BaseMap", new Vector2(1, Mathf.Floor(height / 3f)));
			renderer.sharedMaterial.SetTextureOffset("_BaseMap", new Vector2(0, 0));
			//mesh.uv = uv.ToArray();

			mc.mesh.Optimize();
			mc.mesh.RecalculateNormals();
		}
		StartCoroutine(CreateSecondMesh(mc.data, secondPoints));
	}

	public IEnumerator CreateSecondMesh(BuildingData data, List<Vector3> points)
	{
		yield return null;
		if(points != null && points.Count > 0)
		{
			var mc = Instantiate<MeshController>(Resources.Load<MeshController>("MeshController"));
			meshes.Add(mc);
			mc.SetData(data);
			StartCoroutine(Create(mc));
			//Mesh, 변수 생성 및 초기화
			mc.mesh = new Mesh();
			mc.root.AddComponent<MeshFilter>().mesh = mc.mesh;
			mc.mesh.Clear();
			var renderer = mc.root.AddComponent<MeshRenderer>();

			var minY = float.MaxValue;
			var maxY = float.MinValue;

			//모든 정점 좌표를 받아온다. (천장, 바닥, 벽면)
			//층이 다른 건물이 있을 경우 하나 더 만든다.

			List<int> triangles = new List<int>();

			//정점 좌표를 근거로 3개씩 묶어 면을 만든다.
			triangles = CreateCeilTriangle(points);
			Vector3[] vertices = points.ToArray();

			//건물 높이 값 계산 (가장 높은 값과 가장 낮은 값을 구해 높이를 구한다.
			foreach (var pos in points)
			{
				minY = Mathf.Min(pos.y, minY);
				maxY = Mathf.Max(pos.y, maxY);
			}

			var height = Mathf.Abs(maxY - minY);

			mc.root.transform.parent = DataManager.Instance.complexParent;
			mc.root.transform.localPosition = new Vector3(0, 0, mc.data.meta.height);
			mc.root.name = mc.data.meta.name;


			if (triangles.Count > 0)
			{
				mc.mesh.vertices = vertices;
				mc.mesh.triangles = triangles.ToArray();
				mc.mesh.uv = new Vector2[vertices.Length];
				SetSecondUV(mc);
				mc.CreatePoints();
				renderer.sharedMaterial = DataManager.Instance.CreateMaterial();
				renderer.sharedMaterial.SetTextureScale("_BaseMap", new Vector2(1, Mathf.Floor(height / 3f)));
				renderer.sharedMaterial.SetTextureOffset("_BaseMap", new Vector2(0, 0));
				//mesh.uv = uv.ToArray();

				mc.mesh.Optimize();
				mc.mesh.RecalculateNormals();
			}
		}
	}

	public void SetUV(MeshController mc)
	{
		List<Vector3> secondPoints;
		var points = mc.GetPoints(out secondPoints);
		var positions = new Dictionary<int, Vector3>();
		for (int i = 0; i < points.Count; i++)
		{
			positions.Add(i, points[i]);
			//포지션 6개 단위로 uv 매핑
			if (positions.Count == 6)
			{
				var uv = mc.mesh.uv;
				var state = GetWallState(positions, mc.GetFoward());	
				var pos = positions.ToArray();
				if (pos.Length == 0)
				{
					positions.Clear();
					continue;
				}
				switch (state)
				{
					case Defines.WallState.FRONT:
						uv[pos[0].Key] = new Vector2(0.5f, 0);          // 1 (1, 0)
						uv[pos[1].Key] = new Vector2(0, 0);             // 0 (0, 0)
						uv[pos[2].Key] = new Vector2(0, 0.5f);          // 3 (0, 1)
						uv[pos[3].Key] = new Vector2(0, 0.5f);          // 3 (0, 1)
						uv[pos[4].Key] = new Vector2(0.5f, 0.5f);       // 2 (1, 1)
						uv[pos[5].Key] = new Vector2(0.5f, 0);          // 1 (1, 0)
						break;
					case Defines.WallState.WALL:
						uv[pos[0].Key] = new Vector2(0.75f, 0);         // 1 (1, 0)
						uv[pos[1].Key] = new Vector2(0.5f, 0);          // 0 (0, 0)
						uv[pos[2].Key] = new Vector2(0.5f, 0.5f);       // 3 (0, 1)
						uv[pos[3].Key] = new Vector2(0.5f, 0.5f);       // 3 (0, 1)
						uv[pos[4].Key] = new Vector2(0.75f, 0.5f);      // 2 (1, 1)
						uv[pos[5].Key] = new Vector2(0.75f, 0);         // 1 (1, 0)
						break;
					case Defines.WallState.CEIL:
						uv[pos[0].Key] = new Vector2(0.75f, 0);         // 0 (0, 0)
						uv[pos[1].Key] = new Vector2(1f, 0);            // 1 (1, 0)
						uv[pos[2].Key] = new Vector2(1f, 0.5f);         // 2 (1, 1)
						uv[pos[3].Key] = new Vector2(0.75f, 0);         // 3 (0, 1)
						uv[pos[4].Key] = new Vector2(0.75f, 0.5f);      // 0 (0, 0)
						uv[pos[5].Key] = new Vector2(1f, 0.5f);         // 2 (1, 1)
						break;
				}
				positions.Clear();
				mc.mesh.uv = uv.ToArray();
			}
		}
	}

	public void SetSecondUV(MeshController mc)
	{
		List<Vector3> secondPoints;
		var points = mc.GetPoints(out secondPoints);
		var positions = new Dictionary<int, Vector3>();
		for (int i = 0; i < secondPoints.Count; i++)
		{
			positions.Add(i, secondPoints[i]);
			if (positions.Count == 6)
			{
				var uv = mc.mesh.uv;
				var state = GetWallState(positions, mc.GetFoward());
				var pos = positions.ToArray();
				if (pos.Length == 0)
				{
					positions.Clear();
					continue;
				}
				switch (state)
				{
					case Defines.WallState.FRONT:
						uv[pos[0].Key] = new Vector2(0.5f, 0);          // 1 (1, 0)
						uv[pos[1].Key] = new Vector2(0, 0);             // 0 (0, 0)
						uv[pos[2].Key] = new Vector2(0, 0.5f);          // 3 (0, 1)
						uv[pos[3].Key] = new Vector2(0, 0.5f);          // 3 (0, 1)
						uv[pos[4].Key] = new Vector2(0.5f, 0.5f);       // 2 (1, 1)
						uv[pos[5].Key] = new Vector2(0.5f, 0);          // 1 (1, 0)
						break;
					case Defines.WallState.WALL:
						uv[pos[0].Key] = new Vector2(0.75f, 0);         // 1 (1, 0)
						uv[pos[1].Key] = new Vector2(0.5f, 0);          // 0 (0, 0)
						uv[pos[2].Key] = new Vector2(0.5f, 0.5f);       // 3 (0, 1)
						uv[pos[3].Key] = new Vector2(0.5f, 0.5f);       // 3 (0, 1)
						uv[pos[4].Key] = new Vector2(0.75f, 0.5f);      // 2 (1, 1)
						uv[pos[5].Key] = new Vector2(0.75f, 0);         // 1 (1, 0)
						break;
					case Defines.WallState.CEIL:
						uv[pos[0].Key] = new Vector2(0.75f, 0);         // 0 (0, 0)
						uv[pos[1].Key] = new Vector2(1f, 0);            // 1 (1, 0)
						uv[pos[2].Key] = new Vector2(1f, 0.5f);         // 2 (1, 1)
						uv[pos[3].Key] = new Vector2(0.75f, 0);         // 3 (0, 1)
						uv[pos[4].Key] = new Vector2(0.75f, 0.5f);      // 0 (0, 0)
						uv[pos[5].Key] = new Vector2(1f, 0.5f);         // 2 (1, 1)
						break;
				}
				positions.Clear();
				mc.mesh.uv = uv.ToArray();
			}
		}
	}

	public Defines.WallState GetWallState(Dictionary<int, Vector3> positions, Vector3 forward)
	{
		var pos = positions.ToArray();
		var normal = Math.Normal(pos[0].Value, pos[1].Value, pos[2].Value);
		//forward = Vector3.forward;
		var angle = Math.ContAngle(forward, normal);
		var wallState = Defines.WallState.WALL;
		Debug.Log(angle);
		//wallState = angle >= 180 && angle <= 220 ? WallState.FRONT : wallState;
		//var differentAngle = Vector3.Angle(forward, pbm.transform.forward);
		//var rightAngle = Vector3.Angle(normal, pbm.transform.right) - differentAngle;
		//if (rightAngle > 90) angle = 360 - angle;
		//wallState = angle <= 180 && angle >= 140 ? Defines.WallState.FRONT : wallState;
		wallState = angle >= 170 && angle <= 220 ? Defines.WallState.FRONT : wallState;
		wallState = normal == Vector3.up || normal == Vector3.down ? Defines.WallState.CEIL : wallState;
		return wallState;
	}

	public bool IsSideWall(Dictionary<int, Vector3> positions, Vector3 forward)
	{
		var pos = positions.ToArray();
		var normal = Math.Normal(pos[0].Value, pos[1].Value, pos[2].Value);
		var angle = Vector3.Angle(forward, normal);
		if (normal != Vector3.up && normal != Vector3.down && angle == 90) return true;
		return false;
	}

	public List<int> CreateCeilTriangle(List<Vector3> points)
	{
		List<int> indexes = new List<int>();

		for (int i = 0; i < points.Count; i += 3)
		{
			indexes.Add(i);
			indexes.Add(i + 1);
			indexes.Add(i + 2);
		}/**//*
		for (int i = 0; i < points.Count; i += 4)
		{
			indexes.Add(i);
			indexes.Add(i + 1);
			indexes.Add(i + 2);
			indexes.Add(i + 3);
			indexes.Add(i);
			indexes.Add(i + 2);
		}/**/
		return indexes;
	}

	public List<int> CreateWallTriangle(List<Vector3> points)
	{
		List<int> indexes = new List<int>();

		for (int i = 0; i < points.Count; i += 3)
		{
			indexes.Add(i);
			indexes.Add(i + 1);
			indexes.Add(i + 2);
		}/**//*
		for (int i = 0; i < points.Count; i += 6)
		{
			indexes.Add(i);
			indexes.Add(i + 1);
			indexes.Add(i + 2);
			indexes.Add(i + 3);
			indexes.Add(i + 4);
			indexes.Add(i + 5);
		}/**/
		return indexes;
	}

	public void CreatePoint(int idx, Vector3 pos)
	{
		var point = Instantiate(Resources.Load("Point")) as GameObject;
		point.name = "Point" + idx;
		point.transform.parent = transform;
		point.transform.position = pos;
	}
}
