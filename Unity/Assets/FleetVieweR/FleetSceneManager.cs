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
    public const bool GL_WIREFRAME = false;

    public const bool RESET_SETTINGS = false;

    private AppSettings AppSettings;

    private SortedDictionary<string, ModelInfo> ModelInfos = new SortedDictionary<string, ModelInfo>(StringComparer.OrdinalIgnoreCase);


    private GameObject FleetRoot;
    private GameObject FleetPlanes;
    private GameObject FleetModels;
    private GameObject Player;
    private GameObject Respawn;


    void Start()
    {
        Input.backButtonLeavesApp = true;

        FleetRoot = GameObject.FindGameObjectWithTag("FleetModels");
        FleetPlanes = GameObject.FindGameObjectWithTag("FleetPlanes");
        FleetModels = GameObject.FindGameObjectWithTag("FleetModels");
        Player = GameObject.FindGameObjectWithTag("Player");
        Respawn = GameObject.FindGameObjectWithTag("Respawn");

        if (RESET_SETTINGS)
        {
            AppSettings.Reset();
        }

        AppSettings = AppSettings.Load();

        SystemName = AppSettings.SystemName;

        if (true)
        {
            //
            // The simplest way to load; usually intended for testing purposes only...
            //

            for (int i = 0; i < 2; i++)
            {
                AddNewModel("Nox");
            }
        }
        else
        {
            foreach (ModelSettings modelSettings in AppSettings.ModelSettings)
            {
                GameObject go = AddSavedModel(modelSettings);
            }
        }

        RepositionPlayerToViewFleet();
    }

    void OnPreRender()
    {
        if (GL_WIREFRAME)
        {
            GL.wireframe = true;
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

    private void OnDrawGizmos()
    {
        DebugBoxFleetModels();
    }

    void OnPostRender()
    {
        if (GL_WIREFRAME)
        {
            GL.wireframe = false;
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
                Debug.LogError("SystemName: Failed to load systemName == " + Utils.Quote(value));
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
        //Debug.Log("LoadModelInfos: ModelInfos.Count == " + ModelInfos.Count);

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
            if (modelInfo == null || modelInfo.ModelPathLocal == null)
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

    private GameObject LoadModel(string modelKey)
    {
        ModelInfo modelInfo;
        if (!ModelInfos.TryGetValue(modelKey, out modelInfo) || modelInfo == null)
        {
            Debug.LogError("LoadModel: Failed to load modelKey == " + Utils.Quote(modelKey));
            return null;
        }

        return modelInfo.Model;
    }

    private GameObject AddSavedModel(ModelSettings modelSettings, bool repositionPlayerToViewFleet = false)
    {
        string modelKey = modelSettings.Key;

        GameObject go = LoadModel(modelKey);

        //go.transform.position = modelSettings.Position;
        //go.transform.rotation = modelSettings.Rotation;

        return go;
    }

    private GameObject AddNewModel(string modelKey, bool repositionPlayerToViewFleet = false)
    {
        Debug.Log("AddNewModel(modelKey:" + Utils.Quote(modelKey) +
                  ", repositionPlayerToViewFleet:" + repositionPlayerToViewFleet + ")");

        GameObject model = LoadModel(modelKey);

        Transform modelTransform = model.transform;
        Bounds modelBounds = Utils.CalculateBounds(modelTransform, new Bounds());
        Debug.LogError("AddNewModel: BEFORE modelBounds == " + modelBounds + ", Size: " + modelBounds.size);

        GameObject fleetPlanes = FleetPlanes;
        Transform fleetPlanesTransform = fleetPlanes.transform;
        Bounds fleetPlanesBounds = Utils.CalculateBounds(fleetPlanesTransform, new Bounds());
        Debug.LogError("AddNewModel: BEFORE fleetPlanesBounds == " + fleetPlanesBounds + ", Size: " + fleetPlanesBounds.size);

        GameObject fleetModels = FleetModels;
        Transform fleetModelsTransform = fleetModels.transform;
        Bounds fleetModelsBounds = Utils.CalculateBounds(fleetModelsTransform, new Bounds());
        Debug.LogError("AddNewModel: BEFORE fleetModelsBounds == " + fleetModelsBounds + ", Size: " + fleetModelsBounds.size);

        modelTransform.SetParent(fleetModelsTransform);

        modelBounds = Utils.CalculateBounds(modelTransform, new Bounds());
        Debug.LogError("AddNewModel: AFTER SetParent modelBounds == " + modelBounds + ", Size: " + modelBounds.size);

        fleetModelsBounds = Utils.CalculateBounds(fleetModelsTransform, new Bounds());
        Debug.LogError("AddNewModel: AFTER SetParent fleetModelsBounds == " + fleetModelsBounds + ", Size: " + fleetModelsBounds.size);

        float scaleX = fleetModelsBounds.size.x / fleetPlanesBounds.size.x;
        float scaleY = fleetModelsBounds.size.y / fleetPlanesBounds.size.y;
        float scaleZ = fleetModelsBounds.size.z / fleetPlanesBounds.size.z;
        Vector3 fleetPlanesScale = new Vector3(scaleX, scaleY, scaleZ);
        fleetPlanesTransform.localScale = fleetPlanesScale;

        if (repositionPlayerToViewFleet)
        {
            RepositionPlayerToViewFleet();
        }

        // TODO:(pv) Auto-arrange/position according to scale and previously loaded models...

        // Calculate width of all loaded non-positioned models
        // Evenly reposition all loaded models

        // TODO:(pv) Save modelSettings...
        //modelSettings.Position = modelPosition;
        //modelSettings.Rotation = modelRotation;

        return model;
    }

    private Dictionary<int, Color> debugColors = new Dictionary<int, Color>();

    private void DebugBoxFleetModels()
    {
        if (FleetModels != null)
        {
            Transform fleetModelsTransform = FleetModels.transform;
            foreach (Transform fleetModelTransform in fleetModelsTransform)
            {
                DebugBox(fleetModelTransform);
            }
            DebugBox(fleetModelsTransform);
        }
    }

    private void DebugBox(Transform transform)
    {
        Color savedColor = Gizmos.color;

        int instanceId = transform.GetInstanceID();

        Color color;
        if (!debugColors.TryGetValue(instanceId, out color))
        {
            color = UnityEngine.Random.ColorHSV();

            debugColors[instanceId] = color;
        }
        Gizmos.color = color;

        Bounds bounds = Utils.CalculateBounds(transform, new Bounds());

        Gizmos.DrawWireCube(bounds.center, bounds.size);

        Gizmos.color = savedColor;
    }

    private void RepositionPlayerToViewFleet()
    {
        GameObject fleetRoot = FleetRoot;
        Bounds fleetRootBounds = Utils.CalculateBounds(fleetRoot, new Bounds());
        Debug.LogError("RepositionPlayerToViewFleet: fleetRootBounds == " + fleetRootBounds);
        Debug.LogError("RepositionPlayerToViewFleet: fleetRootBounds.max == " + fleetRootBounds.max);
        Debug.LogError("RepositionPlayerToViewFleet: fleetRootBounds.min == " + fleetRootBounds.min);
        Debug.LogError("RepositionPlayerToViewFleet: fleetRootBounds.size == " + fleetRootBounds.size);

        Vector3 position = new Vector3(fleetRootBounds.center.x,
                                       -(float)(fleetRootBounds.extents.y * 0.4),
                                       -(float)(fleetRootBounds.extents.z * 1.5));

        // TODO:(pv) Improve this zoom in/out...
        Debug.LogError("RepositionPlayerToViewFleet: BEFORE Player.transform.position == " + Player.transform.position);
        Player.transform.position = position;
        Debug.LogError("RepositionPlayerToViewFleet: AFTER Player.transform.position == " + Player.transform.position);

        position.y -= 1;

        Respawn.transform.position = position;
    }
}
