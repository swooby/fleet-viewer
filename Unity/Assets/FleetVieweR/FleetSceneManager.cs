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

            for (int i = 0; i < 1; i++)
            {
                AddNewModel("Hull E");
            }
            for (int i = 0; i < 1; i++)
            {
                AddNewModel("Idris-P");
            }
            for (int i = 0; i < 1; i++)
            {
                AddNewModel("Hull D");
            }
            for (int i = 0; i < 1; i++)
            {
                AddNewModel("Endeavor");
            }
            for (int i = 0; i < 1; i++)
            {
                AddNewModel("Orion");
            }
            for (int i = 0; i < 1; i++)
            {
                AddNewModel("Reclaimer");
            }
            for (int i = 0; i < 1; i++)
            {
                AddNewModel("Polaris");
            }
            for (int i = 0; i < 1; i++)
            {
                AddNewModel("Carrack");
            }
            for (int i = 0; i < 1; i++)
            {
                AddNewModel("890 Jump");
            }
            for (int i = 0; i < 1; i++)
            {
                AddNewModel("Hull C");
            }
            for (int i = 0; i < 1; i++)
            {
                AddNewModel("Starfarer");
            }
            for (int i = 0; i < 1; i++)
            {
                AddNewModel("Genesis");
            }
            for (int i = 0; i < 1; i++)
            {
                AddNewModel("Crucible");
            }
            for (int i = 0; i < 1; i++)
            {
                AddNewModel("Retaliator Base");
            }
            for (int i = 0; i < 1; i++)
            {
                AddNewModel("Caterpillar");
            }
            for (int i = 0; i < 1; i++)
            {
                AddNewModel("Constellation Phoenix");
            }
            for (int i = 0; i < 1; i++)
            {
                AddNewModel("Constellation Andromeda");
            }
            for (int i = 0; i < 1; i++)
            {
                AddNewModel("Constellation Aquila");
            }
            for (int i = 0; i < 1; i++)
            {
                AddNewModel("Constellation Taurus");
            }
            for (int i = 0; i < 1; i++)
            {
                AddNewModel("Hull B");
            }
            for (int i = 0; i < 1; i++)
            {
                AddNewModel("Redeemer");
            }
            for (int i = 0; i < 1; i++)
            {
                AddNewModel("Vanguard Harbinger");
            }
            for (int i = 0; i < 1; i++)
            {
                AddNewModel("Vanguard Sentinel");
            }
            for (int i = 0; i < 1; i++)
            {
                AddNewModel("Esperia Prowler");
            }
            for (int i = 0; i < 1; i++)
            {
                AddNewModel("Freelancer");
            }
            for (int i = 0; i < 1; i++)
            {
                AddNewModel("Esperia Glaive");
            }
            for (int i = 0; i < 1; i++)
            {
                AddNewModel("Vanduul Scythe");
            }
            for (int i = 0; i < 1; i++)
            {
                AddNewModel("Cutlass Blue");
            }
            for (int i = 0; i < 1; i++)
            {
                AddNewModel("Cutlass Red");
            }
            for (int i = 0; i < 1; i++)
            {
                AddNewModel("Cutlass Black");
            }
            for (int i = 0; i < 1; i++)
            {
                AddNewModel("Defender");
            }
            for (int i = 0; i < 1; i++)
            {
                AddNewModel("Sabre");
            }
            for (int i = 0; i < 1; i++)
            {
                AddNewModel("F7C-M Super Hornet");
            }
            for (int i = 0; i < 1; i++)
            {
                AddNewModel("Prospector");
            }
            for (int i = 0; i < 1; i++)
            {
                AddNewModel("300i");
            }
            for (int i = 0; i < 1; i++)
            {
                AddNewModel("315p");
            }
            for (int i = 0; i < 1; i++)
            {
                AddNewModel("325a");
            }
            for (int i = 0; i < 1; i++)
            {
                AddNewModel("Herald");
            }
            for (int i = 0; i < 1; i++)
            {
                AddNewModel("F7C Hornet");
            }
            for (int i = 0; i < 1; i++)
            {
                AddNewModel("F7C-R Hornet Tracker");
            }
            for (int i = 0; i < 1; i++)
            {
                AddNewModel("F7C-S Hornet Ghost");
            }
            for (int i = 0; i < 1; i++)
            {
                AddNewModel("Hurricane");
            }
            for (int i = 0; i < 1; i++)
            {
                AddNewModel("Gladiator");
            }
            for (int i = 0; i < 1; i++)
            {
                AddNewModel("Hull A");
            }
            for (int i = 0; i < 1; i++)
            {
                AddNewModel("Eclipse");
            }
            for (int i = 0; i < 1; i++)
            {
                AddNewModel("Gladius");
            }
            for (int i = 0; i < 1; i++)
            {
                AddNewModel("Buccaneer");
            }
            for (int i = 0; i < 1; i++)
            {
                AddNewModel("Terrapin");
            }
            for (int i = 0; i < 1; i++)
            {
                AddNewModel("Avenger Stalker");
            }
            for (int i = 0; i < 1; i++)
            {
                AddNewModel("Aurora ES");
            }
            for (int i = 0; i < 1; i++)
            {
                AddNewModel("Aurora LX");
            }
            for (int i = 0; i < 1; i++)
            {
                AddNewModel("Aurora MR");
            }
            for (int i = 0; i < 1; i++)
            {
                AddNewModel("Aurora CL");
            }
            for (int i = 0; i < 1; i++)
            {
                AddNewModel("Aurora LN");
            }
            for (int i = 0; i < 1; i++)
            {
                AddNewModel("Mustang Alpha");
            }
            for (int i = 0; i < 1; i++)
            {
                AddNewModel("Mustang Beta");
            }
            for (int i = 0; i < 1; i++)
            {
                AddNewModel("Mustang Delta");
            }
            for (int i = 0; i < 1; i++)
            {
                AddNewModel("Mustang Gamma");
            }
            for (int i = 0; i < 1; i++)
            {
                AddNewModel("Mustang Omega");
            }
            for (int i = 0; i < 1; i++)
            {
                AddNewModel("Reliant Mako");
            }
            for (int i = 0; i < 1; i++)
            {
                AddNewModel("Reliant Sen");
            }
            for (int i = 0; i < 1; i++)
            {
                AddNewModel("Reliant Tana");
            }
            for (int i = 0; i < 1; i++)
            {
                AddNewModel("Reliant Kore");
            }
            for (int i = 0; i < 1; i++)
            {
                AddNewModel("P-72 Archimedes");
            }
            for (int i = 0; i < 1; i++)
            {
                AddNewModel("P-52 Merlin");
            }
            for (int i = 0; i < 1; i++)
            {
                AddNewModel("Razor");
            }
            for (int i = 0; i < 1; i++)
            {
                AddNewModel("M50");
            }
            for (int i = 0; i < 1; i++)
            {
                AddNewModel("Khartu-Al");
            }
            for (int i = 0; i < 1; i++)
            {
                AddNewModel("MPUV Cargo");
            }
            for (int i = 0; i < 1; i++)
            {
                AddNewModel("MPUV Personnel");
            }
            for (int i = 0; i < 1; i++)
            {
                AddNewModel("Dragonfly");
            }
            for (int i = 0; i < 1; i++)
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
        if (model == null)
        {
            Debug.LogWarning("AddNewModel: Failed to load modelKey == " + Utils.Quote(modelKey));
            return null;
        }

        Transform modelTransform = model.transform;
        Bounds modelBounds = Utils.CalculateBounds(modelTransform);
        //Debug.LogError("AddNewModel: BEFORE modelBounds == " + Utils.ToString(modelBounds));
        Vector3 modelLocalPosition = modelTransform.localPosition;
        //Debug.LogError("AddNewModel: BEFORE modelLocalPosition == " + modelLocalPosition);

        GameObject fleetModels = FleetModels;
        Transform fleetModelsTransform = fleetModels.transform;
        Bounds fleetModelsBounds = Utils.CalculateBounds(fleetModelsTransform);
        //Debug.LogError("AddNewModel: BEFORE fleetModelsBounds == " + Utils.ToString(fleetModelsBounds));

        modelTransform.SetParent(fleetModelsTransform);

        modelLocalPosition.x = fleetModelsBounds.size.x + modelBounds.extents.x;
        if (fleetModelsBounds.size.x > 0)
        {
            modelLocalPosition.x += 2;
        }
        //Debug.LogError("AddNewModel: AFTER modelLocalPosition == " + modelLocalPosition);

        modelTransform.localPosition = modelLocalPosition;

        modelBounds = Utils.CalculateBounds(modelTransform);
        //Debug.LogError("AddNewModel: AFTER SetParent modelBounds == " + Utils.ToString(modelBounds));

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
        Debug.Log("FleetPlanesScale()");

        GameObject fleetPlanes = FleetPlanes;
        Transform fleetPlanesTransform = fleetPlanes.transform;

        GameObject fleetModels = FleetModels;
        Transform fleetModelsTransform = fleetModels.transform;
        Bounds fleetModelsBounds = Utils.CalculateBounds(fleetModelsTransform);
        //Debug.LogError("FleetPlanesScale: fleetModelsBounds == " + Utils.ToString(fleetModelsBounds));

        fleetPlanesTransform.localScale = fleetModelsBounds.size;

        Bounds fleetPlanesBounds = Utils.CalculateBounds(fleetPlanesTransform);
        //Debug.LogError("FleetPlanesScale: AFTER localScale fleetPlanesBounds == " + Utils.ToString(fleetPlanesBounds));
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
        //Debug.LogError("RepositionPlayerToViewFleet: fleetRootBounds == " + Utils.ToString(fleetRootBounds));
        //Debug.LogError("RepositionPlayerToViewFleet: fleetRootBounds.min == " + fleetRootBounds.min + ", fleetRootBounds.max == " + fleetRootBounds.max);

        Vector3 position = new Vector3(fleetRootBounds.center.x,
                                       fleetRootBounds.size.y * 0.4f,
                                       fleetRootBounds.size.z * -1.5f);

        // TODO:(pv) Improve this zoom in/out...
        //Debug.LogError("RepositionPlayerToViewFleet: BEFORE Player.transform.position == " + Player.transform.position);
        Player.transform.position = position;
        //Debug.LogError("RepositionPlayerToViewFleet: AFTER Player.transform.position == " + Player.transform.position);
        Player.transform.rotation = Quaternion.identity;

        position.y -= 1;

        Respawn.transform.position = position;
        Respawn.transform.rotation = Quaternion.identity;
    }
}
