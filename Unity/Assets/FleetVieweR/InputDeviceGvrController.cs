using UnityEngine;
using UnityEngine.EventSystems;

namespace RTEditor
{
    public class InputDeviceGvrController : InputDeviceAbstract
    {
        private GvrLaserPointer _laserPointer;
        private Vector3 _laserPointerStartPosition;
        private Quaternion _laserPointerStartRotation;

        private GvrLaserPointer LaserPointer
        {
            get
            {
                if (_laserPointer == null)
                {
                    _laserPointer = GvrPointerInputModule.Pointer as GvrLaserPointer;
                }
                return _laserPointer;
            }
        }

        private Vector3 GetLaserPointerEndpoint(GvrLaserPointer laserPointer = null)
        {
            if (laserPointer == null)
            {
                laserPointer = LaserPointer;
            }

            Vector3 laserPointerEndPoint;

            if (laserPointer == null || !laserPointer.isActiveAndEnabled)
            {
                laserPointerEndPoint = Vector3.zero;
            }
            else
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

            /*
            if (laserPointerEndPoint != null && laserPointerEndPoint != Vector3.zero)
            {
                Debug.LogWarning("GetLaserPointerEndpoint: laserPointerEndPoint:" + laserPointerEndPoint);
            }
            */

            return laserPointerEndPoint;
        }

        private void Start()
        {
            GvrLaserPointer laserPointer = LaserPointer;
            if (laserPointer != null)
            {
                Transform laserPointerTransform = laserPointer.gameObject.transform;
                _laserPointerStartPosition = laserPointerTransform.position;
                _laserPointerStartRotation = laserPointerTransform.rotation;
            }
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
            isPressed = GvrControllerInput.ClickButton;
            //Debug.LogWarning("isPressed B:" + isPressed);
            return isPressed;
        }

        public override bool WasPressedInCurrentFrame(int index)
        {
            bool wasPressedInCurrentFrame = Input.GetMouseButtonDown(index);
            //Debug.LogWarning("wasPressedInCurrentFrame A:" + wasPressedInCurrentFrame);
            wasPressedInCurrentFrame = GvrControllerInput.ClickButtonDown;
            //Debug.LogWarning("wasPressedInCurrentFrame B:" + wasPressedInCurrentFrame);
            return wasPressedInCurrentFrame;
        }

        public override bool WasReleasedInCurrentFrame(int index)
        {
            bool wasReleasedInCurrentFrame = Input.GetMouseButtonUp(index);
            //Debug.LogWarning("wasReleasedInCurrentFrame A:" + wasReleasedInCurrentFrame);
            wasReleasedInCurrentFrame = GvrControllerInput.ClickButtonUp;
            //Debug.LogWarning("wasReleasedInCurrentFrame B:" + wasReleasedInCurrentFrame);
            return wasReleasedInCurrentFrame;
        }

        public override bool GetPosition(out Vector2 position)
        {
            //position = Input.mousePosition;
            //Debug.LogWarning("GetPosition: position A:" + position);

            Vector3 laserPointerEndPoint = GetLaserPointerEndpoint();
            if (laserPointerEndPoint == Vector3.zero)
            {
                position = Vector2.zero;
                return false;
            }

            //Debug.LogWarning("GetPosition: laserPointerEndPoint A:" + laserPointerEndPoint);
            laserPointerEndPoint = EditorCamera.Instance.Camera.WorldToScreenPoint(laserPointerEndPoint);
            //Debug.LogWarning("GetPosition: laserPointerEndPoint B:" + laserPointerEndPoint);

            position = new Vector2(laserPointerEndPoint.x, laserPointerEndPoint.y);
            //Debug.LogWarning("GetPosition: position B:" + position);

            return true;
        }

        public override bool GetPickRay(Camera camera, out Ray ray)
        {
            Vector2 mousePosition = Input.mousePosition;
            //Debug.LogWarning("GetPickRay: mousePosition:" + mousePosition);
            ray = camera.ScreenPointToRay(mousePosition);
            //Debug.LogWarning("GetPickRay: ray A:" + ray);

            GvrLaserPointer laserPointer = LaserPointer;
            if (laserPointer == null)
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
            Transform laserPointerTransform = laserPointer.gameObject.transform;
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
            // Calculate the mouse delta
            Vector2 mousePos;
            GetPosition(out mousePos);
            _deltaSinceLastFrame[0] = mousePos - _previousFramePositions[0];

            // Store the current mouse position as the previous position for the next frame
            _previousFramePositions[0] = mousePos;

            // Now we will loop through all possible mouse buttons and update the mouse offset
            // since each button was pressed.
            // TODO:(pv) Handle both Touch+TouchPos & Click+Move...
            //for (int mouseBtnIndex = 0; mouseBtnIndex < 3; ++mouseBtnIndex)
            {
                // If the button was pressed or released in the current frame, there is nothing to
                // do except to reset the corresponding offset to the zero vector.
                if (WasPressedInCurrentFrame(0) ||
                    WasReleasedInCurrentFrame(0))
                {
                    _deltaSincePressed[0] = Vector2.zero;
                    //continue;
                }
                else
                {
                    // If the mouse button wasn't pressed or released, we will check to see if it is
                    // still pressed (it may have been pressed in a previous frame). If it is, we will
                    // add the delta since the last frame to get the delta since it was pressed.
                    if (IsPressed(0)) _deltaSincePressed[0] += _deltaSinceLastFrame[0];
                }
            }
        }
    }
}