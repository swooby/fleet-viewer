using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using RTEditor;
using DaydreamElements.ClickMenu;


namespace FleetVieweR
{
    public class TestManipulateSceneManager : MonoBehaviour
    {
        private static readonly string TAG = Utils.TAG<TestManipulateSceneManager>();

        public const bool VERBOSE_LOG = false;

        [Tooltip("Reference to ModelsRoot")]
        public GameObject ModelsRoot;

        private SortedDictionary<string, ModelInfo> ModelInfos = new SortedDictionary<string, ModelInfo>(StringComparer.OrdinalIgnoreCase);

        private void Awake()
        {

        }

        void Start()
        {
            //List<string> modelsToLoad = new List<string>();
            //modelsToLoad.Add(StarCitizen.Nox);
            //LoadNextModel(modelsToLoad);
        }

        void Update()
        {
        }

        private void SetMessageBoxText(string text, float removeInSeconds = 0)
        {
            StartCoroutine(SetMessageBoxTextCoroutine(text, removeInSeconds));
        }

        private string messageBoxText;

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

        private DateTime timeLoadingStarted = DateTime.MinValue;

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

                GameObject modelsRoot = ModelsRoot;
                Transform modelsRootTransform = modelsRoot.transform;
                Bounds modelsRootBounds = Utils.CalculateBounds(modelsRootTransform);
                if (VERBOSE_LOG)
                {
                    Debug.LogError("AddNewModel: BEFORE modelsRootBounds == " + Utils.ToString(modelsRootBounds));
                }

                modelTransform.SetParent(modelsRootTransform);

                modelLocalPosition.x = -(modelsRootBounds.size.x + modelBounds.extents.x);
                if (modelsRootBounds.size.x > 0)
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

                //FleetPlanesPositionAndScale();

                if (PlayerController.HasNeverMoved)
                {
                    //RepositionPlayerToViewFleet();
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

        private delegate void AsyncLoadModelCaller(GameObject model);

        private void LoadModelAsync(string modelKey, AsyncLoadModelCaller caller)
        {
            ModelInfo modelInfo;

            if (!ModelInfos.TryGetValue(modelKey, out modelInfo) || modelInfo == null)
            {
                Debug.LogError("LoadModelAsync: Failed to load modelKey == " + Utils.Quote(modelKey));
                return;
            }

            modelInfo.LoadModelAsync((model) =>
            {
                caller(model);
            });
        }
    }
}