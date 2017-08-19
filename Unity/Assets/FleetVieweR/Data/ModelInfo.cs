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

                // We must rotate first to ensure Z starts off as length
				ModelCorrectRotation(model, ModelRotation);

				ModelSetLengthMeters(model, LengthMeters);

				ModelCorrectPosition(model);

                cachedModel = model;
            }
            else
            {
                model = UnityEngine.Object.Instantiate(cachedModel);
            }

            Debug.LogError("ModelInfo.GameObject: gameObject.transform.position == " + model.transform.position);
            Debug.LogError("ModelInfo.GameObject: gameObject.transform.rotation == " + model.transform.rotation);

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

	private static void ModelSetLengthMeters(GameObject model, float lengthMeters)
	{
		Debug.Log("ModelInfo.ModelSetLength(model, lengthMeters:" + lengthMeters + ")");

		Transform transform = model.transform;

		Bounds bounds = Utils.CalculateBounds(transform);

		float scale = lengthMeters / bounds.size.z;
		if (VERBOSE_LOG)
		{
			Debug.LogError("ModelInfo.ModelSetLengthMeters: scale == " + scale);
		}

		transform.localScale = new Vector3(scale, scale, scale);// * 1000;

		if (VERBOSE_LOG)
		{
			bounds = Utils.CalculateBounds(transform);
			Debug.LogError("ModelInfo.ModelSetLengthMeters: AFTER SCALE bounds == " + bounds + ", Size: " + bounds.size);

			float finalLengthMeters = bounds.size.z;
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
            Debug.LogError("ModelInfo.ModelCorrectPosition: BEFORE TRANSLATE bounds == " + bounds + ", Size: " + bounds.size);
        }

        Vector3 translate = new Vector3(bounds.extents.x,
                                         bounds.extents.y,
                                        -bounds.extents.z);
        
        translate -= bounds.center;

        transform.Translate(translate);

        if (VERBOSE_LOG)
        {
			bounds = Utils.CalculateBounds(transform);
			Debug.LogError("ModelInfo.ModelCorrectPosition: AFTER TRANSLATE bounds == " + bounds + ", Size: " + bounds.size);
        }
    }

	private static void ModelCorrectRotation(GameObject model, Vector3 rotation)
	{
		Debug.Log("ModelInfo.ModelCorrectRotation(model, rotation:" + rotation + ")");

		Transform transform = model.transform;

		transform.Rotate(rotation);

		if (VERBOSE_LOG)
		{
			Bounds bounds = Utils.CalculateBounds(transform);
			Debug.LogError("ModelInfo.ModelCorrectRotation: AFTER ROTATE transform.position == " + transform.position);
			Debug.LogError("ModelInfo.ModelCorrectRotation: AFTER ROTATE transform.rotation == " + transform.rotation);
			Debug.LogError("ModelInfo.ModelCorrectRotation: AFTER ROTATE bounds == " + Utils.ToString(bounds));
		}
	}
}
