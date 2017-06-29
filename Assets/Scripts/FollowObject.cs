using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowObject : MonoBehaviour
{
	public Transform follow;
	public Vector3 offset;
	public float smoothRatio;

	// Use this for initialization
	void Start ()
	{
		if (follow == null) {
			this.enabled = false;
		}
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (follow == null)
			return;

		var fromPos = transform.position;
		var toPos = follow.position + offset;
		if (fromPos == toPos)
			return;

		if (smoothRatio > 0) {
			transform.position = Vector3.MoveTowards(fromPos, toPos, smoothRatio * Time.deltaTime);
		}
		else {
			transform.position = toPos;
		}
	}
}
