using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlPlayer : MonoBehaviour
{
	public Rigidbody rb;
	public Respawn respawn;
	public Vector3 movement;
	public Vector3 forceSpeed;
	public Vector3 rotationAxis;
	bool doJump = false;
	bool isJumping = false;
	bool isRespawning = false;
	private void Update()
	{
		isRespawning = false;
		movement.z = Input.GetAxis(Axes.Vertical) * forceSpeed.z;
		movement.x = Input.GetAxis(Axes.Horizontal) * forceSpeed.x;
		if (Input.GetButton(Axes.Jump)) {
			doJump = true;
		}
		if (Input.GetKeyDown(KeyCode.R)) {
			Respawn.DoRespawn(respawn);
		}
	}

	private void FixedUpdate()
	{
		if (isRespawning)
			return;

		rb.AddForce(Vector3.forward * movement.z * Time.deltaTime, ForceMode.VelocityChange);
		if (doJump) {
			if (!isJumping) {
				rb.AddForce(Vector3.up * forceSpeed.y * Time.deltaTime, ForceMode.Impulse);
				isJumping = true;
			}
			doJump = false;
		}
		if (movement.z != 0) {
			transform.Rotate(rotationAxis * movement.x * forceSpeed.x * Time.deltaTime);
		}
	}
	private void OnCollisionEnter(Collision collision)
	{
		if (collision.collider.name == "Floor") {
			isJumping = false;
		}
	}
}