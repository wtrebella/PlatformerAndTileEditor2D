using UnityEngine;
using System.Collections;

public class Fireball : MonoBehaviour {
	public float rotationSpeed;
	[HideInInspector] public Direction facingDirection;
	public float movementSpeed;
	public float variation;
	public AudioClip killSound;

	protected Vector3 velocity;

	void Awake() {
		movementSpeed = Random.Range(movementSpeed - variation, movementSpeed + variation);
		rotationSpeed = Random.Range(rotationSpeed - variation, rotationSpeed + variation);
	}

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		transform.Rotate(Quaternion.Euler(0, 0, -rotationSpeed * Time.deltaTime).eulerAngles);

		if (facingDirection == Direction.Right) {
			transform.position = new Vector3(transform.position.x + movementSpeed * Time.deltaTime, transform.position.y, transform.position.z);
		}

		else if (facingDirection == Direction.Left) {
			transform.position = new Vector3(transform.position.x - movementSpeed * Time.deltaTime, transform.position.y, transform.position.z);
		}

		if (Mathf.Abs(transform.position.x - Camera.main.transform.position.x) > 50) Destroy(gameObject);
	}

	void OnTriggerEnter2D(Collider2D coll) {
		if (coll.tag == "MadGuy") {
			Destroy(coll.gameObject);
			KillFireball();
		}
	}

	void KillFireball() {
		Destroy(this.gameObject);
		AudioSource.PlayClipAtPoint(killSound, Vector3.zero);
		ParticleSystem p = GameObject.Find("Explosion Particles").GetComponent<ParticleSystem>();
		p.startColor = Color.red;
		p.transform.position = transform.position;
		p.Play();
	}
}
