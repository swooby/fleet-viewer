using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;
using UnityEngine;
using OpenCTM;

public class FleetSceneManager : MonoBehaviour
{
    public const bool RESET_SETTINGS = false;
    // TODO:(pv) Define this per game system?
    public const string DEFAULT_MODEL_KEY = "Nox";

    #region DATA

    [Serializable]
    private class AppSettings
    {
        public string SystemName;
        public List<ModelSettings> ModelSettings = new List<ModelSettings>();

    }

	[Serializable]
	private class Vector3Serializable
    {
		public float x;
		public float y;
		public float z;
	}

	[Serializable]
	private class QuaternionSerializable
	{
		public float x;
		public float y;
		public float z;
		public float w;
	}

	[Serializable]
    private class ModelSettings
    {
        public string Key;
        public Vector3Serializable Position;
        public QuaternionSerializable Rotation;
    }

    #endregion DATA

    private string AppSettingsPath;

    private AppSettings Settings;

    public SortedDictionary<string, ModelInfo> ModelInfos { get; private set; }

    void Start()
    {
		Input.backButtonLeavesApp = true;

		AppSettingsPath = Application.persistentDataPath + "/settings.fvr";
        Debug.Log("Start: AppSettingsPath == " + Utils.Quote(AppSettingsPath));

        if (RESET_SETTINGS)
        {
            File.Delete(AppSettingsPath);
        }

        LoadAppSettings(true);

        LoadSystem(Settings.SystemName);

        foreach(ModelSettings modelSettings in Settings.ModelSettings)
        {
            LoadModel(modelSettings);
		}
    }

	void Update()
	{
		// Exit when (X) is tapped.
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			Application.Quit();
		}
	}

	private void LoadAppSettings(bool handleException)
    {
        Debug.Log("LoadAppSettings(handleException:" + handleException + ")");
		try
        {
            using (FileStream file = File.Open(AppSettingsPath, FileMode.Open))
            {
                BinaryFormatter bf = new BinaryFormatter();
                Settings = (AppSettings)bf.Deserialize(file);
            }
        }
        catch(Exception e)
        {
            Debug.LogWarning("LoadAppSettings: " + e);

            if (handleException)
            {
                Settings = new AppSettings();
                Settings.SystemName = "Star Citizen";

                ModelSettings modelSettings = new ModelSettings();
                modelSettings.Key = DEFAULT_MODEL_KEY;
                Settings.ModelSettings.Add(modelSettings);

                try
                {
                    SaveAppSettings();
					LoadAppSettings(false);
				}
                catch
                {
                    throw;
                }
            }
		}
    }

    private void SaveAppSettings()
    {
		Debug.Log("SaveAppSettings()");
        try
        {
            using (FileStream file = File.Create(AppSettingsPath))
            {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(file, Settings);
            }
        }
        catch(Exception e)
        {
			Debug.LogWarning("SaveAppSettings: " + e);
		}
	}

    private void LoadSystem(string systemName)
    {
        Debug.Log("LoadSystem(systemName:" + Utils.Quote(systemName) + ")");

        SystemInfo system;
        SortedDictionary<string, SystemInfo> systems = new SortedDictionary<string, SystemInfo>(StringComparer.OrdinalIgnoreCase);
        CSVReader.Read<SystemInfo>("Systems", (dictionary) =>
        {
            system = new SystemInfo(dictionary);
            systems[system.Name] = system;
            return null;
        });

        systems.TryGetValue(systemName, out system);
        if (system == null)
        {
            Debug.LogError("LoadSystem: Failed to load systemName == " + Utils.Quote(systemName));
        }

        string configPath = system.ConfigPath;

        LoadModelInfos(configPath);
    }

    public void LoadModelInfos(string configPath)
    {
		Debug.Log("LoadModelInfos(configPath:" + Utils.Quote(configPath) + ")");

		ModelInfos = new SortedDictionary<string, ModelInfo>(StringComparer.OrdinalIgnoreCase);
        CSVReader.Read<ModelInfo>(configPath, (dictionary) =>
        {
            ModelInfo modelInfo = new ModelInfo(dictionary);
            ModelInfos[modelInfo.Name] = modelInfo;
            return null;
        });
        Debug.Log("LoadModelInfos: ModelInfos.Count == " + ModelInfos.Count);

        /*
        GameObject modelInfosDropdown = GameObject.Find("/Player/Main Camera/Overlay Canvas/ModelInfosDropdown");
        if (modelInfosDropdown == null)
        {
            Debug.LogWarning("LoadModelInfos: modelInfosDropdown == null; ignoring");
            return;
        }

        GvrDropdown dropdown = modelInfosDropdown.GetComponent<GvrDropdown>();
        if (dropdown == null)
        {
            Debug.LogWarning("LoadModelInfos: dropdown == null; ignoring");
            return;
        }

        List<Dropdown.OptionData> optionDatas = new List<Dropdown.OptionData>();
        foreach (ModelInfo modelInfo in ModelInfos.Values)
        {
            if (modelInfo == null || modelInfo.ModelPathRemote == null)
            {
                continue;
            }

            Dropdown.OptionData optionData = new Dropdown.OptionData()
            {
                text = modelInfo.Name
            };
            optionDatas.Add(optionData);
        }

        dropdown.ClearOptions();
        dropdown.AddOptions(optionDatas);
        dropdown.value = 0;

        Debug.Log("LoadModelInfos: dropdown.options.Count == " + dropdown.options.Count);
        */
    }

    private GameObject LoadModel(ModelSettings modelSettings)
    {
		string modelKey = modelSettings.Key;

        GameObject model = LoadModel(modelKey);

        /*
		// TODO:(pv) Auto-arrange/position according to scale and previously loaded models...

		Vector3 modelPosition = modelSettings.Position;
        if (modelPosition != null)
        {
            model.transform.position = modelPosition;
        }

        Quaternion modelRotation = modelSettings.Rotation;
        if (modelRotation != null)
        {
            model.transform.rotation = modelRotation;
        }

		modelSettings.Position = modelPosition;
		modelSettings.Rotation = modelRotation;
        */

        return model;
	}

	private GameObject LoadModel(string modelKey)
	{
		ModelInfo modelInfo;
		if (!ModelInfos.TryGetValue(modelKey, out modelInfo) || modelInfo == null)
		{
            Debug.LogError("LoadScalePositionModel: Failed to load modelKey == " + Utils.Quote(modelKey));
			return null;
		}

		float modelLengthMeters = modelInfo.LengthMeters;
		Debug.Log("LoadScalePositionModel: modelLengthMeters == " + modelLengthMeters);
		string modelPathLocal = modelInfo.ModelPathLocal;
		Debug.Log("LoadScalePositionModel: modelPathLocal == " + modelPathLocal);
		Vector3 modelRotation = modelInfo.ModelRotation;
		Debug.Log("LoadScalePositionModel: modelRotation == " + modelRotation);

        GameObject go = modelInfo.GameObject;//CTMReader.Read(modelPathLocal);

		Transform goTransform = go.transform;

		Bounds goBounds = CalculateBounds(go);
		//Debug.LogError("LoadScalePositionModel: BEFORE goBounds == " + goBounds);
		float scale = modelLengthMeters / (goBounds.extents.z * 2);
		//Debug.LogError("LoadScalePositionModel: scale == " + scale);
		goTransform.localScale = new Vector3(scale, scale, scale);
		goBounds = CalculateBounds(go);
		//Debug.LogError("LoadScalePositionModel: AFTER goBounds == " + goBounds);

		//float goLengthMeters = goBounds.extents.z * 2;
		//Debug.LogError("LoadScalePositionModel: goLengthMeters == " + goLengthMeters);

		goTransform.Rotate(modelRotation);

		return go;
	}
	private static Bounds CalculateBounds(GameObject go)
	{
		Bounds bounds;

		Transform transform = go.transform;
		Quaternion currentRotation = transform.rotation;
		{
			transform.rotation = Quaternion.Euler(0f, 0f, 0f);

			bounds = new Bounds(transform.position, Vector3.zero);
			foreach (Renderer renderer in go.GetComponentsInChildren<Renderer>())
			{
				bounds.Encapsulate(renderer.bounds);
			}

		}
		transform.rotation = currentRotation;

		return bounds;
	}
}
