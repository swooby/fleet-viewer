using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.EventSystems;

namespace FleetVieweR
{
    public class Utils
    {
        public const bool VERBOSE_LOG = false;

        private Utils()
        {
        }

        public static string Quote(string value)
        {
            return value == null ? "null" : "\"" + value + "\"";
        }

        public static string ToString(System.Object o)
        {
            String s = o == null ? "null" : o.ToString();
            if (o is string)
            {
                s = Quote(s);
            }
            else
            {
                if (o != null && s == "")
                {
                    s = o.GetType().Name;
                }
            }
            return s;
        }

        public static string ToString<K,V>(IDictionary<K,V> values)
        {
            if (values == null)
            {
                return "null";
            }

            StringBuilder sb = new StringBuilder();
            sb.Append('{');
            int i = 0;
            foreach (KeyValuePair<K,V> entry in values)
            {
                K key = entry.Key;
                V value = entry.Value;

                if (i > 0)
                {
                    sb.Append(", ");
                }
                sb.Append(ToString(key)).Append(':').Append(ToString(value));
                i++;
            }
            sb.Append('}');
            return sb.ToString();
        }

        public static string ToString<T>(IList<T> values)
        {
            if (values == null)
            {
                return "null";
            }

            StringBuilder sb = new StringBuilder();
            sb.Append('[');
            int i = 0;
            foreach (T temp in values)
            {
                if (i > 0)
                {
                    sb.Append(", ");
                }
                sb.Append(ToString(temp));
                i++;
            }
            sb.Append(']');
            return sb.ToString();
        }

        public static string ToString<T>(T[] values)
        {
            if (values == null)
            {
                return "null";
            }

            StringBuilder sb = new StringBuilder();
            sb.Append('[');
            int i = 0;
            foreach (T temp in values)
            {
                if (i > 0)
                {
                    sb.Append(", ");
                }
                sb.Append(ToString(temp));
                i++;
            }
            sb.Append(']');
            return sb.ToString();
        }

        public static string ToString(Bounds bounds)
        {
            return bounds + ", Size: " + bounds.size + ", Min: " + bounds.min + ", Max: " + bounds.max;
        }

        public static string ToString(RaycastResult raycastResult)
        {
            return String.Format("{{gameObject:{0}}}", raycastResult.gameObject);
        }

        public static string ToString(TimeSpan duration)
        {
            string durationString = "";

            if (duration.Days > 0)
            {
                durationString += duration.Days + "d";
            }

            if (duration.Hours > 0)
            {
                if (durationString != "")
                {
                    durationString += ":";
                }
                durationString += duration.Hours + "h";
            }

            if (duration.Minutes > 0)
            {
                if (durationString != "")
                {
                    durationString += ":";
                }
                durationString += duration.Minutes + "m";
            }

            if (duration.Seconds > 0)
            {
                if (durationString != "")
                {
                    durationString += ":";
                }
                durationString += duration.Seconds;
            }
            else
            {
                durationString += "0";
            }

            durationString += "." + duration.Milliseconds;

            durationString += "s";

            return durationString;
        }

        public static Vector3 ToVector3(string value)
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
            return CalculateBounds(go != null ? go.transform : null, unrotate);
        }

        public static Bounds CalculateBounds(Transform transform, bool unrotate = false)
        {
            if (transform == null)
            {
                return new Bounds();
            }

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

        public static void DisableRenderers(GameObject go, bool disable)
        {
            DisableRenderers(go != null ? go.transform : null, disable);
        }

        public static void DisableRenderers(Transform parent, bool disable)
        {
            if (parent == null)
            {
                return;
            }

            foreach (Transform child in parent.transform)
            {
                DisableRenderers(child, disable);
            }
            parent.gameObject.GetComponent<Renderer>().enabled = !disable;
        }

        public static void ActivateLodRenderers(GameObject lodRoot)
        {
            LODGroup lodGroup = lodRoot.GetComponent<LODGroup>();
            lodGroup.RecalculateBounds();
        }

        public static void NormalizeRotation(GameObject model, Vector3 rotation)
        {
            Debug.Log("Utils.NormalizeRotation(model, rotation:" + rotation + ")");

            Transform transform = model.transform;

            transform.Rotate(rotation, Space.Self);

            if (VERBOSE_LOG)
            {
                Bounds bounds = CalculateBounds(transform);
                Debug.LogError("Utils.NormalizeRotation: AFTER ROTATE transform.position == " + transform.position);
                Debug.LogError("Utils.NormalizeRotation: AFTER ROTATE transform.rotation == " + transform.rotation);
                Debug.LogError("Utils.NormalizeRotation: AFTER ROTATE bounds == " + ToString(bounds));
            }
        }

        public static void NormalizeScale(GameObject model, Vector3 axis, float lengthMeters)
        {
            Debug.Log("Utils.NormalizeScale(model, axis:" + axis + ", lengthMeters:" + lengthMeters + ")");

            Transform transform = model.transform;

            Bounds bounds = CalculateBounds(transform);

            float boundsLength;
            if (axis == Vector3.forward || axis == Vector3.back)
            {
                boundsLength = bounds.size.z;
            }
            else if (axis == Vector3.right || axis == Vector3.left)
            {
                boundsLength = bounds.size.x;
            }
            else if (axis == Vector3.up || axis == Vector3.down)
            {
                boundsLength = bounds.size.y;
            }
            else
            {
                throw new ArgumentException("axis must be Vector3.forward, Vector3.back, Vector3.right, Vector3.left, Vector3.up, or Vector3.down");
            }

            float scale = lengthMeters / boundsLength;
            if (VERBOSE_LOG)
            {
                Debug.LogError("Utils.NormalizeScale: scale == " + scale);
            }

            transform.localScale = new Vector3(scale, scale, scale);// * 1000;
            if (VERBOSE_LOG)
            {
                bounds = CalculateBounds(transform);
                Debug.LogError("Utils.NormalizeScale: AFTER SCALE bounds == " + ToString(bounds));

                float finalLengthMeters;
                if (axis == Vector3.forward || axis == Vector3.back)
                {
                    finalLengthMeters = bounds.size.z;
                }
                else if (axis == Vector3.right || axis == Vector3.left)
                {
                    finalLengthMeters = bounds.size.x;
                }
                else if (axis == Vector3.up || axis == Vector3.down)
                {
                    finalLengthMeters = bounds.size.y;
                }
                else
                {
                    throw new ArgumentException("axis must be Vector3.forward, Vector3.back, Vector3.right, Vector3.left, Vector3.up, or Vector3.down");
                }
                Debug.LogError("Utils.NormalizeScale: finalLengthMeters == " + finalLengthMeters);
            }
        }

        public static void NormalizePosition(GameObject model)
        {
            Debug.Log("Utils.NormalizePosition(model)");

            Transform transform = model.transform;

            Bounds bounds = CalculateBounds(transform);

            if (VERBOSE_LOG)
            {
                Debug.LogError("Utils.NormalizePosition: BEFORE POSITION transform.position == " + transform.position);
                Debug.LogError("Utils.NormalizePosition: BEFORE POSITION transform.rotation == " + transform.rotation);
                Debug.LogError("Utils.NormalizePosition: BEFORE POSITION bounds == " + ToString(bounds));
            }

            if (bounds.center == Vector3.zero)
            {
                return;
            }

            Vector3 translation = new Vector3(
                transform.position.x - bounds.min.x,
                transform.position.y - bounds.min.y,
                transform.position.z - bounds.max.z);
            transform.Translate(translation, Space.World);

            if (VERBOSE_LOG)
            {
                bounds = CalculateBounds(transform);
                Debug.LogError("Utils.NormalizePosition: AFTER POSITION transform.position == " + transform.position);
                Debug.LogError("Utils.NormalizePosition: AFTER POSITION transform.rotation == " + transform.rotation);
                Debug.LogError("Utils.NormalizePosition: AFTER POSITION bounds == " + ToString(bounds));
            }
        }

        public static GameObject PrimitiveCubeMesh()
        {
            Vector3[] vertices = {
            new Vector3 (0, 0, 0),
            new Vector3 (1, 0, 0),
            new Vector3 (1, 1, 0),
            new Vector3 (0, 1, 0),
            new Vector3 (0, 1, 1),
            new Vector3 (1, 1, 1),
            new Vector3 (1, 0, 1),
            new Vector3 (0, 0, 1),
        };
            int[] triangles = {
            0, 2, 1, //face front
            0, 3, 2,
            2, 3, 4, //face top
            2, 4, 5,
            1, 2, 5, //face right
            1, 5, 6,
            0, 7, 4, //face left
            0, 4, 3,
            5, 4, 7, //face back
            5, 7, 6,
            0, 6, 7, //face bottom
            0, 1, 6
        };

            Mesh mesh = new Mesh()
            {
                vertices = vertices,
                triangles = triangles
            };
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();

            GameObject gameObject = new GameObject();

            MeshRenderer mr = gameObject.AddComponent<MeshRenderer>();
            Shader shader = Shader.Find("Standard");
            Material material = new Material(shader);
            mr.material = material;

            MeshFilter mf = gameObject.AddComponent<MeshFilter>();
            mf.mesh = mesh;

            return gameObject;
        }
    }
}