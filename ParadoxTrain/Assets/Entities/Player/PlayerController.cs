using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(SpriteRenderer))]
public class PlayerController : Entity {
  public bool canMove = true;

  float movementXDir = 0f;
  float movementYDir = 0f;

  public float damage = 5;
  public float attackRadius = 2f;
  public LayerMask enemyLayer;

  bool hasJumped = false;
  float queuedJump = float.NegativeInfinity;
  float lastGroundedTime = float.NegativeInfinity;
  public Vector2 groundPos;
  readonly float coyoteTime = 150f; // milliseconds
  readonly float queuedJumpBufferTime = 150f; // milliseconds
  
  float lastDashTime = float.NegativeInfinity;
  readonly float dashCooldown = 500f; // milliseconds
  public float dashDistance = 2.5f;
  bool isDashing = false;
  bool canDash;

  bool isCrouching = false;
  
  public override void Start() {
    base.Start();
    speed = 10f;
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

  public override void FixedUpdate() {
    // Functions split up so that we can modify them separately later if needed
    // Should help us in the long run    
    if (!isDashing) Jump();

    Move();
    StartCoroutine(Dash());
    Crouch();
    Attack();

    if (canMove) base.FixedUpdate();
  }

  void Move() {
    if (isDashing) return;
    if (movementXDir != 0) {
      facingDirection = movementXDir;
    }
    
    anim.SetBool("isRunning", movementXDir != 0);
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
      if (Input.GetButton("Vertical") && movementXDir == 0) {
        dashDirection.x = 0;
        dashDirection.y = 1;
      }

      float dashTime = 0.25f; // seconds
      float elapsed = 0f;

        while (elapsed < dashTime) {
          float t = elapsed / dashTime;
          float speedMultiplier = Mathf.Lerp(1f, 0f, t * t); // quadratic ease-out
          float frameDistance = (dashDistance * speedMultiplier / dashTime) * Time.fixedDeltaTime;

          controller.Move(dashDirection * frameDistance);

          elapsed += Time.fixedDeltaTime;
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

      if (controller.isGrounded) {
        groundPos = transform.position;
      } else {
        Vector3 pos = transform.position;
        pos.z = 0;

        if (Physics.Raycast(pos, Vector3.down, out RaycastHit hit, 10f, 1 << LayerMask.NameToLayer("Colliders"))) {
          groundPos = hit.point;
        }

        // Leveling stuff so it's perfectly centered (I think that's why)
        // Basically it keeps it in the same level as {transform.position} would
        groundPos.y += (controller.height / 2) + controller.skinWidth;
      }

      // small clamp to prevent micro-floating
      if (controller.isGrounded && velocity.y < 0f) {
        velocity.y = -0.1f;
      }
    }
  }

  void Crouch() {
    bool wantsToCrouch = Input.GetButton("Crouch");

    if (wantsToCrouch) {
      controller.height = 1.4f;
      controller.center = new Vector3(0, -0.3f, 0);
      isCrouching = true;
    } else if (isCrouching && !IsCeilingBlocked()) {
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
    float crouchHeight = 1.4f;
    float extraHeight = standHeight - crouchHeight;

    // Capsule points for the area above the player's head
    Vector3 checkStart = bottom + Vector3.up * crouchHeight;           // top of crouch
    Vector3 checkEnd = checkStart + Vector3.up * extraHeight;          // where head would be if standing

    // Perform a capsule check with same radius as controller
    return Physics.CheckCapsule(checkStart, checkEnd, controller.radius, LayerMask.GetMask("Colliders"), QueryTriggerInteraction.Ignore);
  }

  void Attack() {
    bool pressedHurt = Input.GetButtonDown("Fire1");
    if (!pressedHurt) return;

    Collider[] hits = Physics.OverlapSphere(transform.position, attackRadius, enemyLayer);
    foreach (Collider enemy in hits) {
      Vector2 relativePos = enemy.transform.position - transform.position;

      // Check if enemy is on the correct side (left/right)
      if ((facingDirection == 1 && relativePos.x >= 0) || (facingDirection == -1 && relativePos.x <= 0)) {
        // Check vertical range
        if (Mathf.Abs(relativePos.y) <= controller.height + attackRadius) {    // 2 (player height) + 1.5 (attack range)
          // Enemy is within swing
          StartCoroutine(enemy.GetComponent<Enemy>().Hurt(damage, transform.position));
        }
      }
    }
  }
}