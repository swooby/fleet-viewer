using UnityEngine;
using UnityEditor;

namespace FleetVieweR
{
    [CustomEditor(typeof(FleetControllerManager))]
    public class FleetControllerManagerEditor : Editor
    {
        SerializedProperty emulatedPlatformTypeProp;
        SerializedProperty gvrControllerMainProp;
        SerializedProperty gvrControllerPointerProp;
        SerializedProperty gvrReticlePointerProp;

        void OnEnable()
        {
            gvrControllerMainProp = serializedObject.FindProperty(FleetControllerManager.CONTROLLER_MAIN_PROP_NAME);
            gvrControllerPointerProp = serializedObject.FindProperty(FleetControllerManager.CONTROLLER_POINTER_PROP_NAME);
            gvrReticlePointerProp = serializedObject.FindProperty(FleetControllerManager.RETICLE_POINTER_PROP_NAME);

            emulatedPlatformTypeProp = serializedObject.FindProperty(FleetControllerManager.EMULATED_PLATFORM_PROP_NAME);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(gvrControllerMainProp);
            EditorGUILayout.PropertyField(gvrControllerPointerProp);
            EditorGUILayout.PropertyField(gvrReticlePointerProp);

            if (DemoInputManager.playerSettingsHasCardboard() == FleetControllerManager.playerSettingsHasDaydream())
            {
                // Show the platform emulation dropdown only if both or neither VR SDK selected in
                // Player Settings > Virtual Reality supported,
                EditorGUILayout.PropertyField(emulatedPlatformTypeProp);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}