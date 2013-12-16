using UnityEngine;
using System.Collections;

// you need to add this to objects if you want them to interact with colliders and triggers and stuff
// even if they're not being controlled by physics

[RequireComponent (typeof(Rigidbody2D))]
public class NonMovingRigidbody : MonoBehaviour {

	void Awake () {
		if (rigidbody2D == null) gameObject.AddComponent<Rigidbody2D>();
		rigidbody2D.gravityScale = 0;
		rigidbody2D.fixedAngle = true;
	}

	void Start () {
	
	}
	
	void FixedUpdate () {
		// so the physics don't actually control anything
		rigidbody2D.velocity = Vector3.zero;
		rigidbody2D.angularVelocity = 0;
	}
}
