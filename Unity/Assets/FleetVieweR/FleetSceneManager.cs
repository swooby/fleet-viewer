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

            AddNewModel("Idris-P");
            AddNewModel("Reclaimer");
            AddNewModel("Genesis");
            AddNewModel("Prospector");
            AddNewModel("Terrapin");
            AddNewModel("M50");
            AddNewModel("MPUV Cargo");
            for (int i = 0; i < 2; i++)
            {
                AddNewModel("MPUV Personnel");
            }
            AddNewModel("Dragonfly");
            for (int i = 0; i < 3; i++)
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
        DebugDecorate();
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
        Bounds modelBounds = Utils.CalculateBounds(modelTransform);
        Debug.LogError("AddNewModel: BEFORE modelBounds == " + Utils.ToString(modelBounds));

        GameObject fleetModels = FleetModels;
        Transform fleetModelsTransform = fleetModels.transform;
        Bounds fleetModelsBounds = Utils.CalculateBounds(fleetModelsTransform);
        Debug.LogError("AddNewModel: BEFORE fleetModelsBounds == " + Utils.ToString(fleetModelsBounds));

        modelTransform.SetParent(fleetModelsTransform);

        Vector3 modelTranslate = new Vector3(fleetModelsBounds.size.x + modelBounds.extents.x,
                                             0,
                                             0);
        if (fleetModelsBounds.size.x > 0)
        {
            modelTranslate.x += 2;
        }
        Debug.LogError("AddNewModel: modelTranslate == " + modelTranslate);

        modelTransform.Translate(modelTranslate, Space.Self);

        modelBounds = Utils.CalculateBounds(modelTransform);
        Debug.LogError("AddNewModel: AFTER SetParent modelBounds == " + Utils.ToString(modelBounds));

        FleetPlanesScale();

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

    private void FleetPlanesScale()
    {
        Debug.LogWarning("FleetPlanesScale()");

        GameObject fleetPlanes = FleetPlanes;
        Transform fleetPlanesTransform = fleetPlanes.transform;

        GameObject fleetModels = FleetModels;
        Transform fleetModelsTransform = fleetModels.transform;
        Bounds fleetModelsBounds = Utils.CalculateBounds(fleetModelsTransform);
        Debug.LogError("FleetPlanesScale: fleetModelsBounds == " + Utils.ToString(fleetModelsBounds));

        fleetPlanesTransform.localScale = fleetModelsBounds.size;

        Bounds fleetPlanesBounds = Utils.CalculateBounds(fleetPlanesTransform);
        Debug.LogError("FleetPlanesScale: AFTER localScale fleetPlanesBounds == " + Utils.ToString(fleetPlanesBounds));
    }

    private Dictionary<int, Color> debugColors = new Dictionary<int, Color>();

    private void DebugDecorate()
    {
        if (FleetModels != null)
        {
            Transform fleetModelsTransform = FleetModels.transform;
            foreach (Transform fleetModelTransform in fleetModelsTransform)
            {
                DebugDecorate(fleetModelTransform);
            }
            DebugDecorate(fleetModelsTransform);
        }
    }

    private void DebugDecorate(Transform transform)
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

        Bounds bounds = Utils.CalculateBounds(transform);

        Gizmos.DrawWireCube(bounds.center, bounds.size);
        Gizmos.DrawWireSphere(bounds.center, 0.5f);
        Gizmos.DrawWireSphere(bounds.max, 0.5f);
        Gizmos.DrawWireSphere(bounds.min, 0.5f);

        Gizmos.color = savedColor;
    }

    private void RepositionPlayerToViewFleet()
    {
        Debug.Log("RepositionPlayerToViewFleet()");

        GameObject fleetRoot = FleetRoot;
        Bounds fleetRootBounds = Utils.CalculateBounds(fleetRoot);
        Debug.LogError("RepositionPlayerToViewFleet: fleetRootBounds == " + Utils.ToString(fleetRootBounds));
        Debug.LogError("RepositionPlayerToViewFleet: fleetRootBounds.min == " + fleetRootBounds.min + ", fleetRootBounds.max == " + fleetRootBounds.max);

        Vector3 position = new Vector3(fleetRootBounds.center.x,
                                       fleetRootBounds.size.y * 0.4f,
                                       fleetRootBounds.size.z * -1.5f);

        // TODO:(pv) Improve this zoom in/out...
        Debug.LogError("RepositionPlayerToViewFleet: BEFORE Player.transform.position == " + Player.transform.position);
        Player.transform.position = position;
        Debug.LogError("RepositionPlayerToViewFleet: AFTER Player.transform.position == " + Player.transform.position);
        Player.transform.rotation = Quaternion.identity;

        position.y -= 1;

        Respawn.transform.position = position;
        Respawn.transform.rotation = Quaternion.identity;
    }
}
