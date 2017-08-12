using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
	public float Speed = 0.1f;

	public Text controllerDebugText;

    private bool isMoving;
    private Vector2 startTouchCentered;

    /*
    private void Update()
    {
        if (controllerDebugText == null)
        {
            return;
        }

        if (GvrControllerInput.AppButtonUp)
        {
            GameObject gameObject = controllerDebugText.canvas.gameObject;
            gameObject.SetActive(!gameObject.activeSelf);
        }
    }
    */

    private void FixedUpdate()
    {
        //
        // TODO:(pv) Detect double click and switch to/from flight<->point mode.
        // http://answers.unity3d.com/questions/331545/double-click-mouse-detection-.html
        //

        // TODO:(pv) Shift down doubles [or more] speed
        // TODO:(pv) Momentum
        // TODO:(pv) Roll (yaw and pitch are controlled by mouse input or VR headset)

        float deltaTime = Time.fixedDeltaTime;

        GvrConnectionState connectionState = GvrControllerInput.State;
        GvrControllerBatteryLevel batteryLevel = GvrControllerInput.BatteryLevel;
        bool isTouching = GvrControllerInput.IsTouching;
		Vector2 touchPosCentered = GvrControllerInput.TouchPosCentered;
		bool clickButton = GvrControllerInput.ClickButton;
		bool appButton = GvrControllerInput.AppButton;
        bool homeButtonState = GvrControllerInput.HomeButtonState;
        Quaternion orientation = GvrControllerInput.Orientation;
        Vector3 accel = GvrControllerInput.Accel;
        Vector3 gyro = GvrControllerInput.Gyro;
		Vector3 transformPosition = transform.position;
        Vector3 transformForward = transform.forward;
		Vector3 transformRight = transform.right;
		Vector3 cameraMainTransformPosition = Camera.main.transform.position;
		Vector3 cameraMainTransformForward = Camera.main.transform.forward;
		Vector3 cameraMainTransformRight = Camera.main.transform.right;
		Vector3 cameraMainTransformUp = Camera.main.transform.up;
		Vector2 deltaPosCentered = Vector2.zero;
		Vector3 deltaTransform = Vector3.zero;

		if (isMoving)
        {
            if (isTouching)
			{
                deltaPosCentered = touchPosCentered - startTouchCentered;

                transform.Translate(cameraMainTransformForward * deltaPosCentered.y * Speed * deltaTime);
                transform.Translate(cameraMainTransformRight * deltaPosCentered.x * Speed * deltaTime);
			}
			else
			{
                isMoving = false;
                startTouchCentered = Vector2.zero;
			}
		}
        else
        {
			if (isTouching)
			{
                isMoving = true;
                startTouchCentered = touchPosCentered;
			}
        }

		if (Input.GetKey(KeyCode.W))
		{
            transform.Translate(cameraMainTransformForward * Speed * deltaTime);
		}
		if (Input.GetKey(KeyCode.S))
		{
			transform.Translate(cameraMainTransformForward * -Speed * deltaTime);
		}
		if (Input.GetKey(KeyCode.A))
		{
			transform.Translate(cameraMainTransformRight * -Speed * deltaTime);
		}
		if (Input.GetKey(KeyCode.D))
		{
			transform.Translate(cameraMainTransformRight * Speed * deltaTime);
		}
		if (Input.GetKey(KeyCode.Q))
		{
			transform.Translate(cameraMainTransformUp * -Speed * deltaTime);
		}
		if (Input.GetKey(KeyCode.E))
		{
			transform.Translate(cameraMainTransformUp * Speed * deltaTime);
		}

		if (controllerDebugText != null)
		{
            String message;
			message = "Connection: " + connectionState;
			message += "\nBatteryLevel: " + batteryLevel;
			message += "\nIsTouching: " + isTouching;
			message += "\nTouchPosCentered: " + touchPosCentered;
			message += "\nClickButton: " + clickButton;
			message += "\nAppButton: " + appButton;
			message += "\nHomeButtonState: " + homeButtonState;
			message += "\nOrientation: " + orientation;
			message += "\nAccel: " + accel;
			message += "\nGyro: " + gyro;
			message += "\ntransform.position: " + transformPosition;
			message += "\ntransform.forward: " + transformForward;
			message += "\ntransform.right: " + transformRight;
			message += "\ncameraMainTransformPosition: " + cameraMainTransformPosition;
			message += "\ncameraMainTransformForward: " + cameraMainTransformForward;
			message += "\ncameraMainTransformRight: " + cameraMainTransformRight;
			message += "\ndeltaPosCentered: " + deltaPosCentered;

			controllerDebugText.text = message;
		}
	}
}
