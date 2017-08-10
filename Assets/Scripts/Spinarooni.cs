using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// SPINS THAT SHIT
public class Spinarooni : MonoBehaviour
{
	public float speed = 1.0f;
	[Range(0, 2)]
	public int RotationAxis = 0;
	
	// Update is called once per frame
	void Update ()
	{
		var rot = transform.localEulerAngles;
		rot[RotationAxis] += speed * Time.deltaTime;
		rot[RotationAxis] = rot[RotationAxis] % 360f;
		transform.localEulerAngles = rot;
	}
}