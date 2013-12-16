using UnityEngine;
using System.Collections;

public class CameraFollow : MonoBehaviour {
	public GameObject objectToFollow;

	void Start () {
	
	}
	
	void LateUpdate () {
		if (objectToFollow != null) transform.position = new Vector3(objectToFollow.transform.position.x, objectToFollow.transform.position.y, transform.position.z);
	}
}
