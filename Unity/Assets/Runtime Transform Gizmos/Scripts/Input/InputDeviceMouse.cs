using UnityEngine;

namespace RTEditor
{
    public class InputDeviceMouse : InputDeviceAbstract
    {
        public override bool UsingTouch
        {
            get
            {
                return false;
            }
        }

        public override bool IsPressed(int deviceButtonIndex)
        {
            return Input.GetMouseButton(deviceButtonIndex);
        }

        public override bool WasPressedInCurrentFrame(int index)
        {
            return Input.GetMouseButtonDown(index);
        }

        public override bool WasReleasedInCurrentFrame(int index)
        {
            return Input.GetMouseButtonUp(index);
        }

        public override bool GetPosition(out Vector2 position)
        {
            position = Input.mousePosition;
            return true;
        }

        public override bool GetPickRay(Camera camera, out Ray ray)
        {
            ray = camera.ScreenPointToRay(Input.mousePosition);
            return true;
        }

        public override bool WasMoved()
        {
            return Input.GetAxis("Mouse X") != 0.0f || Input.GetAxis("Mouse Y") != 0.0f;
        }

        public override void Update()
        {
            // Calculate the mouse delta
            Vector2 mousePos = Input.mousePosition;
            _deltaSinceLastFrame[0] = mousePos - _previousFramePositions[0];

            // Store the current mouse position as the previous position for the next frame
            _previousFramePositions[0] = mousePos;

            // Now we will loop through all possible mouse buttons and update the mouse offset
            // since each button was pressed.
            for (int mouseBtnIndex = 0; mouseBtnIndex < 3; ++mouseBtnIndex)
            {
                // If the button was pressed or released in the current frame, there is nothing to
                // do except to reset the corresponding offset to the zero vector.
                if (WasPressedInCurrentFrame(mouseBtnIndex) ||
                    WasReleasedInCurrentFrame(mouseBtnIndex))
                {
                    _deltaSincePressed[mouseBtnIndex] = Vector2.zero;
                    continue;
                }
                else
                {
                    // If the mouse button wasn't pressed or released, we will check to see if it is
                    // still pressed (it may have been pressed in a previous frame). If it is, we will
                    // add the delta since the last frame to get the delta since it was pressed.
                    if (IsPressed(mouseBtnIndex)) _deltaSincePressed[mouseBtnIndex] += _deltaSinceLastFrame[0];
                }
            }
        }
    }
}