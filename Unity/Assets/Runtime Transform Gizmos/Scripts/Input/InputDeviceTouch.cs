using UnityEngine;

namespace RTEditor
{
    public class InputDeviceTouch : InputDeviceAbstract
    {
        public override bool UsingTouch
        {
            get
            {
                return true;
            }
        }

        public override bool IsPressed(int deviceButtonIndex)
        {
            return deviceButtonIndex < Input.touchCount;
        }

        public override bool WasPressedInCurrentFrame(int index)
        {
            if (index >= Input.touchCount) return false;
            return Input.GetTouch(index).phase == TouchPhase.Began;
        }

        public override bool WasReleasedInCurrentFrame(int index)
        {
            if (index >= Input.touchCount) return false;
            Touch touch = Input.GetTouch(index);
            return touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled;
        }

        public override bool GetPosition(out Vector2 position)
        {
            position = new Vector2(float.MaxValue, float.MaxValue);
            if (Input.touchCount != 0)
            {
                position = Input.GetTouch(0).position;
                return true;
            }
            return false;
        }

        public override bool GetPickRay(Camera camera, out Ray ray)
        {
            ray = new Ray(Vector3.zero, Vector3.zero);
            if (Input.touchCount != 0)
            {
                Touch touch = Input.GetTouch(0);
                ray = camera.ScreenPointToRay(touch.position);
                return true;
            }
            return false;
        }

        public override bool WasMoved()
        {
            if (Input.touchCount != 0)
            {
                Touch touch = Input.GetTouch(0);
                return touch.phase == TouchPhase.Moved;
            }
            return false;
        }

        public override void Update()
        {
            int touchCount = Input.touchCount;
            for (int touchIndex = 0; touchIndex < touchCount; ++touchIndex)
            {
                if (touchIndex >= touchCount)
                {
                    _deltaSinceLastFrame[0] = Vector2.zero;
                    _previousFramePositions[0] = Vector2.zero;
                    continue;
                }

                // If the touch has begun or ended in the current frame, there is nothing to
                // do except to reset the corresponding offset to the zero vector.
                if (WasPressedInCurrentFrame(touchIndex) ||
                    WasReleasedInCurrentFrame(touchIndex))
                {
                    _deltaSincePressed[touchIndex] = Vector2.zero;
                    continue;
                }
                else
                {
                    // If the touch is still active, we will add the delta since the last frame to get the
                    // delta since it was pressed.
                    if (IsPressed(touchIndex)) _deltaSincePressed[touchIndex] += _deltaSinceLastFrame[touchIndex];
                }
            }
        }
    }
}