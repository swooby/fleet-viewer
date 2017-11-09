using System;
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
    ///
    /// Preferred RuntimeTransformGizmos' settings:
    ///     Runtime Editor Application:
    ///         Enable Undo/Redo: True
    ///         Use Custom Camera: True Main Camera
    ///         Use Unity Colliders: True
    ///     XZ Grid Settings:
    ///         Is Visible: True
    ///         Scroll grip up/down: Num keys 0
    ///         Scroll grip up/down (STEP): Num keys 0
    ///     Editor Gizmo System:
    ///         Translation: True
    ///         Rotation: True
    ///         Activate move gizmo: Alpha1
    ///         Activate rotation gizmo: Alpha2
    ///         Activate global transform: O
    ///         Activate local transform: L
    ///         Turn off gizmos: 0
    ///         Toggle pivot: P
    ///     Editor Object Selection:
    ///         Can Select Empty Objects: True
    ///         Can Click-Select: True
    ///         Can Multi-Select: True
    ///         Default: True
    ///         TransparentFX: True
    ///         Ignore Raycast: True
    ///         Water: True
    ///         UI: True
    ///         Default: True
    ///         TransparentFX: True
    ///         Ignore Raycast: True
    ///         Water: True
    ///         UI: True
    ///         Draw Selection Boxes: True
    ///         Append to selection: Num keys 0
    ///         Multi deselect: Num keys 0
    ///         Duplicate selection: Return (Setting to Num keys 0 causes weird unusable lag bug!)
    ///         Delete selection: Delete
    ///     Scene Gizmo:
    ///         Lock Perspective: True
    ///     Translation Gizmo:
    ///         Gizmo Base Scale: 2
    ///         Preserve Gizmo Screen Size: True
    ///         No keymappings
    ///     Rotation Gizmo:
    ///         Gizmo Base Scale: 4
    ///         Preserve Gizmo Screen Size: True
    ///         Full Circle X/Y/Z: ?
    ///         No keymappings
    ///
    /// Preferred GvrLaserPointer Settings:
    ///     Hybrid
    ///     Default Reticle Distance: 0.5
    ///     Draw Debug Rays (for now)
    ///
    public class TestManipulateSceneManager : MonoBehaviour
    {
        private const string TAG = "TestManipulateSceneManager";

        public const bool VERBOSE_LOG = false;

        [Tooltip("Reference to ModelsRoot")]
        public GameObject ModelsRoot;
        [Tooltip("Reference to GvrControllerPointer")]
        public GameObject GvrControllerPointer;

        private InputDeviceGvrController inputDeviceGvrController;

        private SortedDictionary<string, ModelInfo> ModelInfos = new SortedDictionary<string, ModelInfo>(StringComparer.OrdinalIgnoreCase);

        void Start()
        {
#if UNITY_ANDROID // TODO:(pv) Better way to detect Daydream/Carboard?
            inputDeviceGvrController = new InputDeviceGvrController();
            InputDevice.Instance.SetInputDevice(inputDeviceGvrController);
#endif
            if (GvrControllerPointer != null)
            {
                List<GameObject> selectionMask = GvrControllerPointer.GetAllChildren();
                selectionMask.Add(GvrControllerPointer);
                EditorObjectSelection.Instance.AddGameObjectCollectionToSelectionMask(selectionMask);
            }

            EditorObjectSelection.Instance.SelectionChanged += OnSelectionChanged;

            //List<string> modelsToLoad = new List<string>();
            //modelsToLoad.Add(StarCitizen.Nox);
            //LoadNextModel(modelsToLoad);
        }

        private void OnSelectionChanged(ObjectSelectionChangedEventArgs args)
        {
            //Debug.Log(TAG + " OnSelectionChanged: args.SelectActionType:" + args.SelectActionType);
            //Debug.Log(TAG + " OnSelectionChanged: args.SelectedObjects:" + Utils.ToString(args.SelectedObjects));
            //Debug.Log(TAG + " OnSelectionChanged: args.DeselectActionType:" + args.DeselectActionType);
            //Debug.Log(TAG + " OnSelectionChanged: args.DeselectedObjects:" + Utils.ToString(args.DeselectedObjects));
            switch (args.SelectActionType)
            {
                case ObjectSelectActionType.SetSelectedObjectsCall:
                    {
                        EditorObjectSelection.Instance.ObjectSelectionSettings.ObjectSelectionBoxRenderSettings.DrawBoxes = true;
                        break;
                    }
                case ObjectSelectActionType.Click:
                case ObjectSelectActionType.MultiSelect:
                    {
                        List<GameObject> selectedObjects = new List<GameObject>();
                        foreach (GameObject selectedObject in args.SelectedObjects)
                        {
                            //Debug.Log(TAG + " OnSelectionChanged: selectedObject:" + selectedObject);
                            GameObject modelRoot = FindModelRoot(ModelsRoot, selectedObject);
                            //Debug.Log(TAG + " OnSelectionChanged: modelRoot:" + modelRoot);
                            if (modelRoot != null)
                            {
                                EditorObjectSelection.Instance.RemoveObjectFromSelection(selectedObject, false);
                                if (!selectedObjects.Contains(modelRoot))
                                {
                                    selectedObjects.Add(modelRoot);
                                }
                            }
                        }

                        if (selectedObjects.Count > 0)
                        {
                            // Per documentation, allowUndoRedo should always be false when inside of a handler
                            // TODO:(pv) Experiment to see if this can be nicely set true
                            EditorObjectSelection.Instance.SetSelectedObjects(selectedObjects, false);
                        }
                        else
                        {
                            EditorObjectSelection.Instance.ObjectSelectionSettings.ObjectSelectionBoxRenderSettings.DrawBoxes = true;
                        }
                        break;
                    }
                case ObjectSelectActionType.None:
                    {
                        EditorObjectSelection.Instance.ObjectSelectionSettings.ObjectSelectionBoxRenderSettings.DrawBoxes = false;
                        break;
                    }
            }
        }

        private static GameObject FindModelRoot(GameObject modelsRoot, GameObject child)
        {
            while (true)
            {
                Transform parentTransform = child.transform.parent;
                if (parentTransform == null)
                {
                    break;
                }

                GameObject parent = parentTransform.gameObject;
                if (parent == modelsRoot)
                {
                    return child;
                }

                child = parent;
            }

            return null;
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