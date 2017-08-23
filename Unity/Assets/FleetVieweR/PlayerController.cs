using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    public float Speed = 5.0f;

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
        Transform cameraMainTransform = Camera.main.transform;
        Vector3 cameraMainTransformForward = WorldToLocal(cameraMainTransform.forward);
        Vector3 cameraMainTransformRight = WorldToLocal(cameraMainTransform.right);
        Vector3 cameraMainTransformUp = WorldToLocal(cameraMainTransform.up);
        Vector2 deltaPosCentered = Vector2.zero;
        Vector3 deltaTransform = Vector3.zero;

        float speed = Speed * (Input.GetKey(KeyCode.LeftShift) ? 2.0f : 1.0f);

        if (isMoving)
        {
            if (isTouching)
            {
                deltaPosCentered = touchPosCentered - startTouchCentered;

                transform.Translate(cameraMainTransformForward * deltaPosCentered.y * speed * deltaTime, Space.Self);
                transform.Translate(cameraMainTransformRight * deltaPosCentered.x * speed * deltaTime, Space.Self);
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
            transform.Translate(cameraMainTransformForward * speed * deltaTime, Space.Self);
        }
        if (Input.GetKey(KeyCode.S))
        {
            HasEverMoved = true;
            transform.Translate(cameraMainTransformForward * -speed * deltaTime, Space.Self);
        }
        if (Input.GetKey(KeyCode.A))
        {
            HasEverMoved = true;
            transform.Translate(cameraMainTransformRight * -speed * deltaTime, Space.Self);
        }
        if (Input.GetKey(KeyCode.D))
        {
            HasEverMoved = true;
            transform.Translate(cameraMainTransformRight * speed * deltaTime, Space.Self);
        }
        if (Input.GetKey(KeyCode.Q))
        {
            HasEverMoved = true;
            transform.Rotate(cameraMainTransformForward, 4.0f * speed * deltaTime, Space.Self);
        }
        if (Input.GetKey(KeyCode.E))
        {
            HasEverMoved = true;
            transform.Rotate(cameraMainTransformForward, 4.0f * -speed * deltaTime, Space.Self);
        }
        if (Input.GetKey(KeyCode.LeftControl))
        {
            HasEverMoved = true;
            transform.Translate(cameraMainTransformUp * -speed * deltaTime, Space.Self);
        }
        if (Input.GetKey(KeyCode.Space))
        {
            HasEverMoved = true;
            transform.Translate(cameraMainTransformUp * speed * deltaTime, Space.Self);
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
