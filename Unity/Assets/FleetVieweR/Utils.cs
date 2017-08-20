using System.Text.RegularExpressions;
using UnityEngine;

public class Utils
{
    private Utils()
    {
    }

    public static string Quote(string value)
    {
        return value == null ? "null" : "\"" + value + "\"";
    }

    public static string ToString(Bounds bounds)
    {
        return bounds + ", Size: " + bounds.size + ", Min: " + bounds.min + ", Max: " + bounds.max;
    }

    public static Vector3 StringToVector3(string value)
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

    public static Bounds CalculateBounds(GameObject go, bool unrotate = false)
    {
        return CalculateBounds(go.transform, unrotate);
    }

    public static Bounds CalculateBounds(Transform transform, bool unrotate = false)
    {
		Quaternion savedRotation = transform.rotation;

        if (unrotate)
        {
            transform.rotation = Quaternion.identity;
        }

		Bounds bounds = new Bounds(transform.position, Vector3.zero);

        foreach (Renderer renderer in transform.GetComponentsInChildren<Renderer>())
        {
            bounds.Encapsulate(renderer.bounds);
        }

        if (unrotate)
        {
            transform.rotation = savedRotation;
        }

        return bounds;
    }
}
