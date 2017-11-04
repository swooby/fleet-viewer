#if UNITY_ANDROID && !UNITY_EDITOR
#define RUNNING_ON_ANDROID_DEVICE
#endif  // UNITY_ANDROID && !UNITY_EDITOR

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;
using UnityEngine;
using OpenCTM;

namespace FleetVieweR
{
    public class FleetInputManager : MonoBehaviour
    {
        // Java class, method, and field constants.
        private const int ANDROID_MIN_DAYDREAM_API = 24;
        private const string FIELD_SDK_INT = "SDK_INT";
        private const string PACKAGE_BUILD_VERSION = "android.os.Build$VERSION";
        private const string PACKAGE_DAYDREAM_API_CLASS = "com.google.vr.ndk.base.DaydreamApi";
        private const string METHOD_IS_DAYDREAM_READY = "isDaydreamReadyPlatform";

        private bool isDaydream = false;

        public static string CARDBOARD_DEVICE_NAME = "cardboard";
        public static string DAYDREAM_DEVICE_NAME = "daydream";

        [Tooltip("Reference to GvrControllerMain")]
        public GameObject controllerMain;
        public static string CONTROLLER_MAIN_PROP_NAME = "controllerMain";

        [Tooltip("Reference to GvrControllerPointer")]
        public GameObject controllerPointer;
        public static string CONTROLLER_POINTER_PROP_NAME = "controllerPointer";

        [Tooltip("Reference to GvrReticlePointer")]
        public GameObject reticlePointer;
        public static string RETICLE_POINTER_PROP_NAME = "reticlePointer";

#if !RUNNING_ON_ANDROID_DEVICE
        public enum EmulatedPlatformType
        {
            Daydream,
            Cardboard
        }
        [Tooltip("Emulated GVR Platform")]
        public EmulatedPlatformType gvrEmulatedPlatformType = EmulatedPlatformType.Daydream;
        public static string EMULATED_PLATFORM_PROP_NAME = "gvrEmulatedPlatformType";
#else
    // Running on an Android device.
    private GvrSettings.ViewerPlatformType viewerPlatform;
#endif  // !RUNNING_ON_ANDROID_DEVICE

        void Start()
        {
#if !RUNNING_ON_ANDROID_DEVICE
            if (playerSettingsHasDaydream() || playerSettingsHasCardboard())
            {
                // The list is populated with valid VR SDK(s), pick the first one.
                gvrEmulatedPlatformType =
                  (UnityEngine.VR.VRSettings.supportedDevices[0] == DAYDREAM_DEVICE_NAME) ?
                  EmulatedPlatformType.Daydream :
                  EmulatedPlatformType.Cardboard;
            }
            isDaydream = (gvrEmulatedPlatformType == EmulatedPlatformType.Daydream);
#else
        // Running on an Android device.
        viewerPlatform = GvrSettings.ViewerPlatform;
        // First loaded device in Player Settings.
        string vrDeviceName = UnityEngine.VR.VRSettings.loadedDeviceName;
        if (vrDeviceName != CARDBOARD_DEVICE_NAME &&
            vrDeviceName != DAYDREAM_DEVICE_NAME)
        {
            Debug.LogErrorFormat("Loaded device was '{0}', must be one of '{1}' or '{2}'",
                  vrDeviceName, DAYDREAM_DEVICE_NAME, CARDBOARD_DEVICE_NAME);
            return;
        }

        // On a non-Daydream ready phone, fall back to Cardboard if it's present in the list of
        // enabled VR SDKs.
        // On a Daydream-ready phone, go into Cardboard mode if it's the currently-paired viewer.
        if ((!IsDeviceDaydreamReady() && playerSettingsHasCardboard()) ||
            (IsDeviceDaydreamReady() && playerSettingsHasCardboard() &&
             GvrSettings.ViewerPlatform == GvrSettings.ViewerPlatformType.Cardboard))
        {
            vrDeviceName = CARDBOARD_DEVICE_NAME;
        }
        isDaydream = (vrDeviceName == DAYDREAM_DEVICE_NAME);
#endif  // !RUNNING_ON_ANDROID_DEVICE
            SetVRInputMechanism();
        }

        // Runtime switching enabled only in-editor.
        void Update()
        {
#if !RUNNING_ON_ANDROID_DEVICE
            UpdateEmulatedPlatformIfPlayerSettingsChanged();
            if ((isDaydream && gvrEmulatedPlatformType == EmulatedPlatformType.Daydream) ||
                (!isDaydream && gvrEmulatedPlatformType == EmulatedPlatformType.Cardboard))
            {
                return;
            }
            isDaydream = (gvrEmulatedPlatformType == EmulatedPlatformType.Daydream);
            SetVRInputMechanism();
#else
        // Running on an Android device.
        // Viewer type switched at runtime.
        if (!IsDeviceDaydreamReady() || viewerPlatform == GvrSettings.ViewerPlatform)
        {
            return;
        }
        isDaydream = (GvrSettings.ViewerPlatform == GvrSettings.ViewerPlatformType.Daydream);
        viewerPlatform = GvrSettings.ViewerPlatform;
        SetVRInputMechanism();
#endif  // !RUNNING_ON_ANDROID_DEVICE
        }

        public bool IsCurrentlyDaydream()
        {
            return isDaydream;
        }

        public static bool playerSettingsHasDaydream()
        {
            string[] playerSettingsVrSdks = UnityEngine.VR.VRSettings.supportedDevices;
            return Array.Exists<string>(playerSettingsVrSdks,
                element => element.Equals(DemoInputManager.DAYDREAM_DEVICE_NAME));
        }

        public static bool playerSettingsHasCardboard()
        {
            string[] playerSettingsVrSdks = UnityEngine.VR.VRSettings.supportedDevices;
            return Array.Exists<string>(playerSettingsVrSdks,
                element => element.Equals(DemoInputManager.CARDBOARD_DEVICE_NAME));
        }

#if !RUNNING_ON_ANDROID_DEVICE
        private void UpdateEmulatedPlatformIfPlayerSettingsChanged()
        {
            if (!playerSettingsHasDaydream() && !playerSettingsHasCardboard())
            {
                return;
            }

            // Player Settings > VR SDK list may have changed at runtime. The emulated platform
            // may not have been manually updated if that's the case.
            if (gvrEmulatedPlatformType == EmulatedPlatformType.Daydream &&
                !playerSettingsHasDaydream())
            {
                gvrEmulatedPlatformType = EmulatedPlatformType.Cardboard;
            }
            else if (gvrEmulatedPlatformType == EmulatedPlatformType.Cardboard &&
                     !playerSettingsHasCardboard())
            {
                gvrEmulatedPlatformType = EmulatedPlatformType.Daydream;
            }
        }
#endif  // !RUNNING_ON_ANDROID_DEVICE

#if RUNNING_ON_ANDROID_DEVICE
    // Running on an Android device.
    private static bool IsDeviceDaydreamReady()
    {
        // Check API level.
        using (var version = new AndroidJavaClass(PACKAGE_BUILD_VERSION))
        {
            if (version.GetStatic<int>(FIELD_SDK_INT) < ANDROID_MIN_DAYDREAM_API)
            {
                return false;
            }
        }
        // API level > 24, check whether the device is Daydream-ready..
        AndroidJavaObject androidActivity = null;
        try
        {
            androidActivity = GvrActivityHelper.GetActivity();
        }
        catch (AndroidJavaException e)
        {
            Debug.LogError("Exception while connecting to the Activity: " + e);
            return false;
        }
        AndroidJavaClass daydreamApiClass = new AndroidJavaClass(PACKAGE_DAYDREAM_API_CLASS);
        if (daydreamApiClass == null || androidActivity == null)
        {
            return false;
        }
        return daydreamApiClass.CallStatic<bool>(METHOD_IS_DAYDREAM_READY, androidActivity);
    }
#endif  // RUNNING_ON_ANDROID_DEVICE

        private void SetVRInputMechanism()
        {
            SetGazeInputActive(!isDaydream);
            SetControllerInputActive(isDaydream);
        }

        private void SetGazeInputActive(bool active)
        {
            if (reticlePointer == null)
            {
                return;
            }
            reticlePointer.SetActive(active);

            // Update the pointer type only if this is currently activated.
            if (!active)
            {
                return;
            }

            GvrReticlePointer pointer =
                reticlePointer.GetComponent<GvrReticlePointer>();
            if (pointer != null)
            {
                GvrPointerInputModule.Pointer = pointer;
            }
        }

        private void SetControllerInputActive(bool active)
        {
            if (controllerMain != null)
            {
                controllerMain.SetActive(active);
            }
            if (controllerPointer == null)
            {
                return;
            }
            controllerPointer.SetActive(active);

            // Update the pointer type only if this is currently activated.
            if (!active)
            {
                return;
            }
            GvrLaserPointer pointer =
                controllerPointer.GetComponentInChildren<GvrLaserPointer>(true);
            if (pointer != null)
            {
                GvrPointerInputModule.Pointer = pointer;
            }
        }

        // #endif  // UNITY_ANDROID
    }
}