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
        private Vector3 _laserPointerStartPosition;
        private Quaternion _laserPointerStartRotation;

        public InputDeviceGvrController()
        {
            _laserPointer = GvrPointerInputModule.Pointer as GvrLaserPointer;
            if (_laserPointer != null)
            {
                Transform laserPointerTransform = _laserPointer.gameObject.transform;
                _laserPointerStartPosition = laserPointerTransform.position;
                _laserPointerStartRotation = laserPointerTransform.rotation;
            }
        }

        private Vector3 GetLaserPointerEndpoint()
        {
            Vector3 laserPointerEndPoint = Vector3.zero;

            if (_laserPointer != null && _laserPointer.isActiveAndEnabled)
            {
                RaycastResult raycastResult = _laserPointer.CurrentRaycastResult;
                if (raycastResult.gameObject != null)
                {
                    laserPointerEndPoint = raycastResult.worldPosition;
                }
                else
                {
                    laserPointerEndPoint = _laserPointer.GetPointAlongPointer(_laserPointer.defaultReticleDistance);
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
            bool isPressed = Input.GetMouseButton(deviceButtonIndex);
            //Debug.LogWarning("isPressed A:" + isPressed);
            switch(deviceButtonIndex)
            {
                case INPUT_CLICK:
                    isPressed = GvrControllerInput.ClickButton;
                    break;
                case INPUT_TOUCH:
                    isPressed = GvrControllerInput.IsTouching;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("deviceButtonIndex", "deviceButtonIndex must be INPUT_CLICK or INPUT_TOUCH");
            }
            //Debug.LogWarning("isPressed B:" + isPressed);
            return isPressed;
        }

        public override bool WasPressedInCurrentFrame(int deviceButtonIndex)
        {
            // TODO:(pv) Handle both Touch+TouchPos & Click+Move...
            bool wasPressedInCurrentFrame = Input.GetMouseButtonDown(deviceButtonIndex);
            //Debug.LogWarning("wasPressedInCurrentFrame A:" + wasPressedInCurrentFrame);
            switch (deviceButtonIndex)
            {
                case INPUT_CLICK:
                    wasPressedInCurrentFrame = GvrControllerInput.ClickButtonDown;
                    break;
                case INPUT_TOUCH:
                    wasPressedInCurrentFrame = GvrControllerInput.TouchDown;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("deviceButtonIndex", "deviceButtonIndex must be INPUT_CLICK or INPUT_TOUCH");
            }
            //Debug.LogWarning("wasPressedInCurrentFrame B:" + wasPressedInCurrentFrame);
            return wasPressedInCurrentFrame;
        }

        public override bool WasReleasedInCurrentFrame(int deviceButtonIndex)
        {
            // TODO:(pv) Handle both Touch+TouchPos & Click+Move...
            bool wasReleasedInCurrentFrame = Input.GetMouseButtonUp(deviceButtonIndex);
            //Debug.LogWarning("wasReleasedInCurrentFrame A:" + wasReleasedInCurrentFrame);
            switch (deviceButtonIndex)
            {
                case INPUT_CLICK:
                    wasReleasedInCurrentFrame = GvrControllerInput.ClickButtonUp;
                    break;
                case INPUT_TOUCH:
                    wasReleasedInCurrentFrame = GvrControllerInput.TouchUp;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("deviceButtonIndex", "deviceButtonIndex must be INPUT_CLICK or INPUT_TOUCH");
            }
            //Debug.LogWarning("wasReleasedInCurrentFrame B:" + wasReleasedInCurrentFrame);
            return wasReleasedInCurrentFrame;
        }

        public override bool GetPosition(out Vector2 position)
        {
            //position = Input.mousePosition;
            //Debug.LogWarning("GetPosition: position A:" + position);

            position = Vector2.zero;

            Vector3 laserPointerEndPoint = GetLaserPointerEndpoint();
            if (laserPointerEndPoint == Vector3.zero)
            {
                return false;
            }

            laserPointerEndPoint = EditorCamera.Instance.Camera.WorldToScreenPoint(laserPointerEndPoint);

            position.Set(laserPointerEndPoint.x, laserPointerEndPoint.y);
            //Debug.LogWarning("GetPosition: position B:" + position);

            return true;
        }

        public override bool GetPickRay(Camera camera, out Ray ray)
        {
            //Vector2 mousePosition = Input.mousePosition;
            //Debug.LogWarning("GetPickRay: mousePosition:" + mousePosition);
            //ray = camera.ScreenPointToRay(mousePosition);
            //Debug.LogWarning("GetPickRay: ray A:" + ray);

            if (_laserPointer == null)
            {
                ray = new Ray();
                return false;
            }

            /*
            Vector3 laserPointerEndPoint = GetLaserPointerEndpoint(laserPointer);
            if (laserPointerEndPoint == Vector3.zero)
            {
                ray = new Ray();
                return false;
            }

            //Debug.LogWarning("GetPickRay: lasterPointerEndPoint A:" + laserPointerEndPoint);
            laserPointerEndPoint = EditorCamera.Instance.Camera.WorldToScreenPoint(laserPointerEndPoint);
            //Debug.LogWarning("GetPickRay: laserPointerEndPoint B:" + laserPointerEndPoint);

            //ray = camera.ScreenPointToRay(mousePosition);
            ray = new Ray(laserPointerEndPoint, laserPointer.gameObject.transform.forward);
            // GvrLaserVisual..Orientation.eulerAngles);
            */
            Transform laserPointerTransform = _laserPointer.gameObject.transform;
            ray = new Ray(laserPointerTransform.position, laserPointerTransform.forward);
            //Debug.LogWarning("GetPickRay: ray B:" + ray);

            return true;
        }

        public override bool WasMoved()
        {
            return true;
            /*
            bool wasMoved = Input.GetAxis("Mouse X") != 0.0f || Input.GetAxis("Mouse Y") != 0.0f;
            //Debug.LogWarning("WasMoved: wasMoved A:" + wasMoved);

            GvrLaserPointer laserPointer = LaserPointer;
            if (laserPointer != null)
            {
                Transform laserPointerTransform = laserPointer.gameObject.transform;

                Vector3 laserPointerPosition = laserPointerTransform.position;
                Quaternion laserPointerRotation = laserPointerTransform.rotation;

                wasMoved = laserPointerPosition != _laserPointerStartPosition ||
                    laserPointerRotation != _laserPointerStartRotation;
            }
            else
            {
                wasMoved = false;
            }
            //Debug.LogWarning("WasMoved: wasMoved B:" + wasMoved);

            return wasMoved;
            */
        }

        public override void Update()
        {
            // Calculate the laserPointerEndPoint delta
            Vector2 laserPointerEndPoint;
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
                    _deltaSincePressed[input] = Vector2.zero;
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