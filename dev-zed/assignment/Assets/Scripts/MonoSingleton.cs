using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Class, Inherited = true)]
public class PersistentAttribute : Attribute
{
    public readonly bool Persistent;
    public PersistentAttribute( bool persistent )
    {
        this.Persistent = persistent;
    }
}

public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
{
	private static T instance = null;
	private static bool instantiated = false;

	public static bool isInitialize { get => instance != null;}

	static public T Instance
	{
		get
		{
			if (instance == null)
				Create();
			return instance;
		}
		set
		{
			instance = value;
		}
	}

	static public void Create()
	{
		if (instance == null)
		{
			T[] objects = GameObject.FindObjectsOfType<T>();
			if (objects.Length > 0)
			{
				instance = objects[0];

				for (int i = 1; i < objects.Length; ++i)
				{
					if (Application.isPlaying)
						GameObject.Destroy(objects[i].gameObject);
					else
						GameObject.DestroyImmediate(objects[i].gameObject);
				}
			}
			else
			{
				GameObject go = new GameObject(string.Format("[Singleton]{0}", typeof(T).Name));
				instance = go.AddComponent<T>();
			}

			if (!instantiated)
			{
				PersistentAttribute attribute = Attribute.GetCustomAttribute(typeof(T), typeof(PersistentAttribute)) as PersistentAttribute;
				if (attribute != null && attribute.Persistent)
				{
					instance.persistent = attribute.Persistent;
					GameObject.DontDestroyOnLoad(instance.gameObject);
				}

				instance.OnAwake();
			}

			instantiated = true;
		}
	}

	private bool persistent = false;

	virtual protected void OnAwake() { }
}