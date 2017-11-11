using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using RTEditor;
using DaydreamElements.ClickMenu;

// TODO:(pv) Fix Scene Gizmo rotation snapping-back to original

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
    ///         Turn off gizmos: Alpha0
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
    ///         SELECTION BOX RENDER MODE: From Parent To Bottom
    ///         Append to selection: Num keys 0
    ///         Multi deselect: Num keys 0
    ///         Duplicate selection: Return (Setting to Num keys 0 causes weird unusable lag bug!)
    ///         Delete selection: Delete
    ///     Scene Gizmo:
    ///         Corner: None (Currently no way known to set Orthogonal projection in VR)
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
        public const bool VERBOSE_LOG_EDITOR_OBJECT_SELECTION = false;

        [Tooltip("Reference to ModelsRoot")]
        public GameObject ModelsRoot;
        [Tooltip("Reference to GvrControllerPointer")]
        public GameObject GvrControllerPointer;
        [Tooltip("Reference to ClickMenuRoot")]
        public ClickMenuRoot MenuRoot;

        private InputDeviceGvrController inputDeviceGvrController;

        private SortedDictionary<string, ModelInfo> ModelInfos = new SortedDictionary<string, ModelInfo>(StringComparer.OrdinalIgnoreCase);

        private void Awake()
        {
            Input.backButtonLeavesApp = true;

            MenuRoot.OnMenuOpened += MenuRoot_OnMenuOpened;
            MenuRoot.OnMenuClosed += MenuRoot_OnMenuClosed;
            MenuRoot.OnItemSelected += MenuRoot_OnItemSelected;

            if (GvrControllerPointer != null)
            {
#if UNITY_ANDROID // TODO:(pv) Better way to detect Daydream/Cardboard?
                inputDeviceGvrController = new InputDeviceGvrController();
                InputDevice.Instance.SetInputDevice(inputDeviceGvrController);
#endif
                List<GameObject> selectionMask = GvrControllerPointer.GetAllChildren();
                selectionMask.Add(GvrControllerPointer);
                EditorObjectSelection.Instance.AddGameObjectCollectionToSelectionMask(selectionMask);
            }

            EditorObjectSelection.Instance.SelectionChanged += EditorObjectSelection_OnSelectionChanged;
            EditorGizmoSystem.Instance.TranslationGizmo.GizmoHoverEnter += Gizmo_GizmoHoverEnter;
            EditorGizmoSystem.Instance.TranslationGizmo.GizmoHoverExit += Gizmo_GizmoHoverExit;
            EditorGizmoSystem.Instance.RotationGizmo.GizmoHoverEnter += Gizmo_GizmoHoverEnter;
            EditorGizmoSystem.Instance.RotationGizmo.GizmoHoverExit += Gizmo_GizmoHoverExit;
        }

        void Start()
        {
            //List<string> modelsToLoad = new List<string>();
            //modelsToLoad.Add(StarCitizen.Nox);
            //LoadNextModel(modelsToLoad);

            EnableEvaMode(true);
        }

        void Update()
        {
            // Exit when (X) is tapped.
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Application.Quit();
            }
        }

        private void Gizmo_GizmoHoverEnter(Gizmo gizmo)
        {
            //Debug.LogWarning("Gizmo_GizmoHoverEnter(" + gizmo + ")");
            PlayerControllerAllowTouchMovementForceDisable();
        }

        private void Gizmo_GizmoHoverExit(Gizmo gizmo)
        {
            //Debug.LogWarning("Gizmo_GizmoHoverExit(" + gizmo + ")");
            PlayerControllerAllowTouchMovementRestore();
        }

        private void MenuRoot_OnMenuOpened()
        {
            PlayerControllerAllowTouchMovementForceDisable();
        }

        private void MenuRoot_OnMenuClosed()
        {
            PlayerControllerAllowTouchMovementRestore();
        }

        private bool? playerControllerAllowTouchMovementBeforeForcedDisabled;

        private void PlayerControllerAllowTouchMovementForceDisable()
        {
            playerControllerAllowTouchMovementBeforeForcedDisabled = PlayerController.AllowTouchMovement;
            PlayerController.AllowTouchMovement = false;
        }

        private void PlayerControllerAllowTouchMovementRestore()
        {
            if (playerControllerAllowTouchMovementBeforeForcedDisabled.HasValue)
            {
                PlayerController.AllowTouchMovement = playerControllerAllowTouchMovementBeforeForcedDisabled.Value;
            }
        }
        {
            switch (item.id)
            {
                case 100: // EVA Disables TransformGizmo & RotationGizmo, Enables EVA
                    break;
                case 200: // Add Brings up model Carrasel (sp)
                    break;
                case 300: // Move Disables EVA & RotationGizmo, Enables TransformGizmo
                    break;
                case 400: // Save Name fleet file and Save to cloud
                    break;
                case 500: // Exit
                    Exit();
                    break;
                case 600: // Load Browse and Load models from cloud
                    break;
                case 700: // Rotate Disables EVA & TransformGizmo, Enables RotationGizmo
                    break;
                case 800: // Remove (Hidden if no item(s) selected) Remove Selected Item(s)
                    break;
            }
        }

        private void EnableEvaMode(bool enable)
        {
            playerControllerAllowTouchMovementBeforeForcedDisabled = null;
            PlayerController.AllowTouchMovement = enable;
            EnableObjectSelection(!enable);
        }

        private void EnableObjectSelection(bool enable)
        {
            ObjectSelectionSettings objectSelectionSettings = EditorObjectSelection.Instance.ObjectSelectionSettings;
            objectSelectionSettings.CanSelectEmptyObjects =
                    objectSelectionSettings.CanClickSelect =
                    objectSelectionSettings.CanMultiSelect = enable;
        }

        private void Exit()
        {
            // TODO:(pv) Prompt to save first...
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
//#elif UNITY_WEBPLAYER
            //Application.OpenURL(webplayerQuitURL);
#else
            Application.Quit();
#endif
        }

        private void EditorObjectSelection_OnSelectionChanged(ObjectSelectionChangedEventArgs args)
        {
            if (VERBOSE_LOG_EDITOR_OBJECT_SELECTION)
            {
                Debug.Log(TAG + " EditorObjectSelection_OnSelectionChanged: args.SelectActionType:" + args.SelectActionType);
                Debug.Log(TAG + " EditorObjectSelection_OnSelectionChanged: args.SelectedObjects:" + Utils.ToString(args.SelectedObjects));
                Debug.Log(TAG + " EditorObjectSelection_OnSelectionChanged: args.DeselectActionType:" + args.DeselectActionType);
                Debug.Log(TAG + " EditorObjectSelection_OnSelectionChanged: args.DeselectedObjects:" + Utils.ToString(args.DeselectedObjects));
            }
            switch (args.SelectActionType)
            {
                case ObjectSelectActionType.Click:
                case ObjectSelectActionType.MultiSelect:
                    {
                        List<GameObject> selectedObjects = new List<GameObject>();
                        foreach (GameObject selectedObject in args.SelectedObjects)
                        {
                            if (VERBOSE_LOG_EDITOR_OBJECT_SELECTION)
                            {
                                Debug.Log(TAG + " EditorObjectSelection_OnSelectionChanged: selectedObject:" + selectedObject);
                            }
                            GameObject modelRoot = FindModelRoot(ModelsRoot, selectedObject);
                            if (VERBOSE_LOG_EDITOR_OBJECT_SELECTION)
                            {
                                Debug.Log(TAG + " EditorObjectSelection_OnSelectionChanged: modelRoot:" + modelRoot);
                            }
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