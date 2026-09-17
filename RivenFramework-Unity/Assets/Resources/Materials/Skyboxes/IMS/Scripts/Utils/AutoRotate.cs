using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoRotate : MonoBehaviour
{
	public Vector3 rotationSpeed;
	
	void Update()
	{        
		transform.Rotate(rotationSpeed * Time.deltaTime, Space.Self);
	}
}
