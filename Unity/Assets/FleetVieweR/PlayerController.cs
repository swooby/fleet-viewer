using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour {

    public float Speed = 0.0001f;

	public Text controllerDebugText;

    private bool isMoving;
    private Vector2 startTouchCentered;

    private void Update()
    {
        UpdateControllerDebugText();
    }

    private void UpdateControllerDebugText()
    {
        if (controllerDebugText == null)
        {
            return;
        }

        String message = "";

        message += "\nIsTouching: " + GvrControllerInput.IsTouching;
		message += "\nTouchPosCentered: " + GvrControllerInput.TouchPosCentered;
		message += "\nClickButton: " + GvrControllerInput.ClickButton;
		message += "\nAppButton: " + GvrControllerInput.AppButton;
		message += "\nHomeButtonState: " + GvrControllerInput.HomeButtonState;
		message += "\nOrientation: " + GvrControllerInput.Orientation;
		message += "\nAccel: " + GvrControllerInput.Accel;
		message += "\nGyro: " + GvrControllerInput.Gyro;

        controllerDebugText.text = message;
	}

    private void FixedUpdate()
    {
		//
		// TODO:(pv) Detect double click and switch to/from flight<->point mode.
		// http://answers.unity3d.com/questions/331545/double-click-mouse-detection-.html
		//

        if (isMoving)
        {
			if (GvrControllerInput.IsTouching)
			{
                Vector2 touchPosCentered = GvrControllerInput.TouchPosCentered;
                Vector2 deltaPosCentered = startTouchCentered - touchPosCentered;

				float xAxisValue = -deltaPosCentered.x * Speed;
				float zAxisValue = -deltaPosCentered.y * Speed;
				float yAxisValue = 0.0f;//...

				/*
				if (Input.GetKey(KeyCode.Q))
				{
					yValue = -Speed;
				}
				if (Input.GetKey(KeyCode.E))
				{
					yValue = Speed;
				}
				*/

				Vector3 delta = new Vector3(xAxisValue, yAxisValue, zAxisValue);

                transform.position += delta;
			}
			else
			{
                isMoving = false;
                startTouchCentered = Vector2.zero;
			}

		}
        else
        {
			if (GvrControllerInput.IsTouching)
			{
                isMoving = true;
				startTouchCentered = GvrControllerInput.TouchPosCentered;
			}
        }
	}
}
