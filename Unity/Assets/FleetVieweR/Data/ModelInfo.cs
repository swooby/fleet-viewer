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
                Debug.Log("ModelInfo.GameObject: ModelPathLocal == " + ModelPathLocal);
                model = ModelFactory.Get(Name, ModelPathLocal);

                Transform transform = model.transform;

                Debug.Log("ModelInfo.GameObject: LengthMeters == " + LengthMeters);
                Bounds bounds = Utils.CalculateBounds(model, new Bounds(transform.position, Vector3.zero));
                if (VERBOSE_LOG)
                {
                    Debug.LogError("ModelInfo.GameObject: BEFORE SCALE bounds == " + bounds + ", Size: " + bounds.size);
                }
                float scale = LengthMeters / bounds.size.z;
                if (VERBOSE_LOG)
                {
                    Debug.LogError("ModelInfo.GameObject: scale == " + scale);
                }
                transform.localScale = new Vector3(scale, scale, scale);// * 1000;
                if (VERBOSE_LOG)
                {
                    bounds = Utils.CalculateBounds(model, new Bounds(transform.position, Vector3.zero));
                    Debug.LogError("ModelInfo.GameObject: AFTER SCALE bounds == " + bounds + ", Size: " + bounds.size);

                    float finalLengthMeters = bounds.size.z;
                    Debug.LogError("ModelInfo.GameObject: finalLengthMeters == " + finalLengthMeters);
                }

                bounds = Utils.CalculateBounds(model, new Bounds(transform.position, Vector3.zero));
                if (VERBOSE_LOG)
                {
                    Debug.LogError("ModelInfo.GameObject: BEFORE TRANSLATE bounds == " + bounds + ", Size: " + bounds.size);
                }
				Vector3 translate = new Vector3(bounds.extents.x,
												 bounds.extents.y,
												-bounds.extents.z);
                translate -= bounds.center;
                transform.Translate(translate);
                bounds = Utils.CalculateBounds(model, new Bounds(transform.position, Vector3.zero));
                if (VERBOSE_LOG)
                {
                    Debug.LogError("ModelInfo.GameObject: AFTER TRANSLATE bounds == " + bounds + ", Size: " + bounds.size);
                }

                Debug.Log("ModelInfo.GameObject: ModelRotation == " + ModelRotation);
                transform.Rotate(ModelRotation);

                cachedModel = model;
            }
            else
            {
                model = UnityEngine.Object.Instantiate(cachedModel);
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
}
