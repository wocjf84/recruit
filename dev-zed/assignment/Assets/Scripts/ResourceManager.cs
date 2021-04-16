using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ResourceData
{
	public string resourceName;
	public UnityAction<GameObject, BuildingData> callback;
	public BuildingData data;

	public ResourceData(string name, UnityAction<GameObject, BuildingData> callback, BuildingData data = null)
	{
		this.resourceName = name;
		this.callback = callback;
		this.data = data;
	}
}

public class ResourceManager : MonoSingleton<ResourceManager>
{
	Queue<ResourceData> resourceQueue = new Queue<ResourceData>();
	protected override void OnAwake()
	{
		StartCoroutine(OnUpdate());
	}

	public IEnumerator OnUpdate()
	{
		int idx = 0;
		while(true)
		{
			if (idx == 10 || resourceQueue.Count == 0)
			{
				idx = 0;
				yield return null;
			}
			if(resourceQueue.Count > 0)
			{
				var resourceData = resourceQueue.Dequeue();
				var obj = Instantiate(Resources.Load(resourceData.resourceName)) as GameObject;
				resourceData.callback(obj, resourceData.data);
			}
			idx++;
		}
	}

	public void CreateObject(string resource, UnityAction<GameObject, BuildingData> callback, BuildingData data = null)
	{
		resourceQueue.Enqueue(new ResourceData(resource, callback, data));
	}
}
