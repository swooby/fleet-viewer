using System;
using System.Collections.Generic;
using UnityEngine;

public class ModelInfo
{
    public const bool VERBOSE_LOG = false;

    public const string FIELD_NAME = "Name";
    public const string FIELD_LAST_CHECKED = "Last Checked";
    public const string FIELD_LENGTH = "Length";
    public const string FIELD_BEAM = "Beam";
    public const string FIELD_HEIGHT = "Height";
    public const string FIELD_STORE_URL = "Store URL";
    public const string FIELD_MODEL_PATH_REMOTE = "Model Path Remote";
    public const string FIELD_MODEL_PATH_LOCAL = "Model Path Local";
    public const string FIELD_MODEL_ROTATION = "Model Rotation";

    public string Name { get; private set; }
    public DateTime LastChecked { get; private set; }
    public float LengthMeters { get; private set; }
    public float BeamMeters { get; private set; }
    public float HeightMeters { get; private set; }
    public Uri StoreUrl { get; private set; }
    public Uri ModelPathRemote { get; private set; }
    public string ModelPathLocal { get; private set; }
    public Vector3 ModelRotation { get; private set; }

    public ModelInfo(Dictionary<string, string> dictionary)
    {
        Name = dictionary[FIELD_NAME];

        try
        {
            LastChecked = DateTime.Parse(dictionary[FIELD_LAST_CHECKED]);
        }
        catch (FormatException)
        {
            LastChecked = DateTime.MinValue;
        }

        try
        {
            LengthMeters = float.Parse(dictionary[FIELD_LENGTH]);
        }
        catch (FormatException)
        {
            LengthMeters = float.NaN;
        }

        try
        {
            BeamMeters = float.Parse(dictionary[FIELD_BEAM]);
        }
        catch (FormatException)
        {
            BeamMeters = float.NaN;
        }

        try
        {
            HeightMeters = float.Parse(dictionary[FIELD_HEIGHT]);
        }
        catch (FormatException)
        {
            HeightMeters = float.NaN;
        }

        try
        {
            StoreUrl = new Uri(dictionary[FIELD_STORE_URL]);
        }
        catch (FormatException)
        {
            StoreUrl = null;
        }

        try
        {
            ModelPathRemote = new Uri(dictionary[FIELD_MODEL_PATH_REMOTE]);
        }
        catch (FormatException)
        {
            ModelPathRemote = null;
        }

        ModelPathLocal = dictionary[FIELD_MODEL_PATH_LOCAL];

        String modelRotation;
        dictionary.TryGetValue(FIELD_MODEL_ROTATION, out modelRotation);
        ModelRotation = Utils.StringToVector3(modelRotation);
    }

    private GameObject cachedModel;

    public delegate void LoadModelCallback(GameObject model);

    private List<LoadModelCallback> callbacks = new List<LoadModelCallback>();

    public void LoadModelAsync(LoadModelCallback callback)
    {
		Debug.Log("+ModelInfo.LoadModelAsync(callback:" + callback + ")");

		if (cachedModel == null)
        {
            lock (callbacks)
            {
                if (cachedModel == null)
                {
                    callbacks.Add(callback);

                    if (callbacks.Count == 1)
                    {
                        if (VERBOSE_LOG)
                        {
                            Debug.LogError("ModelInfo.LoadModelAsync: Non-Cached Load: ModelFactory.LoadModelAsync(...)");
                        }
                        ModelFactory.LoadModelAsync(ModelPathLocal, (model) =>
                        {
                            if (VERBOSE_LOG)
                            {
                                Debug.LogError("ModelInfo.LoadModelAsync: ModelFactory.LoadAsync completed");
                            }
							if (model == null)
                            {
                                return;
                            }

                            model.name = Name;

                            LoadModelCallback tempCallback;
                            lock (callbacks)
                            {
                                while (callbacks.Count > 0)
                                {
                                    model = OnModelLoaded(model);

                                    tempCallback = callbacks[0];
                                    tempCallback(model);
                                    callbacks.RemoveAt(0);
                                }
                            }
                        });
                    }
                    else
                    {
						if (VERBOSE_LOG)
						{
							Debug.LogError("ModelInfo.LoadModelAsync: ModelFactory.LoadModelAsync already in progress");
						}
					}
                }
                else
                {
					if (VERBOSE_LOG)
					{
						Debug.LogError("ModelInfo.LoadModelAsync: Handling edge condition");
					}
					OnModelLoaded(cachedModel);
                }
            }
        }
        else
        {
            if (VERBOSE_LOG)
            {
                Debug.LogError("ModelInfo.LoadModelAsync: Cached Load");
            }
            OnModelLoaded(cachedModel);
        }

		Debug.Log("-ModelInfo.LoadModelAsync(callback:" + callback + ")");
	}

    private GameObject OnModelLoaded(GameObject model)
    {
        Debug.Log("+ModelInfo.OnModelLoaded(model:" + model + ")");

        if (cachedModel == null)
        {
            cachedModel = model;
        }
        else
        {
            model = UnityEngine.Object.Instantiate(cachedModel);

            model.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            model.transform.localScale = Vector3.one;
        }

        if (VERBOSE_LOG)
        {
            Transform transform = model.transform;
            Bounds bounds = Utils.CalculateBounds(transform);
            Debug.LogError("ModelInfo.ModelLoad: AFTER LOAD transform.position == " + transform.position);
            Debug.LogError("ModelInfo.ModelLoad: AFTER LOAD transform.rotation == " + transform.rotation);
            Debug.LogError("ModelInfo.ModelLoad: AFTER LOAD bounds == " + Utils.ToString(bounds));
        }

        Normalize(model);

		Debug.Log("-ModelInfo.OnModelLoaded(model:" + model + ")");

		return model;
    }

    private GameObject Normalize(GameObject model)
    {
        // 1) Rotate so the length is along Z and bow faces -Z
        NormalizeRotation(model, ModelRotation);

        // 2) Uniformly scale X/Y/Z so that bounds.size.z is the expected length
        NormalizeScale(model, Vector3.forward, LengthMeters);

        // 3) Use the scaled length to position the stern at Z == 0
        NormalizePosition(model);

        // These are affecting the scaling and positioning when loading multiple ships
        //Decorate(model);

        if (VERBOSE_LOG)
        {
            Debug.LogError("ModelInfo.Normalize: gameObject.transform.position == " + model.transform.position);
            Debug.LogError("ModelInfo.Normalize: gameObject.transform.rotation == " + model.transform.rotation);
        }

        return model;
    }

    private static void NormalizeRotation(GameObject model, Vector3 rotation)
    {
        Debug.Log("ModelInfo.NormalizeRotation(model, rotation:" + rotation + ")");

        Transform transform = model.transform;

        transform.Rotate(rotation, Space.Self);

        if (VERBOSE_LOG)
        {
            Bounds bounds = Utils.CalculateBounds(transform);
            Debug.LogError("ModelInfo.NormalizeRotation: AFTER ROTATE transform.position == " + transform.position);
            Debug.LogError("ModelInfo.NormalizeRotation: AFTER ROTATE transform.rotation == " + transform.rotation);
            Debug.LogError("ModelInfo.NormalizeRotation: AFTER ROTATE bounds == " + Utils.ToString(bounds));
        }
    }

    private static void NormalizeScale(GameObject model, Vector3 axis, float lengthMeters)
    {
        Debug.Log("ModelInfo.NormalizeScale(model, axis:" + axis + ", lengthMeters:" + lengthMeters + ")");

        Transform transform = model.transform;

        Bounds bounds = Utils.CalculateBounds(transform);

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
            Debug.LogError("ModelInfo.NormalizeScale: scale == " + scale);
        }

        transform.localScale = new Vector3(scale, scale, scale);// * 1000;
        if (VERBOSE_LOG)
        {
            bounds = Utils.CalculateBounds(transform);
            Debug.LogError("ModelInfo.NormalizeScale: AFTER SCALE bounds == " + Utils.ToString(bounds));

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
            Debug.LogError("ModelInfo.NormalizeScale: finalLengthMeters == " + finalLengthMeters);
        }
    }

    private static void NormalizePosition(GameObject model)
    {
        Debug.Log("ModelInfo.NormalizePosition(model)");

        Transform transform = model.transform;

        Bounds bounds = Utils.CalculateBounds(transform);

        if (VERBOSE_LOG)
        {
            Debug.LogError("ModelInfo.NormalizePosition: BEFORE POSITION transform.position == " + transform.position);
            Debug.LogError("ModelInfo.NormalizePosition: BEFORE POSITION transform.rotation == " + transform.rotation);
            Debug.LogError("ModelInfo.NormalizePosition: BEFORE POSITION bounds == " + Utils.ToString(bounds));
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
            bounds = Utils.CalculateBounds(transform);
            Debug.LogError("ModelInfo.NormalizePosition: AFTER POSITION transform.position == " + transform.position);
            Debug.LogError("ModelInfo.NormalizePosition: AFTER POSITION transform.rotation == " + transform.rotation);
            Debug.LogError("ModelInfo.NormalizePosition: AFTER POSITION bounds == " + Utils.ToString(bounds));
        }
    }

    private static void Decorate(GameObject model)
    {
        //Debug.Log("ModelInfo.Decorate(model)");

        Transform transform = model.transform;

        Bounds bounds = Utils.CalculateBounds(transform);

        GameObject primitive;
        Transform primitiveTransform;

        primitive = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        primitiveTransform = primitive.transform;
        primitive.name = "position";
        primitiveTransform.position = transform.position;
        primitiveTransform.SetParent(transform);

        primitive = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        primitiveTransform = primitive.transform;
        primitive.name = "center";
        primitiveTransform.position = bounds.center;
        primitiveTransform.SetParent(transform);

        primitive = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        primitiveTransform = primitive.transform;
        primitive.name = "min";
        primitiveTransform.position = bounds.min;
        primitiveTransform.SetParent(transform);

        primitive = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        primitiveTransform = primitive.transform;
        primitive.name = "max";
        primitiveTransform.position = bounds.max;
        primitiveTransform.SetParent(transform);
    }
}
