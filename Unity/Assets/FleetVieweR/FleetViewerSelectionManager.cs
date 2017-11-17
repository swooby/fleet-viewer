using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RTEditor;

namespace FleetVieweR
{
    public class FleetViewerSelectionManager : MonoBehaviour
    {
        private static readonly string TAG = Utils.TAG<FleetViewerSelectionManager>();

        public const bool VERBOSE_LOG_EDITOR_OBJECT_SELECTION = false;

        [Tooltip("Reference to ModelsRoot")]
        public GameObject ModelsRoot;
        [Tooltip("Reference to GvrControllerPointer")]
        public GameObject GvrControllerPointer;
        [Tooltip("Reference to Unselectedable Game Objects")]
        public GameObject[] UnselectableGameObjects;

        void Awake()
        {
            List<GameObject> selectionMask = GvrControllerPointer.GetAllChildren();

            if (GvrControllerPointer != null)
            {
#if UNITY_ANDROID // TODO:(pv) Better way to runtime detect Daydream/Cardboard?
                InputDeviceGvrController inputDeviceGvrController = new InputDeviceGvrController();
                InputDevice.Instance.SetInputDevice(inputDeviceGvrController);
#endif
                selectionMask.Add(GvrControllerPointer);
            }

            foreach (GameObject gameObject in UnselectableGameObjects)
            {
                selectionMask.Add(gameObject);
            }

            EditorObjectSelection.Instance.AddGameObjectCollectionToSelectionMask(selectionMask);

            EditorObjectSelection.Instance.SelectionChanged += EditorObjectSelection_OnSelectionChanged;
            EditorGizmoSystem.Instance.TranslationGizmo.GizmoHoverEnter += Gizmo_GizmoHoverEnter;
            EditorGizmoSystem.Instance.TranslationGizmo.GizmoHoverExit += Gizmo_GizmoHoverExit;
            EditorGizmoSystem.Instance.RotationGizmo.GizmoHoverEnter += Gizmo_GizmoHoverEnter;
            EditorGizmoSystem.Instance.RotationGizmo.GizmoHoverExit += Gizmo_GizmoHoverExit;
        }

        void Start()
        {
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
    }
}
