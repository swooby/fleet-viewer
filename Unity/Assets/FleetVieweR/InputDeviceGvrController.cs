using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace RTEditor
{
    public class InputDeviceGvrController : InputDeviceAbstract
    {
        public const int INPUT_CLICK = 0;
        public const int INPUT_TOUCH = 1;

        private static readonly int[] INPUTS = { INPUT_CLICK, INPUT_TOUCH };

        private GvrLaserPointer _laserPointer;

        private GvrLaserPointer GetLaserPointerIfActiveAndEnabled()
        {
            if (_laserPointer == null)
            {
                _laserPointer = GvrPointerInputModule.Pointer as GvrLaserPointer;
            }
            return _laserPointer != null && _laserPointer.isActiveAndEnabled ?
                                                         _laserPointer :
                                                         null;
        }

        /// <summary>
        /// Gets the laser pointer endpoint.
        /// </summary>
        /// <returns>The laser pointer endpoint.</returns>
        /// <param name="laserPointer">Laser pointer; Must be non-null and previously checked for isActiveAndEnabled.</param>
        private static Vector3 GetLaserPointerEndpoint(GvrLaserPointer laserPointer)
        {
            Vector3 laserPointerEndPoint = Vector3.zero;
            if (laserPointer != null)
            {
                RaycastResult raycastResult = laserPointer.CurrentRaycastResult;
                if (raycastResult.gameObject != null)
                {
                    laserPointerEndPoint = raycastResult.worldPosition;
                }
                else
                {
                    laserPointerEndPoint = laserPointer.GetPointAlongPointer(laserPointer.defaultReticleDistance);
                }
            }
            return laserPointerEndPoint;
        }

        public override bool UsingTouch
        {
            get
            {
                return false;
            }
        }

        public override bool IsPressed(int deviceButtonIndex)
        {
            switch (deviceButtonIndex)
            {
                case INPUT_CLICK:
                    return GvrControllerInput.ClickButton;
                case INPUT_TOUCH:
                    return GvrControllerInput.IsTouching;
                default:
                    throw new ArgumentOutOfRangeException("deviceButtonIndex", "deviceButtonIndex must be INPUT_CLICK or INPUT_TOUCH");
            }
        }

        public override bool WasPressedInCurrentFrame(int deviceButtonIndex)
        {
            switch (deviceButtonIndex)
            {
                case INPUT_CLICK:
                    return GvrControllerInput.ClickButtonDown;
                case INPUT_TOUCH:
                    return GvrControllerInput.TouchDown;
                default:
                    throw new ArgumentOutOfRangeException("deviceButtonIndex", "deviceButtonIndex must be INPUT_CLICK or INPUT_TOUCH");
            }
        }

        public override bool WasReleasedInCurrentFrame(int deviceButtonIndex)
        {
            switch (deviceButtonIndex)
            {
                case INPUT_CLICK:
                    return GvrControllerInput.ClickButtonUp;
                case INPUT_TOUCH:
                    return GvrControllerInput.TouchUp;
                default:
                    throw new ArgumentOutOfRangeException("deviceButtonIndex", "deviceButtonIndex must be INPUT_CLICK or INPUT_TOUCH");
            }
        }

        public override bool GetPosition(out Vector3 position)
        {
            GvrLaserPointer laserPointer = GetLaserPointerIfActiveAndEnabled();
            if (laserPointer != null)
            {
                Vector3 laserPointerEndPoint = GetLaserPointerEndpoint(laserPointer);
                if (laserPointerEndPoint != Vector3.zero)
                {
                    position = EditorCamera.Instance.Camera.WorldToScreenPoint(laserPointerEndPoint);
                    return true;
                }
            }

            position = Vector3.zero;
            return false;
        }

        public override bool GetPickRay(Camera camera, out Ray ray)
        {
            Vector3 laserPointerEndPoint;
            if (GetPosition(out laserPointerEndPoint))
            {
                ray = camera.ScreenPointToRay(laserPointerEndPoint);
                return true;
            }

            ray = new Ray();
            return false;
        }

        public override bool WasMoved()
        {
            return GetLaserPointerIfActiveAndEnabled() != null &&
                _deltaSinceLastFrame[0] != Vector3.zero;
        }

        public override void Update()
        {
            // Calculate the laserPointerEndPoint delta
            Vector3 laserPointerEndPoint;
            if (!GetPosition(out laserPointerEndPoint))
            {
                return;
            }

            _deltaSinceLastFrame[0] = laserPointerEndPoint - _previousFramePositions[0];

            // Store the current laserPointerEndPoint position as the previous position for the next frame
            _previousFramePositions[0] = laserPointerEndPoint;

            // Now we will loop through all supported laserPointer inputs and update the input offset
            // since each button was pressed.
            foreach (int input in INPUTS)
            {
                // If the input was pressed or released in the current frame, there is nothing to
                // do except to reset the corresponding offset to the zero vector.
                if (WasPressedInCurrentFrame(input) ||
                    WasReleasedInCurrentFrame(input))
                {
                    _deltaSincePressed[input] = Vector3.zero;
                    continue;
                }
                else
                {
                    // If the input wasn't pressed or released, we will check to see if it is
                    // still pressed (it may have been pressed in a previous frame). If it is, we will
                    // add the delta since the last frame to get the delta since it was pressed.
                    if (IsPressed(input)) _deltaSincePressed[input] += _deltaSinceLastFrame[input];
                }
            }
        }
    }
}