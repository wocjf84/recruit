using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.ProBuilder;
using System.Linq;

[System.Serializable]
public class ApiResponse
{
	public bool seccess;
	public int code;
	public List<BuildingData> data;
}

[System.Serializable]
public class BuildingMeta
{
	public int bd_id;
	public string name;
	public int height;
}

[System.Serializable]
public class BuildingData
{
	public List<TypeData> roomtypes;
	public BuildingMeta meta;
}

[System.Serializable]
public class TypeMeta
{
	public int roomId;
}

[System.Serializable]
public class TypeData
{
	public string[] coordinatesBase64s;
	public TypeMeta meta;

	public List<List<Vector3>> vertices = new List<List<Vector3>>();

	public void CreatePositions()
	{
		foreach (var coordinates in coordinatesBase64s)
		{
			var bytes = System.Convert.FromBase64String(coordinates);
			var floats = new float[bytes.Length / 4];
			System.Buffer.BlockCopy(bytes, 0, floats, 0, bytes.Length);
			var positions = new List<Vector3>();
			for (int i = 0; i < floats.Length; i += 3)
			{
				positions.Add(new Vector3(floats[i], floats[i + 2], floats[i + 1]));
			}
			vertices.Add(positions);
		}
	}
}

public class DataManager : MonoSingleton<DataManager>
{
	public ApiResponse response = new ApiResponse();

	[SerializeField]
	public Material wallMaterial;

	[SerializeField]
	public Material ceilMaterial;

	[SerializeField]
	public Material sideMaterial;

	[SerializeField]
	public Texture texture;

	[SerializeField]
	public Transform complexParent;


	public void LoadJson()
	{
		var path = Path.Combine(Application.dataPath, "Samples\\json\\dong.json");
		if (File.Exists(path))
		{
			var file = File.ReadAllText(path);
			file = file.Replace("동\":", "name\":");
			file = file.Replace("지면높이", "height");
			file = file.Replace("룸타입id", "roomId");
			response = JsonUtility.FromJson<ApiResponse>(file);
			Debug.Log(file);
			foreach (var building in response.data)
			{
				MyFramework.Instance.CreateMesh(building);
			}
		}
	}

	public Material CreateMaterial()
	{
		Shader lit = Shader.Find("Universal Render Pipeline/Lit");
		var mat = new Material(lit);
		mat.SetTexture("_BaseMap", texture);
		return mat;
	}
}
