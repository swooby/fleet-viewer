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

    private GameObject cachedModel;
    public GameObject Model
    {
        get
        {
            GameObject model;

            if (cachedModel == null)
            {
                model = ModelLoad(Name, ModelPathLocal);

                cachedModel = model;
            }
            else
            {
                model = UnityEngine.Object.Instantiate(cachedModel);

                model.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
                model.transform.localScale = Vector3.one;
            }

            // First rotate so that length is the Z axis
            ModelCorrectRotation(model, ModelRotation);

            // Next set length so that we can reposition
            ModelSetLengthMeters(model, LengthMeters);

            // Reposition now that we have set length on Z axis
            ModelCorrectPosition(model);

            ModelDecorate(model);

            if (VERBOSE_LOG)
            {
                Debug.LogError("ModelInfo.Model: gameObject.transform.position == " + model.transform.position);
                Debug.LogError("ModelInfo.Model: gameObject.transform.rotation == " + model.transform.rotation);
            }

            return model;
        }
    }

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

    private static GameObject ModelLoad(string modelName, string modelPath)
    {
        Debug.Log("ModelInfo.ModelLoad(modelName:" + Utils.Quote(modelName) +
                  ", modelPath:" + Utils.Quote(modelPath) + ")");

        GameObject model = ModelFactory.Get(modelName, modelPath);

        if (VERBOSE_LOG)
        {
            Transform transform = model.transform;
            Bounds bounds = Utils.CalculateBounds(transform);
            Debug.LogError("ModelInfo.ModelLoad: AFTER LOAD transform.position == " + transform.position);
            Debug.LogError("ModelInfo.ModelLoad: AFTER LOAD transform.rotation == " + transform.rotation);
            Debug.LogError("ModelInfo.ModelLoad: AFTER LOAD bounds == " + Utils.ToString(bounds));
        }

        return model;
    }

    private static void ModelCorrectRotation(GameObject model, Vector3 rotation)
    {
        Debug.Log("ModelInfo.ModelCorrectRotation(model, rotation:" + rotation + ")");

        Transform transform = model.transform;

        transform.Rotate(rotation, Space.Self);

        if (VERBOSE_LOG)
        {
            Bounds bounds = Utils.CalculateBounds(transform);
            Debug.LogError("ModelInfo.ModelCorrectRotation: AFTER ROTATE transform.position == " + transform.position);
            Debug.LogError("ModelInfo.ModelCorrectRotation: AFTER ROTATE transform.rotation == " + transform.rotation);
            Debug.LogError("ModelInfo.ModelCorrectRotation: AFTER ROTATE bounds == " + Utils.ToString(bounds));
        }
    }

    private static void ModelSetLengthMeters(GameObject model, float lengthMeters)
    {
        ModelSetLengthMeters(model, lengthMeters, Vector3.forward);
    }

    private static void ModelSetLengthMeters(GameObject model, float lengthMeters, Vector3 axis)
    {
        Debug.Log("ModelInfo.ModelSetLength(model, lengthMeters:" + lengthMeters + ")");

        Transform transform = model.transform;

        Bounds bounds = Utils.CalculateBounds(transform);

        float boundsLength;
        if (axis == Vector3.forward)
        {
            boundsLength = bounds.size.z;
        }
        else if (axis == Vector3.right)
        {
            boundsLength = bounds.size.x;
        }
        else if (axis == Vector3.up)
        {
            boundsLength = bounds.size.y;
        }
        else
        {
            throw new ArgumentException("axis must be Vector3.forward, Vector3.right, or Vector3.up");
        }

        float scale = lengthMeters / boundsLength;
        if (VERBOSE_LOG)
        {
            Debug.LogError("ModelInfo.ModelSetLengthMeters: scale == " + scale);
        }

        transform.localScale = new Vector3(scale, scale, scale);// * 1000;
        if (VERBOSE_LOG)
        {
            bounds = Utils.CalculateBounds(transform);
            Debug.LogError("ModelInfo.ModelSetLengthMeters: AFTER SCALE bounds == " + Utils.ToString(bounds));

            float finalLengthMeters;
            if (axis == Vector3.forward)
            {
                finalLengthMeters = bounds.size.z;
            }
            else if (axis == Vector3.right)
            {
                finalLengthMeters = bounds.size.x;
            }
            else if (axis == Vector3.up)
            {
                finalLengthMeters = bounds.size.y;
            }
            else
            {
                throw new ArgumentException("axis must be Vector3.forward, Vector3.right, or Vector3.up");
            }
            Debug.LogError("ModelInfo.ModelSetLengthMeters: finalLengthMeters == " + finalLengthMeters);
        }
    }

    private static void ModelCorrectPosition(GameObject model)
    {
        Debug.Log("ModelInfo.ModelCorrectPosition(model)");

        Transform transform = model.transform;

        Bounds bounds = Utils.CalculateBounds(transform);

        if (VERBOSE_LOG)
        {
            Debug.LogError("ModelInfo.ModelCorrectRotation: BEFORE POSITION transform.position == " + transform.position);
            Debug.LogError("ModelInfo.ModelCorrectRotation: BEFORE POSITION transform.rotation == " + transform.rotation);
            Debug.LogError("ModelInfo.ModelCorrectPosition: BEFORE POSITION bounds == " + Utils.ToString(bounds));
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
            Debug.LogError("ModelInfo.ModelCorrectRotation: AFTER POSITION transform.position == " + transform.position);
            Debug.LogError("ModelInfo.ModelCorrectRotation: AFTER POSITION transform.rotation == " + transform.rotation);
            Debug.LogError("ModelInfo.ModelCorrectRotation: AFTER POSITION bounds == " + Utils.ToString(bounds));
        }
    }

    private static void ModelDecorate(GameObject model)
    {
        //Debug.Log("ModelInfo.ModelDecorate(model)");

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
