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
        protected Vector2[] _previousFramePositions = new Vector2[MaxNumberOfTouches];

        /// <summary>
        /// Holds the device offsets since the last frame. For a mouse device, the first 
        /// array element will hold the mouse offset. For a touch device, there is an 
        /// element for each possible touch. For touch, when no touches exist, all elements
        /// are set to the zero vector.
        /// </summary>
        protected Vector2[] _deltaSinceLastFrame = new Vector2[MaxNumberOfTouches];

        /// <summary>
        /// For a mouse device, the first 3 elements of this array hold the mouse offset
        /// since the left, right and middle mouse buttons were pressed. For a touch device,
        /// each array element holds the offset of the touch since the touch began. For mouse
        /// buttons or touches that are not currently active, the offset is the zero vector.
        /// </summary>
        protected Vector2[] _deltaSincePressed = new Vector2[MaxNumberOfTouches];
        #endregion

        #region Public Properties
        public abstract bool UsingTouch { get; }

        public int TouchCount { get { return Input.touchCount; } }
        #endregion

        #region Public Methods
        public Vector2 GetDeltaSincePressed(int deviceButtonIndex)
        {
            if (deviceButtonIndex < 0 || deviceButtonIndex >= MaxNumberOfTouches) return Vector2.zero;
            return _deltaSincePressed[deviceButtonIndex];
        }

        public Vector2 GetDeltaSinceLastFrame(int deviceIndex)
        {
            if (deviceIndex < 0 || deviceIndex >= MaxNumberOfTouches) return Vector2.zero;
            return _deltaSinceLastFrame[deviceIndex];
        }

        public abstract bool IsPressed(int deviceButtonIndex);

        public abstract bool WasPressedInCurrentFrame(int deviceButtonIndex);

        public abstract bool WasReleasedInCurrentFrame(int deviceButtonIndex);

        public abstract bool GetPosition(out Vector2 position);

        public abstract bool GetPickRay(Camera camera, out Ray ray);

        public abstract bool WasMoved();

        public abstract void Update();
        #endregion
    }
}