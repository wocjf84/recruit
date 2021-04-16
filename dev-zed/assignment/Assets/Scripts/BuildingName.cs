using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingName : MonoBehaviour
{
	public TextMesh shadow;
	public TextMesh text;

	public Vector3 localPosition { get => transform.localPosition; set => transform.localPosition = value; }

	public void SetText(string name)
	{
		name = name.Replace("동", "");
		shadow.text = name;
		text.text = name;
		text.gameObject.SetActive(false);
	}


	public void Flip()
	{
		var scale = transform.localScale;
		scale.x *= -1;
		transform.localScale = scale;
	}
}
