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

        void SetInputDevice(InputDeviceAbstract inputDevice)
        {
            _inputDevice = inputDevice;
        }

        bool IInputDevice.UsingTouch { get { return _inputDevice.UsingTouch; } }

        int IInputDevice.TouchCount { get { return _inputDevice.TouchCount; } }

        Vector2 IInputDevice.GetDeltaSincePressed(int deviceButtonIndex)
        {
            return _inputDevice.GetDeltaSincePressed(deviceButtonIndex);
        }

        Vector2 IInputDevice.GetDeltaSinceLastFrame(int deviceIndex)
        {
            return _inputDevice.GetDeltaSinceLastFrame(deviceIndex);
        }

        bool IInputDevice.IsPressed(int deviceButtonIndex)
        {
            return _inputDevice.IsPressed(deviceButtonIndex);
        }

        bool IInputDevice.WasPressedInCurrentFrame(int deviceButtonIndex)
        {
            return _inputDevice.WasPressedInCurrentFrame(deviceButtonIndex);
        }

        bool IInputDevice.WasReleasedInCurrentFrame(int deviceButtonIndex)
        {
            return _inputDevice.WasReleasedInCurrentFrame(deviceButtonIndex);
        }

        bool IInputDevice.GetPosition(out Vector2 position)
        {
            return _inputDevice.GetPosition(out position);
        }

        bool IInputDevice.GetPickRay(Camera camera, out Ray ray)
        {
            return _inputDevice.GetPickRay(camera, out ray);
        }

        bool IInputDevice.WasMoved()
        {
            return _inputDevice.WasMoved();
        }

        void IInputDevice.Update()
        {
            _inputDevice.Update();
        }
    }
}
