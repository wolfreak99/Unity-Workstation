using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Respawn : MonoBehaviour
{
	public static void DoRespawn(Respawn rs)
	{
		if (rs == null)
			return;

		rs.enabled = true;
	}

	private bool isSpawnSet = false;
	private Vector3 position;
	private Vector3 rotation;
	private Vector3 scale;
	[SerializeField]
	private Transform respawn;
	[SerializeField]
	private Rigidbody rb;

	private void SaveSpawnPoint()
	{
		position = respawn.localPosition;
		rotation = respawn.localEulerAngles;
		scale = respawn.localScale;
		isSpawnSet = true;
	}
	private void LoadSpawnPoint()
	{
		if (!isSpawnSet) {
			Debug.LogWarning("Spawn info not set for object", gameObject);
		}

		respawn.localPosition = position;
		respawn.localEulerAngles = rotation;
		respawn.localScale = scale;
		if (rb != null) {
			rb.velocity = Vector3.zero;
			rb.angularVelocity = Vector3.zero;
			rb.Sleep();
		}
	}
	// Use this for initialization
	private void Start()
	{
		SaveSpawnPoint();
		enabled = false;
	}
	private void OnEnable()
	{
		if (!isSpawnSet)
			return;
		LoadSpawnPoint();
		enabled = false;
	}
}