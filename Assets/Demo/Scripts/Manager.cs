using UnityEngine;
using System.Collections;

public class Manager : MonoBehaviour {
	public float timeScale {
		get {return Time.timeScale;}
		set {Time.timeScale = value;}
	}

	public Vector3 drag = Vector3.zero;
	public float gravity = -50;
	public bool playerIsAlive = true;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if (!playerIsAlive) {
			if (Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Return)) Application.LoadLevel(0);
		}
	}
}
