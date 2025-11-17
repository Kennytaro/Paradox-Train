using UnityEngine;
using Pathfinding;

[System.Serializable]
[RequireComponent(typeof(Seeker))]
[RequireComponent(typeof(Rigidbody2D))]
public class BirdEnemy : Enemy {
  // -1: Player is to the enemy's left
  // 1: Player is to the enemy's right
  public Transform target;
  public float nextWaypointDistance = 3f;
  
  Path path;
  float playerDirection = -1;
  int currentWaypointIndex = 0;
  bool reachedEndOfPath = false;
  
  bool isAttacking = false;
  float stoopT = 0f;
  float stoopDuration = 1.2f; // tweak
  bool stooping = false;
  Vector2 P0, P1, P2;

  Seeker seeker;
  Rigidbody2D rb;
  
  void Start() {
    speed = 7f;
    seeker = GetComponent<Seeker>();
    rb = GetComponent<Rigidbody2D>();
  
    InvokeRepeating("UpdatePath", 0, 0.2f);
  }

  void Update() {
    if (path == null) return;
    if (currentWaypointIndex >= path.vectorPath.Count) return;
  }

  void FixedUpdate() {
    // Add a small delay after stoop before checking path again
    if (stooping) {
      Stoop();
      return;
    }
    
    if (!reachedEndOfPath) {
      Move();
    } else {
      Stoop();
    }
  }

  void OnPathComplete(Path p) {
    if (!p.error) {
      path = p;
      currentWaypointIndex = 0;
    }
  }

  void UpdatePath() {
    if (stooping || isAttacking) return;

    if (target.position.x >= rb.position.x) {
      playerDirection = 1;
    } else {
      playerDirection = -1;
    }

    Vector2 targetPosition = new Vector2(target.position.x + (5 * -playerDirection), target.position.y + 5f);

    if (seeker.IsDone()) {
      seeker.StartPath(rb.position, targetPosition, OnPathComplete);
    }
  }

  void Move() {
    if (path == null) return;

    reachedEndOfPath = currentWaypointIndex >= path.vectorPath.Count;
    if (reachedEndOfPath) return;

    Vector2 waypoint = path.vectorPath[currentWaypointIndex];
    float distance = Vector2.Distance(rb.position, waypoint);

    // deadzone to not jitter when near target
    float deadZone = 1f;
    if (distance < deadZone) {
      rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, Vector2.zero, 6f * Time.fixedDeltaTime);
      currentWaypointIndex++;
      return;
    }

    // deceleration logic
    float slowRadius = 3f;
    float targetSpeed = Mathf.Clamp01(distance / slowRadius) * speed;

    Vector2 direction = (waypoint - rb.position).normalized;
    Vector2 desiredVelocity = direction * targetSpeed;

    Vector2 acceleration = (desiredVelocity - rb.linearVelocity) * speed;

    rb.AddForce(acceleration);

    // speed limiter
    rb.linearVelocity = Vector2.ClampMagnitude(rb.linearVelocity, speed);

    if (distance < nextWaypointDistance) {
      currentWaypointIndex++;
    }
  }

  // Bird attack
  void Stoop() {
    // Only run once to avoid overriding path
    if (!stooping) {
      stooping = true;
      stoopT = 0f;

      // Cache the curve points
      P0 = rb.position;                       // start
      P1 = (Vector2)target.position + Vector2.down * 4; // lowest point/peak of stoop
      P2 = new Vector2(
        P1.x + 15f * playerDirection,        // exit forward
        P1.y + 10f                           // go above player after attack
      );

      // stop pathfinding during attack and clear any existing velocity
      rb.linearVelocity = Vector2.zero;
    }

    // Stoop movement - use direct position setting for precision
    stoopT += Time.fixedDeltaTime / stoopDuration;
    stoopT = Mathf.Clamp01(stoopT);

    // Calculate exact position on bezier curve
    Vector2 curveTarget = GetBezierPoint(P0, P1, P2, stoopT);
    
    // Move directly to the position (bypasses physics for precision)
    rb.MovePosition(curveTarget);

    // Optional: Add visual feedback for speed
    // This doesn't affect movement, just makes it feel faster
    float visualSpeedBoost = 1.5f;
    rb.linearVelocity = (curveTarget - (Vector2)transform.position).normalized * (speed * visualSpeedBoost);

    // End stoop attack
    if (stoopT >= 1f) {
      ResetAfterStoop();
    }
  }

  void ResetAfterStoop() {
    stooping = false;
    isAttacking = false;
    reachedEndOfPath = false;
    path = null;
    currentWaypointIndex = 0;
    
    // Clear any residual velocity
    rb.linearVelocity = Vector2.zero;
    
    // Force immediate path update
    UpdatePath();
  }

  Vector2 GetBezierPoint(Vector2 a, Vector2 b, Vector2 c, float t) {
    float u = 1f - t;
    return (u * u * a) + (2f * u * t * b) + (t * t * c);
  }
}