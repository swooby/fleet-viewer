﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

	public int Speed = 5;

	void UpdateFixed()
	{
		float xAxisValue = Input.GetAxis("Horizontal") * Speed;
		float zAxisValue = Input.GetAxis("Vertical") * Speed;
		float yValue = 0.0f;

		if (Input.GetKey(KeyCode.Q))
		{
			yValue = -Speed;
		}
		if (Input.GetKey(KeyCode.E))
		{
			yValue = Speed;
		}

		transform.position = new Vector3(transform.position.x + xAxisValue, transform.position.y + yValue, transform.position.z + zAxisValue);
	}
}
