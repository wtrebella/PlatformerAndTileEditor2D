using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {
	public float runSpeed = 2;
	public float jumpHeight = 2;
	public float gravity = -25;
	public float ladderClimbSpeed = 2;
	public float ladderCenteringSpeed = 0.3f;
	public AudioClip coinSound;
	public AudioClip hitSound;
	public AudioClip jumpSound;

	[HideInInspector] public Transform currentLadder;

	protected enum FacingDirection {
		Right,
		Left
	}

	protected bool lastActionWasJump = false; // eventually you might add other positive y velocity things like springs, which will turn this false
	protected int originalPlatformMask;
	protected Transform currentGroundTile;
	protected TileMap tileMap;
	protected Vector3 ladderClimbStartPos = Vector3.zero;
	protected float ladderClimbStartTime = 0;
	protected FacingDirection facingDirection = FacingDirection.Right;
	protected CharacterController2D controller;
	protected Transform playerSpriteObject;
	protected bool isClimbingLadder = false;
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
	}

	void Start() {
		
	}

	void Update() {
		rigidbody2D.velocity = Vector3.zero;
		rigidbody2D.angularVelocity = 0;

		Vector3 velocity = controller.velocity;

		if (controller.isGrounded) velocity.y = 0;

		if (Input.GetKey(KeyCode.RightArrow)) {
			velocity.x = runSpeed;
			Face(FacingDirection.Right);

			if (controller.isGrounded) {
				animator.Play(animationStateWalk);
			}
		}
		else if (Input.GetKey(KeyCode.LeftArrow)) {
			velocity.x = -runSpeed;
			Face(FacingDirection.Left);

			if (controller.isGrounded) {
				animator.Play(animationStateWalk);
			}
		}
		else {
			velocity.x = 0;

			if (controller.isGrounded) {
				animator.Play(animationStateStand);
			}
		}

		if ((controller.isGrounded || isClimbingLadder) && Input.GetKeyDown(KeyCode.Space)) {
			lastActionWasJump = true;
			animator.Play(animationStateJump);
			isClimbingLadder = false;

			if (Input.GetKey(KeyCode.DownArrow) && currentGroundTile != null && currentGroundTile.tag == "OneWayGround") {
				velocity.y = 0;
				StartCoroutine(TemporarilyTurnOffGroundCollisions(0.05f));
			}
			else {
				velocity.y = Mathf.Sqrt(2f * jumpHeight * -gravity * (isClimbingLadder?0.5f:1));
				AudioSource.PlayClipAtPoint(jumpSound, Vector3.zero, 0.1f);
			}
		}

		if (lastActionWasJump) {
			if (Input.GetKeyUp(KeyCode.Space) && controller.velocity.y > 0) {
				velocity.y *= 0.35f;
			}
		}

		velocity.y += gravity * Time.deltaTime;

		if (currentLadder != null && Mathf.Abs(currentLadder.position.x - transform.position.x) > tileMap.tileSize * 2) {
			// this is a hack to make sure the ladder is nullified if the player is nowhere near it.
			// for some reason, sometimes OnTriggerExit doesn't get called so the reference stays.
			currentLadder = null;
		}

		if (currentLadder != null) {
			if (Input.GetKeyDown(KeyCode.UpArrow)) {
				if (!isClimbingLadder) {
					ladderClimbStartPos = transform.position;
					ladderClimbStartTime = Time.time;
				}

				isClimbingLadder = true;
			}

			if (isClimbingLadder) {
				if (!animator.GetCurrentAnimatorStateInfo(0).IsName("PlayerClimb")) animator.Play(animationStateClimb);

				velocity.x = 0;
				velocity.y = 0;

				Vector3 goalPosCenter = new Vector3(currentLadder.position.x, transform.position.y, transform.position.z);

				controller.move(Vector3.Lerp(ladderClimbStartPos, goalPosCenter, (Time.time - ladderClimbStartTime) / ladderCenteringSpeed) - transform.position);

				if (Input.GetKey(KeyCode.UpArrow)) {
					velocity.y += ladderClimbSpeed;
				}
				if (Input.GetKey(KeyCode.DownArrow)) {
					velocity.y -= ladderClimbSpeed;
				}

				BoxCollider2D ladderCollider = (BoxCollider2D)currentLadder.collider2D;
				float maxLadderY = currentLadder.position.y + ladderCollider.size.y / 2f + ladderCollider.center.y;

				if (currentLadder.tag == "LadderTop") {
					if (transform.position.y >= maxLadderY) {
						velocity.y = Mathf.Min(0, velocity.y);

						Vector3 goalPosTop = new Vector3(transform.position.x, maxLadderY, transform.position.z);
						controller.move(Vector3.Lerp(ladderClimbStartPos, goalPosTop, (Time.time - ladderClimbStartTime) / ladderCenteringSpeed) - transform.position);
					}
				}
			}
		}

		controller.move(velocity * Time.deltaTime);

		if (!previouslyWasGrounded && controller.isGrounded) AudioSource.PlayClipAtPoint(hitSound, Vector3.zero);
		previouslyWasGrounded = controller.isGrounded;
	}

	IEnumerator TemporarilyTurnOffGroundCollisions(float time) {
		originalPlatformMask = controller.platformMask;
		controller.platformMask = 0;

		yield return new WaitForSeconds(time);

		controller.platformMask = originalPlatformMask;
	}

	void Face(FacingDirection dir) {
		if (facingDirection == dir) return;

		facingDirection = dir;
		transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
	}

	void HandleControllerCollidedEvent(RaycastHit2D raycastHit) {
		if (controller.isGrounded) {
			isClimbingLadder = false;
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
