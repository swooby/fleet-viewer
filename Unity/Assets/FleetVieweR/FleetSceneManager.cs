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

    private AppSettings AppSettings;

    private SortedDictionary<string, ModelInfo> ModelInfos = new SortedDictionary<string, ModelInfo>(StringComparer.OrdinalIgnoreCase);

    void Start()
    {
        Input.backButtonLeavesApp = true;

        if (RESET_SETTINGS)
        {
            AppSettings.Reset();
        }

        AppSettings = AppSettings.Load();

        SystemName = AppSettings.SystemName;

        Bounds bounds = new Bounds(transform.position, Vector3.zero);

        foreach (ModelSettings modelSettings in AppSettings.ModelSettings)
        {
            GameObject go = AddModel(modelSettings);

            bounds = Utils.CalculateBounds(go, bounds);
            //Debug.LogError("Start: bounds == " + bounds);
        }
        //Debug.LogError("Start: bounds == " + bounds);
        //Debug.LogError("Start: bounds.max == " + bounds.max);
        //Debug.LogError("Start: bounds.min == " + bounds.min);
        //Debug.LogError("Start: bounds.size == " + bounds.size);
        //Debug.LogError("Start: bounds.extents == " + bounds.extents);

        // TODO:(pv) Zoom to fit all loaded models...
        GameObject player = GameObject.Find("/Player");
        player.transform.position = new Vector3(0,
                                                -(float)(bounds.extents.y * 0.2),
                                                -(float)(bounds.extents.z * 1.2));
    }

    void Update()
    {
        // Exit when (X) is tapped.
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    private string systemName;

    private string SystemName
    {
        get
        {
            return systemName;
        }
        set
        {
            SystemInfo system;
            SortedDictionary<string, SystemInfo> systems = new SortedDictionary<string, SystemInfo>(StringComparer.OrdinalIgnoreCase);
            CSVReader.Read<SystemInfo>("Systems", (dictionary) =>
            {
                system = new SystemInfo(dictionary);
                systems[system.Name] = system;
                return null;
            });

            systems.TryGetValue(value, out system);
            if (system == null)
            {
                Debug.LogError("LoadSystem: Failed to load systemName == " + Utils.Quote(value));
            }

            systemName = value;

            string configPath = system.ConfigPath;

            LoadModelInfos(configPath);
        }
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

    private GameObject AddModel(ModelSettings modelSettings)
    {
        string modelKey = modelSettings.Key;

        GameObject go = LoadModel(modelKey);

        go.transform.position = modelSettings.Position;
        go.transform.rotation = modelSettings.Rotation;

        return go;
    }

    private GameObject AddModel(string modelKey)
    {
        GameObject go = LoadModel(modelKey);

        // TODO:(pv) Auto-arrange/position according to scale and previously loaded models...

        // Calculate width of all loaded non-positioned models
        // Evenly reposition all loaded models

        //modelSettings.Position = modelPosition;

        //modelSettings.Rotation = modelRotation;

        return go;
    }

    private GameObject LoadModel(string modelKey)
    {
        ModelInfo modelInfo;
        if (!ModelInfos.TryGetValue(modelKey, out modelInfo) || modelInfo == null)
        {
            Debug.LogError("LoadScalePositionModel: Failed to load modelKey == " + Utils.Quote(modelKey));
            return null;
        }

        return modelInfo.GameObject;
    }
}
