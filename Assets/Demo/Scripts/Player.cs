using UnityEngine;
using System.Collections;

public enum Direction {
	Right,
	Left,
	Up,
	Down
}

public class Player : MonoBehaviour {
	public float runSpeed = 2;
	public float jumpHeight = 2;
	public float climbSpeed = 2;
	public float centeringSpeed = 0.3f;
	public AudioClip coinSound;
	public AudioClip hitSound;
	public AudioClip jumpSound;
	public AudioClip owSound;
	public AudioClip fireballSound;
	public GameObject fireballPrefab;
	public AudioClip frozenSound;

	[HideInInspector] public Transform currentClimbable;
	[HideInInspector] public Direction facingDirection = Direction.Right;

	protected Manager manager;
	protected bool lastActionWasJump = false; // eventually you might add other positive y velocity things like springs, which will turn this false
	protected int originalPlatformMask;
	protected Transform currentGroundTile;
	protected TileMap tileMap;
	protected Vector3 climbStartPos = Vector3.zero;
	protected float climbStartTime = 0;
	protected CharacterController2D controller;
	protected Transform playerSpriteObject;
	protected bool isClimbing = false;
	protected Animator animator;
	protected int animationStateWalk;
	protected int animationStateStand;
	protected int animationStateJump;
	protected int animationStateClimb;
	protected bool previouslyWasGrounded = true;

	void Awake() {
		controller = GetComponent<CharacterController2D>();
		controller.onControllerCollidedEvent += HandleControllerCollidedEvent;
		playerSpriteObject = GameObject.Find("Player Sprite").transform;

		tileMap = GameObject.Find("TileMap").GetComponent<TileMap>();

		animationStateWalk = Animator.StringToHash("PlayerWalk");
		animationStateStand = Animator.StringToHash("PlayerStand");
		animationStateJump = Animator.StringToHash("PlayerJump");
		animationStateClimb = Animator.StringToHash("PlayerClimb");

		animator = playerSpriteObject.GetComponent<Animator>();

		manager = GameObject.Find("Manager").GetComponent<Manager>();
	}

	void Start() {
		
	}

	void Update() {
		if (Input.GetKeyDown(KeyCode.F)) ShootFireball();
		
		Vector3 velocity = controller.velocity;
		
		ApplyDrag(ref velocity);
		UpdateWalking(ref velocity);
		UpdateClimbing(ref velocity);
		UpdateJumping(ref velocity);
		ApplyGravity(ref velocity);
		
		controller.move(velocity * Time.deltaTime);
		
		if (!previouslyWasGrounded && controller.isGrounded) AudioSource.PlayClipAtPoint(hitSound, Vector3.zero);
		previouslyWasGrounded = controller.isGrounded;
	}

	void ApplyDrag(ref Vector3 velocity) {
		if (velocity.x > 0) velocity.x = Mathf.Max(velocity.x - manager.drag.x, 0);
		if (velocity.x < 0) velocity.x = Mathf.Min(velocity.x + manager.drag.x, 0);
		if (velocity.y > 0) velocity.y = Mathf.Max(velocity.y - manager.drag.y, 0);
		if (velocity.y < 0) velocity.y = Mathf.Min(velocity.y + manager.drag.y, 0);
	}

	void UpdateWalking(ref Vector3 velocity) {
		if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)) {
			velocity.x = runSpeed;
			
			Face(Direction.Right);
			
			if (controller.isGrounded) animator.Play(animationStateWalk);
		}
		else if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A)) {
			velocity.x = -runSpeed;
			
			Face(Direction.Left);
			
			if (controller.isGrounded) animator.Play(animationStateWalk);
		}
		else if (controller.isGrounded) {
			animator.Play(animationStateStand);
		}
	}

	void UpdateClimbing(ref Vector3 velocity) {
		if (currentClimbable != null && Mathf.Abs(currentClimbable.position.x - transform.position.x) > tileMap.tileSize * 2) {
			// this is a hack to make sure the climbable is nullified if the player is nowhere near it.
			// for some reason, sometimes OnTriggerExit doesn't get called so the reference stays.
			currentClimbable = null;
		}

		if (currentClimbable != null) {
			if (Input.GetKey(KeyCode.UpArrow)) {
				if (!isClimbing) {
					climbStartPos = transform.position;
					climbStartTime = Time.time;
				}
				
				isClimbing = true;
			}
			
			if (isClimbing) {
				if (!animator.GetCurrentAnimatorStateInfo(0).IsName("PlayerClimb")) animator.Play(animationStateClimb);
				
				velocity.x = 0;
				velocity.y = 0;
				
				Vector3 goalPosCenter = new Vector3(currentClimbable.position.x, transform.position.y, transform.position.z);
				
				controller.move(Vector3.Lerp(climbStartPos, goalPosCenter, (Time.time - climbStartTime) / centeringSpeed) - transform.position);
				
				if (Input.GetKey(KeyCode.UpArrow)) {
					velocity.y += climbSpeed;
				}
				if (Input.GetKey(KeyCode.DownArrow)) {
					velocity.y -= climbSpeed;
				}
				
				BoxCollider2D climbableCollider = (BoxCollider2D)currentClimbable.collider2D;
				float maxClimbableY = currentClimbable.position.y + climbableCollider.size.y / 2f + climbableCollider.center.y;
				
				if (currentClimbable.tag == "ClimbableTop") {
					if (transform.position.y >= maxClimbableY) {
						velocity.y = Mathf.Min(0, velocity.y);
						
						Vector3 goalPosTop = new Vector3(transform.position.x, maxClimbableY, transform.position.z);
						controller.move(Vector3.Lerp(climbStartPos, goalPosTop, (Time.time - climbStartTime) / centeringSpeed) - transform.position);
					}
				}
			}
		}
	}

	void UpdateJumping(ref Vector3 velocity) {
		if ((controller.isGrounded || isClimbing) && (Input.GetKeyDown(KeyCode.Space))) {
			lastActionWasJump = true;
			animator.Play(animationStateJump);
			isClimbing = false;
			
			if (Input.GetKey(KeyCode.DownArrow) && currentGroundTile != null && currentGroundTile.gameObject.layer == LayerMask.NameToLayer("OneWayGround")) {
				velocity.y = 0;
				StartCoroutine(TemporarilyTurnOffGroundCollisions(0.05f));
			}
			else {
				velocity.y = Mathf.Sqrt(2f * jumpHeight * -manager.gravity * (isClimbing?0.5f:1));
				AudioSource.PlayClipAtPoint(jumpSound, Vector3.zero, 0.1f);
			}
		}

		// cut jump short if you release space early
		if (lastActionWasJump) {
			if (Input.GetKeyUp(KeyCode.Space) && controller.velocity.y > 0) {
				velocity.y *= 0.35f;
			}
		}
	}

	void ApplyGravity(ref Vector3 velocity) {
		velocity.y += manager.gravity * Time.deltaTime;
	}

	IEnumerator TemporarilyTurnOffGroundCollisions(float time) {
		originalPlatformMask = controller.platformMask;
		controller.platformMask = 0;

		yield return new WaitForSeconds(time);

		controller.platformMask = originalPlatformMask;
	}

	public void Die() {
		Destroy(this.gameObject);
		AudioSource.PlayClipAtPoint(owSound, Vector3.zero, 0.6f);
		ParticleSystem p = GameObject.Find("Explosion Particles").GetComponent<ParticleSystem>();
		p.startColor = new Color(166f/255f, 210f/255f, 194f/255f);
		p.transform.position = transform.position;
		p.Play();
		manager.playerIsAlive = false;
	}

	void Face(Direction dir) {
		if (facingDirection == dir) return;

		facingDirection = dir;
		transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
	}
	
	public void ShootFireball() {
		AudioSource.PlayClipAtPoint(fireballSound, Vector3.zero, 0.3f);
		GameObject fireballObject = (GameObject)Instantiate(fireballPrefab, new Vector3(transform.position.x, transform.position.y, fireballPrefab.transform.position.z), Quaternion.identity);
		Fireball fireball = fireballObject.GetComponent<Fireball>();
		fireball.facingDirection = facingDirection;
	}

	void HandleControllerCollidedEvent(RaycastHit2D raycastHit) {
		if (controller.isGrounded) {
			isClimbing = false;
			lastActionWasJump = false;
			currentGroundTile = raycastHit.collider.transform;
		}
		else currentGroundTile = null;
	}

	public void CollectCoin(Coin coin) {
		AudioSource.PlayClipAtPoint(coinSound, Vector3.zero);
		Destroy(coin.gameObject);
	}
}
