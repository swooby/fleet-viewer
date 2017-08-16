using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class ModelInfo
{
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

	private GameObject gameObject;

	public GameObject GameObject
	{
		get
		{
			if (gameObject == null)
			{
				string modelPathLocal = ModelPathLocal;
				Debug.Log("ModelInfo.GameObject: modelPathLocal == " + modelPathLocal);
				float modelLengthMeters = LengthMeters;
				Debug.Log("ModelInfo.GameObject: modelLengthMeters == " + modelLengthMeters);
				Vector3 modelRotation = ModelRotation;
				Debug.Log("ModelInfo.GameObject: modelRotation == " + modelRotation);

                gameObject = CTMReader.Read(ModelPathLocal);

				Transform transform = gameObject.transform;

				Bounds bounds = Utils.CalculateBounds(gameObject);
				//Debug.LogError("ModelInfo.GameObject: BEFORE goBounds == " + goBounds);
				float scale = LengthMeters / (bounds.extents.z * 2);
				//Debug.LogError("ModelInfo.GameObject: scale == " + scale);
				transform.localScale = new Vector3(scale, scale, scale);// * 1000;
				//goBounds = CalculateBounds(go);
				//Debug.LogError("ModelInfo.GameObject: AFTER goBounds == " + goBounds);

				//float goLengthMeters = goBounds.extents.z * 2;
				//Debug.LogError("ModelInfo.GameObject: goLengthMeters == " + goLengthMeters);

                transform.Rotate(ModelRotation);

			}
			return gameObject;
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
		ModelRotation = StringToVector3(modelRotation);
	}

	private Vector3 StringToVector3(String value)
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
}
