using UnityEngine;
using System.Collections;

public class Ladder : MonoBehaviour {
	Player player;

	void Awake () {
		player = GameObject.Find("Player").GetComponent<Player>();
	}
	
	void Update () {

	}

	void OnTriggerEnter2D(Collider2D other) {
		if (other.tag == "Player") {
			player.currentLadder = transform;
		}
	}

	void OnTriggerStay2D(Collider2D other) {
		if (other.tag == "Player") {
			player.currentLadder = transform;
		}
	}

	void OnTriggerExit2D(Collider2D other) {
		if (other.tag == "Player") {
			player.currentLadder = null;
		}
	}
}
