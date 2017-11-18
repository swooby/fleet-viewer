using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RTEditor;
using DaydreamElements.Common;
using DaydreamElements.ClickMenu;

// TODO:(pv) Get Scene Gizmo to work (currently doesn't because no Orthogonal cameras are supported in VR)
// TODO:(pv) ClickMenu shows too much XZ grid lines through it

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
            translationGizmo.GizmoDragEnd += Gizmo_GizmoDragEnd;

            RotationGizmo rotationGizmo = editorGizmoSystem.RotationGizmo;
            rotationGizmo.GizmoHoverEnter += Gizmo_GizmoHoverEnter;
            rotationGizmo.GizmoHoverExit += Gizmo_GizmoHoverExit;
            rotationGizmo.GizmoDragEnd += Gizmo_GizmoDragEnd;

#if UNITY_EDITOR
            translationGizmo.GizmoBaseScale = 1;
            rotationGizmo.GizmoBaseScale = translationGizmo.GizmoBaseScale * 2;
#endif

            //
            // Initialize "Daydream Elements Click Menu"
            //

            if (MenuTree != null)
            {
                InitializeClickMenuItem(MenuTree.tree.Root);
            }

            if (MenuRoot != null)
            {
                MenuRoot.GetClickMenuTreeNode += MenuRoot_GetClickMenuTreeNode;
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

        private void Gizmo_GizmoDragEnd(Gizmo gizmo)
        {
            isOneOrMoreModelModified = true;
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
            // TODO:(pv) There are still some selection bugs here, espcially in VR
            if (args.DeselectActionType != ObjectDeselectActionType.None)
            {
                foreach (GameObject deselectedObject in args.DeselectedObjects)
                {
                    SelectedObjects.Remove(deselectedObject);
                }
            }

            isOneOrMoreModelSelected = SelectedObjects.Count > 0;
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

        private bool isOneOrMoreModelLoaded;
        private bool isOneOrMoreModelSelected;
        private bool isOneOrMoreModelModified;

        private Dictionary<int, AssetTree.Node> clickMenuItems = new Dictionary<int, AssetTree.Node>();

        private void InitializeClickMenuItem(AssetTree.Node node)
        {
            ClickMenuItem clickMenuItem = node.value as ClickMenuItem;
            Debug.Log("AddClickMenuItem: clickMenuItem:" + clickMenuItem);
            if (clickMenuItem != null)
            {
                int clickMenuItemId = clickMenuItem.id;

                if (clickMenuItems.ContainsKey(clickMenuItemId))
                {
                    Debug.LogWarning("AddClickMenuItem: Unexpected duplicate clickMenuItems[" +
                                     clickMenuItemId + "]:" + clickMenuItems[clickMenuItemId] +
                                     " and clickMenuItem:" + clickMenuItem);
                }
                clickMenuItems[clickMenuItemId] = node;
            }

            foreach(AssetTree.Node child in node.children)
            {
                InitializeClickMenuItem(child);
            }
        }

        /// <summary>
        /// Must match ClickMenuItems listed in MenuTree
        /// </summary>
        private enum ClickMenuItemIds
        {
            Undo = 100,
            Redo = 200,
            Add = 300,
            Copy = 400,
            Move = 500,
            Save = 600,
            Options = 700,
            OptionsGlobal = 710,
            Exit = 800,
            Load = 900,
            Rotate = 1000,
            Delete = 1100,
            Eva = 1200,
            Select = 1300,
        }

        /// <summary>
        /// No Models Loaded:
        ///          Add
        ///       Load Options 
        ///          Exit 
        /// One Or More Model(s) Loaded:
        ///  EVA/Select Add
        ///       Load   Options  
        ///           Exit 
        /// One Or More Model(s) Selected:
        ///  EVA/Select Add
        ///     Remove   Duplicate
        ///    Rotate     Move
        ///       Load   Options  
        ///           Exit 
        /// Modified:
        ///         Undo Redo
        ///  EVA/Select   Add
        ///        Load   Save
        ///         Exit Options 
        /// Modified & One Or More Model(s) Selected:
        ///         Undo Redo
        ///  EVA/Select   Add
        ///     Remove     Duplicate
        ///     Rotate     Move
        ///        Load   Save
        ///         Exit Options 
        /// </summary>
        /// <returns>The root get click menu tree node.</returns>
        private AssetTree.Node MenuRoot_GetClickMenuTreeNode()
        {
            AssetTree.Node root = new AssetTree.Node();

            AssetTree.Node node;

            node = AddClickMenuItem(root, ClickMenuItemIds.Add);
            node = AddClickMenuItem(root, ClickMenuItemIds.Options);
            node = AddClickMenuItem(root, ClickMenuItemIds.Exit);
            node = AddClickMenuItem(root, ClickMenuItemIds.Load);

            return root;
        }

        private AssetTree.Node AddClickMenuItem(AssetTree.Node root, ClickMenuItemIds id)
        {
            AssetTree.Node node = clickMenuItems[(int)id];
            root.children.Add(node);
            return node;
        }

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

        private void Save()
        {
            //...
            isOneOrMoreModelModified = false;
        }

        private void Load()
        {
            //...
            //isOneOrMoreModelLoaded = ...;
        }
    }
}
