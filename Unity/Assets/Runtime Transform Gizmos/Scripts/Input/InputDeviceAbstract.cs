using UnityEngine;

namespace RTEditor
{
    /// <summary>
    /// This class wraps all input specific functionality and its main purpose is to
    /// relieve the client code from differentiating between Touch, Mouse, VR devices.
    /// </summary>
    public abstract class InputDeviceAbstract : MonoSingletonBase<InputDeviceAbstract>
    {
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

        #region Public Static Properties
        /// <summary>
        /// Returns the maximum number of allowed touches for a touch device. A random
        /// value of 10 was used in this case as for the moment I can not figure out how
        /// to retrieve this value using the Unity API.
        /// </summary>
        public static int MaxNumberOfTouches { get { return 10; } }
        #endregion

        #region Public Properties
        public abstract bool UsingTouch { get; }

        public int TouchCount { get { return Input.touchCount; } }
        #endregion

        #region Public Methods
        /// <summary>
        /// Returns the device offset since the specified device button was pressed.
        /// </summary>
        /// <param name="deviceButtonIndex">
        /// For a mouse, this has to be a value in the [0, 2] interval and it identifies
        /// one of the mouse buttons. For a touch device, this value can reside in the 
        /// [0, MaxNumberOfTouches] interval and it identifies a touch.
        /// </param>
        /// <returns>
        /// The device offset since the specified device button was pressed. For example,
        /// if a mouse is used and the value passed is 0, the method will return the mouse
        /// offset since the left mouse button was pressed. For a touch device, this will
        /// return the offset of the touch with index 'deviceButtonIndex' since the touch began. 
        /// If the specified device button is not pressed, the method will return the zero vector.
        /// The same value is returned if the specified index is out of range. The allowed
        /// interval for this value is [0, MaxNumberOfTouches - 1].
        /// </returns>
        public Vector2 GetDeltaSincePressed(int deviceButtonIndex)
        {
            if (deviceButtonIndex < 0 || deviceButtonIndex >= MaxNumberOfTouches) return Vector2.zero;
            return _deltaSincePressed[deviceButtonIndex];
        }

        /// <summary>
        /// Returns the device offset since the last frame.
        /// </summary>
        /// <param name="deviceIndex">
        /// Identifies the device. For a mouse this has to be set to 0. Otherwise, this 
        /// represents the index of a touch.
        /// </param>
        /// <returns>
        /// The offset of the device since the last frame. When a touch device is used,
        /// if the touch with index 'deviceIndex' is not present, the returned value is
        /// the zero vector. The same value is returned if the specified device index
        /// is out of range. The allowed interval for this value is [0, MaxNumberOfTouches - 1].
        /// </returns>
        public Vector2 GetDeltaSinceLastFrame(int deviceIndex)
        {
            if (deviceIndex < 0 || deviceIndex >= MaxNumberOfTouches) return Vector2.zero;
            return _deltaSinceLastFrame[deviceIndex];
        }

        /// <summary>
        /// Can be used to check if the device button with the specified index is
        /// currently pressed.
        /// </summary>
        /// <param name="deviceButtonIndex">
        /// Identifies the device button. For a mouse this has to reside in the 
        /// [0, 2] interval (left, right, middle mouse button). For touch, this
        /// is the index of a touch.
        /// </param>
        /// <returns>
        /// True if the specified button is pressed and false otherwise.
        /// </returns>
        public abstract bool IsPressed(int deviceButtonIndex);

        /// <summary>
        /// Can be used to check if the device button with the specified index was
        /// pressed in the current frame.
        /// </summary>
        /// <param name="deviceButtonIndex">
        /// Identifies the device button. For a mouse this has to reside in the 
        /// [0, 2] interval (left, right, middle mouse button). For touch, this
        /// is the index of a touch.
        /// </param>
        /// <returns>
        /// True if the specified button is pressed and false otherwise.
        /// </returns>
        public abstract bool WasPressedInCurrentFrame(int deviceButtonIndex);

        /// <summary>
        /// Can be used to check if the device button with the specified index was
        /// released in the current frame.
        /// </summary>
        /// <param name="deviceButtonIndex">
        /// Identifies the device button. For a mouse this has to reside in the 
        /// [0, 2] interval (left, right, middle mouse button). For a touch device, this
        /// is the index of a touch.
        /// </param>
        /// <returns>
        /// True if the specified button is pressed and false otherwise.
        /// </returns>
        public abstract bool WasReleasedInCurrentFrame(int deviceButtonIndex);

        /// <summary>
        /// Returns the device position. For a mouse this is the mouse cursor position
        /// and for a touch device this is the position of the first touch.
        /// </summary>
        /// <param name="position">
        /// The device position. If the method returns false, this position will have
        /// both components set to 'float.MaxValue' and should be ignored.
        /// </param>
        /// <returns>
        /// True if the position can be retreived and false otherwise. The method can
        /// return false if a touch device is used and no touches are available.
        /// </returns>
        public abstract bool GetPosition(out Vector2 position);

        /// <summary>
        /// Returns a pick ray from the device position. This ray can be used to pick entities
        /// in the scene. For a mouse device the ray is constructed using the mouse cursor
        /// position. For a touch device, the ray is constructed using the first available
        /// touch.
        /// </summary>
        /// <param name="ray">
        /// The pick ray. If the method returns false, this ray will have a zero vector origin
        /// and a zero vector direction and should be ignored.
        /// </param>
        /// <returns>
        /// True if the ray can be constructed and false otherwise. The method can return false
        /// if a touch device is used and no touches are available.
        /// </returns>
        public abstract bool GetPickRay(Camera camera, out Ray ray);

        /// <summary>
        /// Can be used to check if the device was moved. For a mouse device, this
        /// method will return true when the mouse cursor is moved. For a mobile
        /// device it returns true only if the first touch was moved.
        /// </summary>
        /// <returns></returns>
        public abstract bool WasMoved();

        /// <summary>
        /// Called every frame to perform any necessary updates.
        /// </summary>
        public abstract void Update();
        #endregion
    }
}