using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(FleetInputManager))]
public class FleetInputManagerEditor : Editor
{
    SerializedProperty emulatedPlatformTypeProp;
    SerializedProperty gvrControllerMainProp;
    SerializedProperty gvrControllerPointerProp;
    SerializedProperty gvrReticlePointerProp;

    void OnEnable()
    {
        gvrControllerMainProp = serializedObject.FindProperty(FleetInputManager.CONTROLLER_MAIN_PROP_NAME);
        gvrControllerPointerProp = serializedObject.FindProperty(FleetInputManager.CONTROLLER_POINTER_PROP_NAME);
        gvrReticlePointerProp = serializedObject.FindProperty(FleetInputManager.RETICLE_POINTER_PROP_NAME);

        emulatedPlatformTypeProp = serializedObject.FindProperty(FleetInputManager.EMULATED_PLATFORM_PROP_NAME);
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(gvrControllerMainProp);
        EditorGUILayout.PropertyField(gvrControllerPointerProp);
        EditorGUILayout.PropertyField(gvrReticlePointerProp);

        if (DemoInputManager.playerSettingsHasCardboard() == FleetInputManager.playerSettingsHasDaydream())
        {
            // Show the platform emulation dropdown only if both or neither VR SDK selected in
            // Player Settings > Virtual Reality supported,
            EditorGUILayout.PropertyField(emulatedPlatformTypeProp);
        }

        serializedObject.ApplyModifiedProperties();
    }
}
