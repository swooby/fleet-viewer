using System;
using UnityEngine;

public class Utils
{
    private Utils()
    {
    }

    public static String Quote(String value)
    {
        return value == null ? "null" : "\"" + value + "\"";
    }

	public static Bounds CalculateBounds(GameObject go, Bounds bounds)
	{
		Transform transform = go.transform;

		Quaternion savedRotation = transform.rotation;
		{
            transform.rotation = Quaternion.identity;

			foreach (Renderer renderer in go.GetComponentsInChildren<Renderer>())
			{
				bounds.Encapsulate(renderer.bounds);
			}

		}

		transform.rotation = savedRotation;

		return bounds;
	}
}
