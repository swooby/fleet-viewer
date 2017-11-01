using UnityEngine;

namespace RTEditor
{
    public abstract class InputDeviceAbstract : IInputDevice
    {
        #region Public Static Properties
        /// <summary>
        /// Returns the maximum number of allowed touches for a touch device. A random
        /// value of 10 was used in this case as for the moment I can not figure out how
        /// to retrieve this value using the Unity API.
        /// </summary>
        public static int MaxNumberOfTouches { get { return 10; } }
        #endregion

        #region Protected Variables
        /// <summary>
        /// Holds the device positions in the previous frame. For a mouse dvice, the
        /// first array element will hold the mouse position. For a touch device, there 
        /// is an element for each possible touch.
        /// </summary>
        protected Vector3[] _previousFramePositions = new Vector3[MaxNumberOfTouches];

        /// <summary>
        /// Holds the device offsets since the last frame. For a mouse device, the first 
        /// array element will hold the mouse offset. For a touch device, there is an 
        /// element for each possible touch. For touch, when no touches exist, all elements
        /// are set to the zero vector.
        /// </summary>
        protected Vector3[] _deltaSinceLastFrame = new Vector3[MaxNumberOfTouches];

        /// <summary>
        /// For a mouse device, the first 3 elements of this array hold the mouse offset
        /// since the left, right and middle mouse buttons were pressed. For a touch device,
        /// each array element holds the offset of the touch since the touch began. For mouse
        /// buttons or touches that are not currently active, the offset is the zero vector.
        /// </summary>
        protected Vector3[] _deltaSincePressed = new Vector3[MaxNumberOfTouches];
        #endregion

        #region IInputDevice

        public abstract bool UsingTouch { get; }

        public int TouchCount { get { return Input.touchCount; } }

        public Vector3 GetDeltaSincePressed(int deviceButtonIndex)
        {
            if (deviceButtonIndex < 0 || deviceButtonIndex >= MaxNumberOfTouches) return Vector3.zero;
            return _deltaSincePressed[deviceButtonIndex];
        }

        public Vector3 GetDeltaSinceLastFrame(int deviceButtonIndex)
        {
            if (deviceButtonIndex < 0 || deviceButtonIndex >= MaxNumberOfTouches) return Vector3.zero;
            return _deltaSinceLastFrame[deviceButtonIndex];
        }

        public abstract bool IsPressed(int deviceButtonIndex);

        public abstract bool WasPressedInCurrentFrame(int deviceButtonIndex);

        public abstract bool WasReleasedInCurrentFrame(int deviceButtonIndex);

        public abstract bool GetPosition(out Vector3 position);

        public abstract bool GetPickRay(Camera camera, out Ray ray);

        public abstract bool WasMoved();

        public abstract void Update();

        #endregion IInputDevice
    }
}