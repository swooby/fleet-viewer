using UnityEngine;

namespace RTEditor
{
    /// <summary>
    /// This class wraps all input specific functionality and its main purpose is to
    /// relieve the client code from differentiating between Touch, Mouse, VR devices.
    /// </summary>
    public class InputDevice : MonoSingletonBase<InputDevice>, IInputDevice
    {
        private IInputDevice _inputDevice;

        public InputDevice()
        {
#if !UNITY_EDITOR && (UNITY_IOS || UNITY_ANDROID || UNITY_WP_8_1)
            SetInputDevice(new InputDeviceTouch());
#else
            SetInputDevice(new InputDeviceMouse());
#endif
        }

        public void SetInputDevice(InputDeviceAbstract inputDevice)
        {
            _inputDevice = inputDevice;
        }

        public bool UsingTouch { get { return _inputDevice.UsingTouch; } }

        public int TouchCount { get { return _inputDevice.TouchCount; } }

        public Vector2 GetDeltaSincePressed(int deviceButtonIndex)
        {
            return _inputDevice.GetDeltaSincePressed(deviceButtonIndex);
        }

        public Vector2 GetDeltaSinceLastFrame(int deviceIndex)
        {
            return _inputDevice.GetDeltaSinceLastFrame(deviceIndex);
        }

        public bool IsPressed(int deviceButtonIndex)
        {
            return _inputDevice.IsPressed(deviceButtonIndex);
        }

        public bool WasPressedInCurrentFrame(int deviceButtonIndex)
        {
            return _inputDevice.WasPressedInCurrentFrame(deviceButtonIndex);
        }

        public bool WasReleasedInCurrentFrame(int deviceButtonIndex)
        {
            return _inputDevice.WasReleasedInCurrentFrame(deviceButtonIndex);
        }

        public bool GetPosition(out Vector2 position)
        {
            return _inputDevice.GetPosition(out position);
        }

        public bool GetPickRay(Camera camera, out Ray ray)
        {
            return _inputDevice.GetPickRay(camera, out ray);
        }

        public bool WasMoved()
        {
            return _inputDevice.WasMoved();
        }

        public void Update()
        {
            _inputDevice.Update();
        }
    }
}
