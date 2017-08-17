using System;
using System.Text.RegularExpressions;
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

    public static Vector3 StringToVector3(String value)
    {
        Vector3 result = Vector3.zero;

        if (value != null)
        {
            Match match = Regex.Match(value, @"(?<X>-?\d?.?\d?),(?<Y>-?\d?.?\d?),(?<Z>-?\d?.?\d?)");
            if (match.Success)
            {
                float x = float.Parse(match.Groups["X"].Value);
                float y = float.Parse(match.Groups["Y"].Value);
                float z = float.Parse(match.Groups["Z"].Value);
                result = new Vector3(x, y, z);
            }
        }

        return result;
    }

    public static Bounds CalculateBounds(GameObject go, Bounds bounds)
    {
        return CalculateBounds(go.transform, bounds);
    }

    public static Bounds CalculateBounds(Transform transform, Bounds bounds)
    {
        Quaternion savedRotation = transform.rotation;

        transform.rotation = Quaternion.identity;

        foreach (Renderer renderer in transform.GetComponentsInChildren<Renderer>())
        {
            bounds.Encapsulate(renderer.bounds);
        }

        transform.rotation = savedRotation;

        return bounds;
    }
}
