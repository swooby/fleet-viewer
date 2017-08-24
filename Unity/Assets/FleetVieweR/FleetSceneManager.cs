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
    public const bool VERBOSE_LOG = false;
    public const bool GL_WIREFRAME = false;

    public const bool RESET_SETTINGS = false;

    private AppSettings AppSettings;

    private SortedDictionary<string, ModelInfo> ModelInfos = new SortedDictionary<string, ModelInfo>(StringComparer.OrdinalIgnoreCase);


    private GameObject FleetRoot;
    private GameObject FleetPlanes;
    private GameObject FleetModels;
    private GameObject Player;
    private GameObject Respawn;

    private DateTime timeLoadingStarted = DateTime.MinValue;

    private string messageBoxText;

    private void LoadNextModel(List<string> modelsToLoad)
    {
        if (modelsToLoad == null || modelsToLoad.Count == 0)
        {
            string text;

            if (timeLoadingStarted != DateTime.MinValue)
            {
                DateTime timeLoadingStopped = DateTime.Now;

                TimeSpan duration = timeLoadingStopped.Subtract(timeLoadingStarted);

                string durationString = Utils.ToString(duration);

                text = "Loaded: Took " + durationString;
                Debug.LogError("LoadNextModel: text == " + Utils.Quote(text));

                timeLoadingStarted = DateTime.MinValue;
            }
            else
            {
                text = "Loaded";
            }

            SetMessageBoxText(text, 5.0f);

            return;
        }

        if (timeLoadingStarted == DateTime.MinValue)
        {
            timeLoadingStarted = DateTime.Now;
        }

        string modelToLoad = modelsToLoad[0];
        modelsToLoad.RemoveAt(0);

        SetMessageBoxText("Loading " + modelToLoad);

        AddNewModel(modelToLoad, () =>
        {
            LoadNextModel(modelsToLoad);
        });
    }

    private void SetMessageBoxText(string text, float removeInSeconds = 0)
    {
        StartCoroutine(SetMessageBoxTextCoroutine(text, removeInSeconds));
    }

    IEnumerator SetMessageBoxTextCoroutine(string text, float removeInSeconds = 0)
    {
        messageBoxText = text;
        if (removeInSeconds > 0)
        {
            yield return new WaitForSeconds(removeInSeconds);
            messageBoxText = null;
        }
        yield return null;
    }

    private GUIStyle centeredStyle;

    private void OnGUI()
    {
        if (!string.IsNullOrEmpty(messageBoxText))
        {
            if (centeredStyle == null)
            {
                centeredStyle = GUI.skin.GetStyle("Box");
                centeredStyle.alignment = TextAnchor.MiddleCenter;
                centeredStyle.wordWrap = true;
            }

            GUI.Box(new Rect((Screen.width) / 2f - (Screen.width) / 8f,
                             (Screen.height) / 2f - (Screen.height) / 8f,
                             (Screen.width) / 4f,
                             (Screen.height) / 4f),
                    messageBoxText,
                    centeredStyle);
        }
    }

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

            // Order shortest to longest...
            // ...to give the appearance of loading fast right from the start...
            // ...as opposed to slowing down when loading the smaller ships.

            List<string> modelsToLoad = new List<string>();

            if (false)
            {
                if (true)
                {
                    // knight55
                    modelsToLoad.Add("Nox");
                    modelsToLoad.Add("Dragonfly");
                    modelsToLoad.Add("MPUV Cargo");
                    /*
					modelsToLoad.Add("Terrapin");
					modelsToLoad.Add("Gladius");
					modelsToLoad.Add("Gladiator");
					modelsToLoad.Add("Prospector");
					modelsToLoad.Add("F7C-M Super Hornet");
					modelsToLoad.Add("Sabre");
					modelsToLoad.Add("Vanguard Sentinel");
					modelsToLoad.Add("Retaliator Base");
					modelsToLoad.Add("Carrack");
					modelsToLoad.Add("Starfarer");
					modelsToLoad.Add("Polaris");
                    */
                    //modelsToLoad.Add("Idris-P");
                }
                else
                {
                    // night55
                }
            }
            else
            {
                for (int i = 0; i < 1; i++)
                {
                    modelsToLoad.Add("Nox");
                }
                for (int i = 0; i < 1; i++)
                {
                    modelsToLoad.Add("Dragonfly");
                }
                for (int i = 0; i < 1; i++)
                {
                    modelsToLoad.Add("MPUV Personnel");
                }
                for (int i = 0; i < 1; i++)
                {
                    modelsToLoad.Add("MPUV Cargo");
                }
                for (int i = 0; i < 1; i++)
                {
                    modelsToLoad.Add("Khartu-Al");
                }
                for (int i = 0; i < 1; i++)
                {
                    modelsToLoad.Add("M50");
                }
                for (int i = 0; i < 1; i++)
                {
                    modelsToLoad.Add("Razor");
                }
                for (int i = 0; i < 1; i++)
                {
                    modelsToLoad.Add("P-52 Merlin");
                }
                for (int i = 0; i < 1; i++)
                {
                    modelsToLoad.Add("P-72 Archimedes");
                }
                for (int i = 0; i < 1; i++)
                {
                    modelsToLoad.Add("Reliant Kore");
                }
                for (int i = 0; i < 1; i++)
                {
                    modelsToLoad.Add("Reliant Tana");
                }
                for (int i = 0; i < 0; i++)
                {
                    modelsToLoad.Add("Reliant Sen");
                }
                for (int i = 0; i < 0; i++)
                {
                    modelsToLoad.Add("Reliant Mako");
                }
                for (int i = 0; i < 1; i++)
                {
                    modelsToLoad.Add("Mustang Omega");
                }
                for (int i = 0; i < 0; i++)
                {
                    modelsToLoad.Add("Mustang Gamma");
                }
                for (int i = 0; i < 0; i++)
                {
                    modelsToLoad.Add("Mustang Delta");
                }
                for (int i = 0; i < 0; i++)
                {
                    modelsToLoad.Add("Mustang Beta");
                }
                for (int i = 0; i < 0; i++)
                {
                    modelsToLoad.Add("Mustang Alpha");
                }
                for (int i = 0; i < 1; i++)
                {
                    modelsToLoad.Add("Aurora LN");
                }
                for (int i = 0; i < 0; i++)
                {
                    modelsToLoad.Add("Aurora CL");
                }
                for (int i = 0; i < 0; i++)
                {
                    modelsToLoad.Add("Aurora MR");
                }
                for (int i = 0; i < 0; i++)
                {
                    modelsToLoad.Add("Aurora LX");
                }
                for (int i = 0; i < 0; i++)
                {
                    modelsToLoad.Add("Aurora ES");
                }
                for (int i = 0; i < 1; i++)
                {
                    modelsToLoad.Add("Avenger Stalker");
                }
                for (int i = 0; i < 1; i++)
                {
                    modelsToLoad.Add("Terrapin");
                }
                for (int i = 0; i < 1; i++)
                {
                    modelsToLoad.Add("Buccaneer");
                }
                for (int i = 0; i < 1; i++)
                {
                    modelsToLoad.Add("Gladius");
                }
                for (int i = 0; i < 1; i++)
                {
                    modelsToLoad.Add("Eclipse");
                }
                for (int i = 0; i < 1; i++)
                {
                    modelsToLoad.Add("Hull A");
                }
                for (int i = 0; i < 1; i++)
                {
                    modelsToLoad.Add("Gladiator");
                }
                for (int i = 0; i < 1; i++)
                {
                    modelsToLoad.Add("Hurricane");
                }
                for (int i = 0; i < 0; i++)
                {
                    modelsToLoad.Add("F7C-S Hornet Ghost");
                }
                for (int i = 0; i < 0; i++)
                {
                    modelsToLoad.Add("F7C-R Hornet Tracker");
                }
                for (int i = 0; i < 1; i++)
                {
                    modelsToLoad.Add("F7C Hornet");
                }
                for (int i = 0; i < 1; i++)
                {
                    modelsToLoad.Add("Herald");
                }
                for (int i = 0; i < 1; i++)
                {
                    modelsToLoad.Add("325a");
                }
                for (int i = 0; i < 0; i++)
                {
                    modelsToLoad.Add("315p");
                }
                for (int i = 0; i < 0; i++)
                {
                    modelsToLoad.Add("300i");
                }
                for (int i = 0; i < 1; i++)
                {
                    modelsToLoad.Add("Prospector");
                }
                for (int i = 0; i < 1; i++)
                {
                    modelsToLoad.Add("F7C-M Super Hornet");
                }
                for (int i = 0; i < 1; i++)
                {
                    modelsToLoad.Add("Sabre");
                }
                for (int i = 0; i < 1; i++)
                {
                    modelsToLoad.Add("Defender");
                }
                // As of 2017/08/22, anything beyond here crashes Pixel VR
                for (int i = 0; i < 1; i++)
                {
                    modelsToLoad.Add("Cutlass Black");
                }
                for (int i = 0; i < 0; i++)
                {
                    modelsToLoad.Add("Cutlass Red");
                }
                for (int i = 0; i < 0; i++)
                {
                    modelsToLoad.Add("Cutlass Blue");
                }
                for (int i = 0; i < 1; i++)
                {
                    modelsToLoad.Add("Vanduul Scythe");
                }
                for (int i = 0; i < 1; i++)
                {
                    modelsToLoad.Add("Esperia Glaive");
                }
                for (int i = 0; i < 1; i++)
                {
                    modelsToLoad.Add("Freelancer");
                }
                for (int i = 0; i < 1; i++)
                {
                    modelsToLoad.Add("Esperia Prowler");
                }
                for (int i = 0; i < 0; i++)
                {
                    modelsToLoad.Add("Vanguard Sentinel");
                }
                for (int i = 0; i < 1; i++)
                {
                    modelsToLoad.Add("Vanguard Harbinger");
                }
                for (int i = 0; i < 1; i++)
                {
                    modelsToLoad.Add("Redeemer");
                }
                for (int i = 0; i < 1; i++) // Pixel takes m:s to load to here
                {
                    modelsToLoad.Add("Hull B");
                }
                for (int i = 0; i < 0; i++)
                {
                    modelsToLoad.Add("Constellation Taurus");
                }
                for (int i = 0; i < 1; i++)
                {
                    modelsToLoad.Add("Constellation Aquila");
                }
                for (int i = 0; i < 0; i++)
                {
                    modelsToLoad.Add("Constellation Andromeda");
                }
                for (int i = 0; i < 0; i++)
                {
                    modelsToLoad.Add("Constellation Phoenix");
                }
                for (int i = 0; i < 1; i++)
                {
                    modelsToLoad.Add("Caterpillar");
                }
                for (int i = 0; i < 1; i++)
                {
                    modelsToLoad.Add("Retaliator Base");
                }
                for (int i = 0; i < 1; i++) // Pixel takes m:s to load to here
                {
                    modelsToLoad.Add("Crucible");
                }
                for (int i = 0; i < 1; i++)
                {
                    modelsToLoad.Add("Genesis");
                }
                for (int i = 0; i < 1; i++)
                {
                    modelsToLoad.Add("Starfarer");
                }
                for (int i = 0; i < 1; i++)
                {
                    modelsToLoad.Add("Hull C");
                }
                for (int i = 0; i < 1; i++)
                {
                    modelsToLoad.Add("890 Jump");
                }
                for (int i = 0; i < 1; i++)
                {
                    modelsToLoad.Add("Carrack");
                }
                for (int i = 0; i < 1; i++)
                {
                    modelsToLoad.Add("Polaris");
                }
                for (int i = 0; i < 1; i++)
                {
                    modelsToLoad.Add("Reclaimer");
                }
                for (int i = 0; i < 1; i++)
                {
                    modelsToLoad.Add("Orion");
                }
                for (int i = 0; i < 1; i++)
                {
                    modelsToLoad.Add("Endeavor");
                }
                for (int i = 0; i < 1; i++)
                {
                    modelsToLoad.Add("Hull D");
                }
                for (int i = 0; i < 1; i++)
                {
                    modelsToLoad.Add("Idris-P");
                }
                for (int i = 0; i < 1; i++)
                {
                    modelsToLoad.Add("Hull E");
                }
            }

            LoadNextModel(modelsToLoad);
        }
        else
        {
            foreach (ModelSettings modelSettings in AppSettings.ModelSettings)
            {
                AddSavedModel(modelSettings);
            }
        }

        //RepositionPlayerToViewFleet();
    }

    void Update()
    {
        // Exit when (X) is tapped.
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    void OnPreRender()
    {
        if (GL_WIREFRAME)
        {
            GL.wireframe = true;
        }
    }

    void OnPostRender()
    {
        if (GL_WIREFRAME)
        {
            GL.wireframe = false;
        }
    }

    private void OnDrawGizmos()
    {
        DebugDecorate();
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

    private delegate void AsyncLoadModelCaller(GameObject model);

    private void LoadModelAsync(string modelKey, AsyncLoadModelCaller caller)
    {
        ModelInfo modelInfo;

        if (!ModelInfos.TryGetValue(modelKey, out modelInfo) || modelInfo == null)
        {
            Debug.LogError("LoadModel: Failed to load modelKey == " + Utils.Quote(modelKey));
            return;
        }

        modelInfo.LoadModelAsync((model) =>
        {
            caller(model);
        });
    }

    private void AddSavedModel(ModelSettings modelSettings, bool repositionPlayerToViewFleet = true)
    {
        string modelKey = modelSettings.Key;

        LoadModelAsync(modelKey, (model) =>
        {

        });
    }

    private void AddNewModel(string modelKey, Action action = null)
    {
        Debug.Log("AddNewModel(modelKey:" + Utils.Quote(modelKey) +
                  ", action:" + action + ")");

        LoadModelAsync(modelKey, (model) =>
        {
            if (VERBOSE_LOG)
            {
                Debug.LogError("AddNewModel: LoadModelAsync completed");
            }

            if (model == null)
            {
                Debug.LogWarning("AddNewModel: Failed to load modelKey == " + Utils.Quote(modelKey));
                return;
            }

            Transform modelTransform = model.transform;
            Bounds modelBounds = Utils.CalculateBounds(modelTransform);
            if (VERBOSE_LOG)
            {
                Debug.LogError("AddNewModel: BEFORE modelBounds == " + Utils.ToString(modelBounds));
            }
            Vector3 modelLocalPosition = modelTransform.localPosition;
            if (VERBOSE_LOG)
            {
                Debug.LogError("AddNewModel: BEFORE modelLocalPosition == " + modelLocalPosition);
            }

            GameObject fleetModels = FleetModels;
            Transform fleetModelsTransform = fleetModels.transform;
            Bounds fleetModelsBounds = Utils.CalculateBounds(fleetModelsTransform);
            if (VERBOSE_LOG)
            {
                Debug.LogError("AddNewModel: BEFORE fleetModelsBounds == " + Utils.ToString(fleetModelsBounds));
            }

            modelTransform.SetParent(fleetModelsTransform);

            modelLocalPosition.x = -(fleetModelsBounds.size.x + modelBounds.extents.x);
            if (fleetModelsBounds.size.x > 0)
            {
                modelLocalPosition.x -= 2;
            }
            if (VERBOSE_LOG)
            {
                Debug.LogError("AddNewModel: AFTER modelLocalPosition == " + modelLocalPosition);
            }

            modelTransform.localPosition = modelLocalPosition;

            modelBounds = Utils.CalculateBounds(modelTransform);
            if (VERBOSE_LOG)
            {
                Debug.LogError("AddNewModel: AFTER SetParent modelBounds == " + Utils.ToString(modelBounds));
            }

            FleetPlanesPositionAndScale();

            if (PlayerController.HasNeverMoved)
            {
                RepositionPlayerToViewFleet();
            }

        // TODO:(pv) Auto-arrange/position according to scale and previously loaded models...

        // Calculate width of all loaded non-positioned models
        // Evenly reposition all loaded models

        // TODO:(pv) Save modelSettings...
        //modelSettings.Position = modelPosition;
        //modelSettings.Rotation = modelRotation;

            if (action != null)
            {
                action();
            }
        });
    }

    private void FleetPlanesPositionAndScale()
    {
        Debug.Log("FleetPlanesPositionAndScale()");

        GameObject fleetPlanes = FleetPlanes;
        Transform fleetPlanesTransform = fleetPlanes.transform;

        GameObject fleetModels = FleetModels;
        Transform fleetModelsTransform = fleetModels.transform;
        Bounds fleetModelsBounds = Utils.CalculateBounds(fleetModelsTransform);
        //Debug.LogError("FleetPlanesPositionAndScale: fleetModelsBounds == " + Utils.ToString(fleetModelsBounds));

        fleetPlanesTransform.localScale = fleetModelsBounds.size;
        fleetPlanesTransform.localPosition = new Vector3(-fleetModelsBounds.size.x, 0, 0);

        Bounds fleetPlanesBounds = Utils.CalculateBounds(fleetPlanesTransform);
        //Debug.LogError("FleetPlanesPositionAndScale: AFTER localScale fleetPlanesBounds == " + Utils.ToString(fleetPlanesBounds));
    }

    private void RepositionPlayerToViewFleet()
    {
        Debug.Log("RepositionPlayerToViewFleet()");

        GameObject fleetRoot = FleetRoot;
        Bounds fleetRootBounds = Utils.CalculateBounds(fleetRoot);
        //Debug.LogError("RepositionPlayerToViewFleet: fleetRootBounds == " + Utils.ToString(fleetRootBounds));
        Vector3 fleetRootBoundsSize = fleetRootBounds.size;

        //
        // https://docs.unity3d.com/Manual/FrustumSizeAtDistance.html
        //

        Camera camera = Camera.main;
        float cameraFieldOfViewX = camera.fieldOfView;
        //Debug.LogError("RepositionPlayerToViewFleet: cameraFieldOfViewX == " + cameraFieldOfViewX);
        float cameraAspect = camera.aspect;
        //Debug.LogError("RepositionPlayerToViewFleet: cameraAspect == " + cameraAspect);
        float cameraFieldOfViewY = cameraFieldOfViewX / cameraAspect;
        //Debug.LogError("RepositionPlayerToViewFleet: cameraFieldOfViewY == " + cameraFieldOfViewY);

        float opposite;
        float angle;

        float fleetRootWidth = fleetRootBoundsSize.x;
        float fleetRootDepth = fleetRootBoundsSize.z;

        if (fleetRootWidth > fleetRootDepth)
        {
            opposite = fleetRootWidth * 0.5f;
            angle = cameraFieldOfViewX * 0.5f;
        }
        else
        {
            opposite = fleetRootDepth * 0.5f;
            angle = cameraFieldOfViewY * 0.5f;
        }
        //Debug.LogError("RepositionPlayerToViewFleet: opposite == " + opposite);
        //Debug.LogError("RepositionPlayerToViewFleet: angle == " + angle);

        float adjacent = (float)(opposite / Math.Tan(Mathf.Deg2Rad * angle));
        //Debug.LogError("RepositionPlayerToViewFleet: adjacent == " + adjacent);

        Vector3 position = new Vector3(fleetRootBounds.center.x,
                                       fleetRootBounds.center.y + adjacent,
                                       fleetRootBounds.center.z);
        //Debug.LogError("RepositionPlayerToViewFleet: position == " + position);

        Quaternion rotation = Quaternion.Euler(90, 180, 0);

        Player.transform.localPosition = position;
        Player.transform.rotation = rotation;

        position.z += 1;

        Respawn.transform.localPosition = position;
        Respawn.transform.rotation = rotation;
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
}
