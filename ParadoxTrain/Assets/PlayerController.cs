using System.Collections;
using System.Diagnostics;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour {
  float facingDirection = 1;  // -1: Left | 1: Right
  public bool canMove = true;
  Stopwatch gameTimer = new Stopwatch();
  CharacterController controller;

  readonly float gravity = -9.81f / 3;
  Vector3 velocity;

  public float health = 100f;
  public float speed = 20f;
  public float jumpForce = 0.75f;
  float movementXDir = 0f;
  float movementYDir = 0f;

  bool hasJumped = false;
  float queuedJump = float.NegativeInfinity;
  float lastGroundedTime = float.NegativeInfinity;
  readonly float coyoteTime = 150f; // milliseconds
  readonly float queuedJumpBufferTime = 150f; // milliseconds
  
  float lastDashTime = float.NegativeInfinity;
  readonly float dashCooldown = 500f; // milliseconds
  public float dashDistance = 10f;
  bool isDashing = false;
  bool canDash;

  bool isCrouching = false;
  
  float lastHurt = float.NegativeInfinity;  // For temporary immunity after hit
  readonly float hurtCooldown = 50f;

  
  void Start() {
    controller = GetComponent<CharacterController>();
    gameTimer.Start();
  }

  void Update() {
    movementXDir = Input.GetAxisRaw("Horizontal");
    movementYDir = Input.GetAxisRaw("Vertical");

    // Queuing up a jump helps the game feel a bit more responsive
    if (Input.GetButtonDown("Jump")) {
      queuedJump = gameTimer.ElapsedMilliseconds;
    }

    canDash = gameTimer.ElapsedMilliseconds - lastDashTime >= dashCooldown;
  }

  void FixedUpdate() {
    // Functions split up so that we can modify them separately later if needed
    // Should help us in the long run    
    if (!isDashing) Jump();

    Move();
    StartCoroutine(Dash());
    Crouch();

    if (canMove) controller.Move(velocity);
  }

  void Move() {
    if (isDashing) return;
    if (movementXDir != 0) {
      facingDirection = movementXDir;
    }
    
    velocity.x = movementXDir * speed * Time.fixedDeltaTime;
  }

  IEnumerator Dash() {
    bool wantsToDash = Input.GetButtonDown("Dash");

    if (wantsToDash && canDash && !isDashing) {
      isDashing = true;
      canDash = false;

      // Disable gravity while dashing
      float storedVerticalVelocity = velocity.y;
      velocity.y = 0f;

      Vector3 dashDirection = new Vector3(facingDirection, movementYDir, 0f).normalized;
      float dashTime = 0.2f; // seconds
      float elapsed = 0f;

        while (elapsed < dashTime) {
          float t = elapsed / dashTime;
          float speedMultiplier = Mathf.Lerp(1f, 0f, t * t); // quadratic ease-out
          float frameDistance = (dashDistance * speedMultiplier / dashTime) * Time.deltaTime;

          controller.Move(dashDirection * frameDistance);

          elapsed += Time.deltaTime;
          yield return null;
        }

      // Not restoring the gravity once the dash is done
      velocity.y = storedVerticalVelocity;
      lastDashTime = gameTimer.ElapsedMilliseconds;
      isDashing = false;
    }
    yield return null;
  }

  // Also handling gravity here
  void Jump() {
    if (controller.isGrounded) {
      lastGroundedTime = gameTimer.ElapsedMilliseconds;
      if (hasJumped) hasJumped = false;
    }

    bool recentlyGrounded = gameTimer.ElapsedMilliseconds - lastGroundedTime <= coyoteTime;
    bool shouldDoQueuedJump = gameTimer.ElapsedMilliseconds - queuedJump <= queuedJumpBufferTime;
    bool IsJumpButtonPressed = Input.GetButton("Jump");

    if ((IsJumpButtonPressed || shouldDoQueuedJump) && recentlyGrounded && !hasJumped) {
      velocity.y = jumpForce;
      queuedJump = float.NegativeInfinity; // Clear the queuedjump
      hasJumped = true;
    }

    if (!isDashing) {
      velocity.y += gravity * Time.fixedDeltaTime;
    
      // small clamp to prevent micro-floating
      if (controller.isGrounded && velocity.y < 0f) {
        velocity.y = -0.1f;
      }
    }
  }

  void Crouch() {
    bool wantsToCrouch = Input.GetButton("Crouch");

    if (wantsToCrouch) {
      transform.localScale = new Vector3(1.0f, 0.7f, 1.0f);
      controller.height = 1.4f;
      controller.center = new Vector3(0, -0.25f, 0);
      isCrouching = true;
    } else if (isCrouching && !IsCeilingBlocked()) {
      transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
      controller.height = 2;
      controller.center = new Vector3(0, 0, 0);
      isCrouching = false;
    }
  }
  
  // Function to check if there is a ceiling above the player
  // that should prevent us from un-crouching.
  bool IsCeilingBlocked() {
    // Current bottom position of the controller
    Vector3 bottom = transform.position;

    // The potential "top" if we uncrouch to full height
    float standHeight = 2f; // normal standing height (match your controller default)
    float crouchHeight = 1.5f;
    float extraHeight = standHeight - crouchHeight;

    // Capsule points for the area above the player's head
    Vector3 checkStart = bottom + Vector3.up * crouchHeight;           // top of crouch
    Vector3 checkEnd = checkStart + Vector3.up * extraHeight;          // where head would be if standing

    // Perform a capsule check with same radius as controller
    return Physics.CheckCapsule(checkStart, checkEnd, controller.radius, LayerMask.GetMask("Default"), QueryTriggerInteraction.Ignore);
  }

  // Basically copy-pasted Dash function to make hurt function
  public IEnumerator Hurt(float hurtAmount, Vector2 attackerPos) {
    if (gameTimer.ElapsedMilliseconds - lastHurt >= hurtCooldown) {
      // Only hurt player after immunity period
      lastHurt = gameTimer.ElapsedMilliseconds;
      health -= hurtAmount;

      // Disable gravity while dashing
      float storedVerticalVelocity = velocity.y;
      velocity.y = 0f;

      // Move player away from hurt
      Vector3 hurtDirection = new Vector3(0, 2, 0);
      if (transform.position.x >= attackerPos.x) {
        hurtDirection.x = 2;
      } else {
        hurtDirection.x = -2;
      }
      hurtDirection = hurtDirection.normalized;

      float hurtTime = 0.2f; // seconds
      float elapsed = 0f;

        while (elapsed < hurtTime) {
          float t = elapsed / hurtTime;
          float speedMultiplier = Mathf.Lerp(1f, 0f, t * t); // quadratic ease-out
          float frameDistance = 4f * speedMultiplier / hurtTime * Time.deltaTime;

          controller.Move(hurtDirection * frameDistance);

          elapsed += Time.deltaTime;
          yield return null;
        }

      // Not restoring the gravity once the dash is done
      velocity.y = storedVerticalVelocity;
    }
    yield return null;
  }
}