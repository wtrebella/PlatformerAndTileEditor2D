using UnityEngine;
using System.Collections.Generic;
using System.Collections;

// this is in development. it doesn't do anything yet.

public class AlternatinglyMovingObject : MonoBehaviour {
	[HideInInspector] public Vector3 velocity;

	public Direction initialDirection = Direction.Up;
	public float velocityMultiplier = 10;
	public float travelDist = 3;
	
	private bool isGoingForward = true;
	private Vector3 initialNormal;
	private Vector3 initialPos;
	private Vector3 endPos;

	void Awake () {
		initialPos = transform.position;

		if (initialDirection == Direction.Up) initialNormal = new Vector3(0, 1, 0);
		if (initialDirection == Direction.Right) initialNormal = new Vector3(1, 0, 0);
		if (initialDirection == Direction.Left) initialNormal = new Vector3(-1, 0, 0);
		if (initialDirection == Direction.Down) initialNormal = new Vector3(0, -1, 0);

		endPos = initialPos + initialNormal * travelDist;
		velocity = initialNormal * velocityMultiplier;
	}
	
	void LateUpdate () {
		// extraDelta is added to all the stuff that's on touching the platform to prevent jitter

		Vector3 newPos = transform.position + velocity * Time.deltaTime;
		bool needsSwitch = false;

		if (isGoingForward) {
			if ((newPos - initialPos).magnitude > travelDist) {
				newPos = initialPos + initialNormal * travelDist;
				needsSwitch = true;
			}
		}
		else {
			if ((endPos - newPos).magnitude > travelDist) {
				newPos = initialPos;
				needsSwitch = true;
			}
		}

		if (needsSwitch) {
			velocity *= -1;
			isGoingForward = !isGoingForward;
			needsSwitch = false;
		}

		transform.position = newPos;
	}
}
