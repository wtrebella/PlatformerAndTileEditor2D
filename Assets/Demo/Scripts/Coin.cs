using UnityEngine;
using System.Collections;

public class Coin : MonoBehaviour {
	public float rotationSpeed = 1;
	public float rotationSpeedVariation = 0.1f;

	protected float initialScaleX;
	protected bool scalingDown = true;

	void Awake () {
		initialScaleX = transform.localScale.x;
		rotationSpeed += Random.Range(-rotationSpeedVariation, rotationSpeedVariation);
	}

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		float newScaleX = transform.localScale.x;
		float speedMultiplier = scalingDown?-rotationSpeed:rotationSpeed;

		newScaleX += speedMultiplier * Time.deltaTime;

		if (scalingDown) {
			if (newScaleX <= -initialScaleX) {
				newScaleX = -initialScaleX;
				scalingDown = !scalingDown;
			}
		}
		else {
			if (newScaleX >= initialScaleX) {
				newScaleX = initialScaleX;
				scalingDown = !scalingDown;
			}
		}

		transform.localScale = new Vector3(newScaleX, transform.localScale.y, transform.localScale.z);
	}

	void OnTriggerEnter2D(Collider2D collider) {
		if (collider.tag == "Player") {
			collider.GetComponent<Player>().CollectCoin(this);
		}
	}
}
