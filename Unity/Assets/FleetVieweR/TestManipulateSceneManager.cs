﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using RTEditor;

// TODO:(pv) Fix Scene Gizmo rotation snapping-back to original
// TODO:(pv) Disable Scene Gizmo Orthogonal toggle

namespace FleetVieweR
{
    public class TestSceneManager : MonoBehaviour
    {
        private const string TAG = "TestSceneManager";

        public const bool VERBOSE_LOG = false;

        //[Tooltip("Reference to GvrControllerMain")]
        //public GameObject controllerMain;
        //[Tooltip("Reference to GvrControllerPointer")]
        //public GameObject controllerPointer;
        //[Tooltip("Reference to GvrReticlePointer")]
        //public GameObject reticlePointer;

        public GameObject ModelsRoot;
        public GameObject TestModel;

        private SortedDictionary<string, ModelInfo> ModelInfos = new SortedDictionary<string, ModelInfo>(StringComparer.OrdinalIgnoreCase);

        void Start()
        {
            //List<string> modelsToLoad = new List<string>();
            //modelsToLoad.Add(StarCitizen.Nox);
            //LoadNextModel(modelsToLoad);

            InputDevice.Instance.SetInputDevice(new InputDeviceGvrController());
            //GvrPointerInputModule.Pointer.drawDebugRays = true;

            //GvrControllerInput.OnControllerInputUpdated += GvrControllerInput_OnControllerInputUpdated;
            //GvrTrackedController
            //GvrPointerInputModule.Pointer.
            //GvrControllerInput.
            //GvrPointerManager.
            //GvrContro

            EditorObjectSelection.Instance.SelectionChanged += OnSelectionChanged;

            if (TestModel != null)
            {
                AddEventTrigger(TestModel, (eventData) =>
                {
                    Debug.Log("Selected Test Model");
                    EditorObjectSelection.Instance.AddObjectToSelection(TestModel, true);
                });
            }
        }

        private HashSet<GameObject> objectSelection = new HashSet<GameObject>();

        private void Update()
        {
            if (GvrControllerInput.ClickButtonDown)
            {
                //Debug.Log("Start");
                AddRaycastObjectToSelection();
            }
            else if (GvrControllerInput.ClickButtonUp)
            {
                //Debug.Log("End");
                AddRaycastObjectToSelection();
                EditorObjectSelection.Instance.SetSelectedObjects(new List<GameObject>(objectSelection), true);
                objectSelection.Clear();
            }
            else if (GvrControllerInput.ClickButton)
            {
                //Debug.Log("Continue");
                AddRaycastObjectToSelection();
            }
        }

        private void AddRaycastObjectToSelection()
        {
            GvrLaserPointer pointer = GvrPointerInputModule.Pointer as GvrLaserPointer;
            if (pointer != null && pointer.isActiveAndEnabled)
            {
                RaycastResult raycastResult = pointer.CurrentRaycastResult;
                if (raycastResult.isValid)
                {
                    GameObject selectedObject = raycastResult.gameObject;
                    //Debug.Log("selectedObjesct:" + selectedObject);
                    objectSelection.Add(selectedObject);
                }
                /*
                else
                {
                    Debug.Log("No Result");
                }
                */
            }
        }

        private void AddEventTrigger(GameObject gameObject, UnityAction<BaseEventData> eventData)
        {
            EventTrigger trigger = gameObject.AddComponent<EventTrigger>();
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerClick;
            entry.callback.AddListener(eventData);
            trigger.triggers.Add(entry);
        }

        private void OnSelectionChanged(ObjectSelectionChangedEventArgs args)
        {
            Debug.Log(TAG + " OnSelectionChanged: args.SelectActionType:" + args.SelectActionType);
            if (args.SelectActionType != ObjectSelectActionType.None)
            {
                var objectsWhichWereSelected = args.SelectedObjects;
                foreach (var obj in objectsWhichWereSelected)
                {
                    // GameObjectExtension.GetFirstEmptyParent?

                    GameObject rootObject = obj.transform.root.gameObject;

                    if (rootObject == obj) continue;

                    EditorObjectSelection.Instance.AddObjectToSelection(rootObject, false);
                }
            }
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