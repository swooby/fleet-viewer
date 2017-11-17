using UnityEngine;
using System.Collections;

namespace FleetVieweR
{
    public class FleetViewerManager : MonoBehaviour
    {
        //public GameObject LaunchVrHomeButton;
        public FleetControllerManager FleetControllerManager;

        void Start()
        {
#if !UNITY_ANDROID || UNITY_EDITOR
            /*
            if (LaunchVrHomeButton == null)
            {
                return;
            }
            LaunchVrHomeButton.SetActive(false);
            */
#else
            GvrDaydreamApi.CreateAsync((success) =>
            {
                if (!success)
                {
                    // Unexpected. See GvrDaydreamApi log messages for details.
                    Debug.LogError("GvrDaydreamApi.CreateAsync() failed");
                  }
            });
#endif  // !UNITY_ANDROID || UNITY_EDITOR
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        void Update()
        {
            /*
            if (LaunchVrHomeButton == null || FleetControllerManager == null)
            {
                return;
            }
            LaunchVrHomeButton.SetActive(FleetControllerManager.IsCurrentlyDaydream());
            */
        }
#endif  // UNITY_ANDROID && !UNITY_EDITOR

        public void LaunchVrHome()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            GvrDaydreamApi.LaunchVrHomeAsync((success) =>
            {
                if (!success)
                {
                    // Unexpected. See GvrDaydreamApi log messages for details.
                    Debug.LogError("GvrDaydreamApi.LaunchVrHomeAsync() failed");
                }
            });
#endif  // UNITY_ANDROID && !UNITY_EDITOR
        }
    }
}