using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    public float VelocityMetersPerSecond = 5.0f;

    public Text controllerDebugText;

    public static bool HasEverMoved { get; private set; }

    public static bool HasNeverMoved
    {
        get
        {
            return !HasEverMoved;
        }
    }

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

    private Vector3 WorldToLocal(Vector3 vector)
    {
        return transform.worldToLocalMatrix.MultiplyVector(vector);
    }

    private void FixedUpdate()
    {
		//
		// TODO:(pv) Detect double click and switch to/from flight<->point mode.
		// http://answers.unity3d.com/questions/331545/double-click-mouse-detection-.html
		//

		// TODO:(pv) Momentum
		// TODO:(pv) float scroll = Input.GetAxis("Mouse ScrollWheel") to control speed

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
        Transform cameraMainTransform = Camera.main.transform;
        Vector3 cameraMainTransformForward = WorldToLocal(cameraMainTransform.forward);
        Vector3 cameraMainTransformRight = WorldToLocal(cameraMainTransform.right);
        Vector3 cameraMainTransformUp = WorldToLocal(cameraMainTransform.up);
        Vector2 deltaPosCentered = Vector2.zero;
        Vector3 deltaTransform = Vector3.zero;

        float deltaDistance = VelocityMetersPerSecond * (Input.GetKey(KeyCode.LeftShift) ? 3.0f : 1.0f) * Time.fixedDeltaTime;

        Vector3 translate = Vector3.zero;
        float rotate = 0.0f;

        if (isMoving)
        {
            if (isTouching)
            {
                deltaPosCentered = touchPosCentered - startTouchCentered;

                translate += deltaPosCentered.y * cameraMainTransformForward * deltaDistance;
                translate += deltaPosCentered.x * cameraMainTransformRight * deltaDistance;
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
                HasEverMoved = true;
                isMoving = true;
                startTouchCentered = touchPosCentered;
            }
        }

        if (Input.GetKey(KeyCode.W))
        {
            HasEverMoved = true;
            translate += cameraMainTransformForward * deltaDistance;
        }
        if (Input.GetKey(KeyCode.S))
        {
            HasEverMoved = true;
            translate -= cameraMainTransformForward * deltaDistance;
        }
        if (Input.GetKey(KeyCode.A))
        {
            HasEverMoved = true;
            translate -= cameraMainTransformRight * deltaDistance;
        }
        if (Input.GetKey(KeyCode.D))
        {
            HasEverMoved = true;
            translate += cameraMainTransformRight * deltaDistance;
        }
        if (Input.GetKey(KeyCode.Q))
        {
            HasEverMoved = true;
            rotate += 4.0f * deltaDistance;
        }
        if (Input.GetKey(KeyCode.E))
        {
            HasEverMoved = true;
            rotate -= 4.0f * deltaDistance;
        }
        if (Input.GetKey(KeyCode.LeftControl))
        {
            HasEverMoved = true;
            translate -= cameraMainTransformUp * deltaDistance;
        }
        if (Input.GetKey(KeyCode.Space))
        {
            HasEverMoved = true;
            translate += cameraMainTransformUp * deltaDistance;
        }

        if (translate != Vector3.zero)
        {
            transform.Translate(translate, Space.Self);
        }

        if (Math.Abs(rotate) > float.Epsilon)
        {
            transform.Rotate(cameraMainTransformForward, rotate, Space.Self);
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
            message += "\ntransform.position: " + transform.position;
            message += "\ntransform.forward: " + transform.forward;
            message += "\ntransform.right: " + transform.right;
            message += "\ntransform.up: " + transform.up;
            message += "\ndeltaPosCentered: " + deltaPosCentered;

            controllerDebugText.text = message;
        }
    }
}
