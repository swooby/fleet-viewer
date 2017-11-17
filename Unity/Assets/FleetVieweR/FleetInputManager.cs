using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RTEditor;
using DaydreamElements.ClickMenu;

namespace FleetVieweR
{
    public class FleetInputManager : MonoBehaviour
    {
        private static readonly string TAG = Utils.TAG<FleetInputManager>();

        public const bool VERBOSE_LOG_EDITOR_OBJECT_SELECTION = false;

        [Tooltip("Reference to ModelsRoot")]
        public GameObject ModelsRoot;
        [Tooltip("Reference to GvrControllerPointer")]
        public GameObject GvrControllerPointer;
        [Tooltip("Reference to Unselectable Game Objects")]
        public GameObject[] UnselectableGameObjects;
        [Tooltip("Reference to ClickMenuRoot")]
        public ClickMenuRoot MenuRoot;
        [Tooltip("Reference to ClickMenuTree")]
        public ClickMenuTree MenuTree;

        private static void AddGameObjectAndAllChildren(GameObject gameObject, List<GameObject> list)
        {
            if (gameObject == null)
            {
                return;
            }
            list.Add(gameObject);
            list.AddRange(gameObject.GetAllChildren());
        }

        void Awake()
        {
            Input.backButtonLeavesApp = true;

            //
            // Initialize "Runtime Transform Gizmos"
            //

            List<GameObject> selectionMask = new List<GameObject>();

            foreach (GameObject unselectableGameObject in UnselectableGameObjects)
            {
                AddGameObjectAndAllChildren(unselectableGameObject, selectionMask);
            }

            if (GvrControllerPointer != null)
            {
#if UNITY_ANDROID // TODO:(pv) Better way to runtime detect Daydream/Cardboard?
                InputDeviceGvrController inputDeviceGvrController = new InputDeviceGvrController();
                InputDevice.Instance.SetInputDevice(inputDeviceGvrController);

                if (!selectionMask.Contains(GvrControllerPointer))
                {
                    AddGameObjectAndAllChildren(GvrControllerPointer, selectionMask);
                }
#endif
            }

            EditorObjectSelection editorObjectSelection = EditorObjectSelection.Instance;

            editorObjectSelection.AddGameObjectCollectionToSelectionMask(selectionMask);

            editorObjectSelection.SelectionChanged += EditorObjectSelection_OnSelectionChanged;

            EditorGizmoSystem editorGizmoSystem = EditorGizmoSystem.Instance;

            TranslationGizmo translationGizmo = editorGizmoSystem.TranslationGizmo;
            translationGizmo.GizmoHoverEnter += Gizmo_GizmoHoverEnter;
            translationGizmo.GizmoHoverExit += Gizmo_GizmoHoverExit;

            RotationGizmo rotationGizmo = editorGizmoSystem.RotationGizmo;
            rotationGizmo.GizmoHoverEnter += Gizmo_GizmoHoverEnter;
            rotationGizmo.GizmoHoverExit += Gizmo_GizmoHoverExit;

#if UNITY_EDITOR
            translationGizmo.GizmoBaseScale = 1;
            rotationGizmo.GizmoBaseScale = translationGizmo.GizmoBaseScale * 2;
#endif

            //
            // Initialize "Daydream Elements Click Menu"
            //

            if (MenuRoot != null)
            {
                MenuRoot.GetClickMenuTree += MenuRoot_GetClickMenuTree;
                MenuRoot.OnMenuOpened += MenuRoot_OnMenuOpened;
                MenuRoot.OnMenuClosed += MenuRoot_OnMenuClosed;
                MenuRoot.OnItemSelected += MenuRoot_OnItemSelected;
            }
        }

        void Start()
        {
            EnableEvaMode(true);
        }

        void Update()
        {
            // Exit when upper-left-X or Escape pressed.
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Exit();
            }
        }

        //
        //
        //

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

        //
        //
        //

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

        private void EnableEvaMode(bool enable)
        {
            playerControllerAllowTouchMovementBeforeForcedDisabled = null;
            PlayerController.AllowTouchMovement = enable;
            EnableObjectSelection(!enable);
            // TODO:(pv) Update ClickMenu/MenuRoot...
        }

        private void EnableObjectSelection(bool enable)
        {
            //Debug.Log("EnableObjectSelection(" + enable + ")");
            ObjectSelectionSettings objectSelectionSettings = EditorObjectSelection.Instance.ObjectSelectionSettings;
            objectSelectionSettings.CanSelectEmptyObjects =
                    objectSelectionSettings.CanClickSelect =
                    objectSelectionSettings.CanMultiSelect = enable;
        }

        private List<GameObject> SelectedObjects = new List<GameObject>();

        private void EditorObjectSelection_OnSelectionChanged(ObjectSelectionChangedEventArgs args)
        {
            if (VERBOSE_LOG_EDITOR_OBJECT_SELECTION)
            {
                Debug.Log(DateTime.Now + " " + TAG + " EditorObjectSelection_OnSelectionChanged: args.SelectActionType:" + args.SelectActionType);
                Debug.Log(DateTime.Now + " " + TAG + " EditorObjectSelection_OnSelectionChanged: args.SelectedObjects:" + Utils.ToString(args.SelectedObjects));
                Debug.Log(DateTime.Now + " " + TAG + " EditorObjectSelection_OnSelectionChanged: args.DeselectActionType:" + args.DeselectActionType);
                Debug.Log(DateTime.Now + " " + TAG + " EditorObjectSelection_OnSelectionChanged: args.DeselectedObjects:" + Utils.ToString(args.DeselectedObjects));
            }
            switch (args.SelectActionType)
            {
                case ObjectSelectActionType.Click:
                case ObjectSelectActionType.MultiSelect:
                    {
                        foreach (GameObject selectedObject in args.SelectedObjects)
                        {
                            if (VERBOSE_LOG_EDITOR_OBJECT_SELECTION)
                            {
                                Debug.Log(DateTime.Now + " " + TAG + " EditorObjectSelection_OnSelectionChanged: selectedObject:" + selectedObject);
                            }
                            GameObject modelRoot = FindModelRoot(ModelsRoot, selectedObject);
                            if (VERBOSE_LOG_EDITOR_OBJECT_SELECTION)
                            {
                                Debug.Log(DateTime.Now + " " + TAG + " EditorObjectSelection_OnSelectionChanged: modelRoot:" + modelRoot);
                            }

                            if (modelRoot != null)
                            {
                                Debug.Log(DateTime.Now + " " + TAG + " EditorObjectSelection_OnSelectionChanged: RemoveObjectFromSelection(" + selectedObject + ")");
                                EditorObjectSelection.Instance.RemoveObjectFromSelection(selectedObject, false);
                            }
                            else
                            {
                                modelRoot = selectedObject;
                            }

                            if (SelectedObjects.Contains(modelRoot))
                            {
                                if (args.SelectActionType == ObjectSelectActionType.Click)
                                {
                                    SelectedObjects.Remove(modelRoot);
                                }
                            }
                            else
                            {
                                SelectedObjects.Add(modelRoot);
                            }
                        }
                        if (!EditorObjectSelection.Instance.IsSelectionExactMatch(SelectedObjects))
                        {
                            // Per documentation, allowUndoRedo should always be false when inside of a handler
                            // TODO:(pv) Experiment to see if this can be nicely set true
                            Debug.Log(DateTime.Now + " " + TAG + " EditorObjectSelection_OnSelectionChanged: SetSelectedObjects(" + Utils.ToString(SelectedObjects) + ")");
                            EditorObjectSelection.Instance.SetSelectedObjects(SelectedObjects, false);
                        }
                        break;
                    }
            }
            if (args.DeselectActionType != ObjectDeselectActionType.None)
            {
                foreach (GameObject deselectedObject in args.DeselectedObjects)
                {
                    SelectedObjects.Remove(deselectedObject);
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

        //
        //
        //

        private void MenuRoot_OnMenuOpened()
        {
            PlayerControllerAllowTouchMovementForceDisable();
        }

        private void MenuRoot_OnMenuClosed()
        {
            PlayerControllerAllowTouchMovementRestore();
        }

        private ClickMenuTree MenuRoot_GetClickMenuTree()
        {
            return MenuTree;
        }

        /// <summary>
        /// No Models Loaded:
        ///          Add
        ///       Load Settings 
        ///          Exit 
        /// One Or More Model(s) Loaded:
        ///  EVA/Select Add
        ///       Load   Settings  
        ///           Exit 
        /// One Or More Model(s) Selected:
        ///  EVA/Select Add
        ///     Remove   Duplicate
        ///    Rotate     Move
        ///       Load   Settings  
        ///           Exit 
        /// Modified:
        ///         Undo Redo
        ///  EVA/Select   Add
        ///        Load   Save
        ///         Exit Settings 
        /// Modified & One Or More Model(s) Selected:
        ///         Undo Redo
        ///  EVA/Select   Add
        ///     Remove     Duplicate
        ///     Rotate     Move
        ///        Load   Save
        ///         Exit Settings 
        /// </summary>
        /// <param name="item">Item.</param>
        private void MenuRoot_OnItemSelected(ClickMenuItem item)
        {
            Debug.Log("MenuRoot_OnItemSelected(item:" + item + ")");
            if (item == null)
            {
                return;
            }
            Debug.Log("MenuRoot_OnItemSelected: item.id:" + item.id);
            switch (item.id)
            {
                // TODO:(pv) Toggle Global/Local Transform
                // TODO:(pv) Step Snapping
                // TODO:(pv) Vertex Snapping
                // TODO:(pv) Box Snapping
                // TODO:(pv) Surface Placement
                // TODO:(pv) Surface Placement Align X
                // TODO:(pv) Surface Placement Align Y
                // TODO:(pv) Surface Placement Align Z
                // TODO:(pv) Surface Placement Align None
                // TODO:(pv) Move Scale
                case 0100: // Undo:
                    //EditorObjectSelection.Instance.SelectedGameObjects;
                    //EditorObjectSelection.Instance.UndoRedoSelection();
                    EditorUndoRedoSystem.Instance.Undo();
                    break;
                case 0200: // Redo:
                    EditorUndoRedoSystem.Instance.Redo();
                    break;
                case 0300: // Add: Brings up model Carrossel
                    break;
                case 0400: // Duplicate: (Hidden if no item(s) selected) Duplicate Selected Item(s)
                    EditorObjectSelection.Instance.DuplicateSelection();
                    break;
                case 0500: // Move: Disables EVA & RotationGizmo, Enables TransformGizmo
                    EditorGizmoSystem.Instance.ActiveGizmoType = GizmoType.Translation;
                    break;
                case 0600: // Save: Name fleet file and Save to cloud
                    break;
                case 0700: // Settings:
                    break;
                case 0800: // Exit:
                    Exit();
                    break;
                case 0900: // Load: Browse fleet files and Load from cloud
                    break;
                case 1000: // Rotate: Disables EVA & TransformGizmo, Enables RotationGizmo
                    EditorGizmoSystem.Instance.ActiveGizmoType = GizmoType.Rotation;
                    break;
                case 1100: // Remove: (Hidden if no item(s) selected) Remove Selected Item(s)
                    EditorObjectSelection.Instance.DeleteSelection();
                    break;
                case 1200: // Toggle EVA/Select:
                    EnableEvaMode(true);
                    break;
                case 1300:
                    EnableEvaMode(false);
                    break;
            }
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
    }
}
