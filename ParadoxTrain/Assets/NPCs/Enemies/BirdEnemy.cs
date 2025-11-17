using UnityEngine;
using Pathfinding;

[System.Serializable]
[RequireComponent(typeof(Seeker))]
[RequireComponent(typeof(Rigidbody))]
public class BirdEnemy : Enemy {
  // -1: Player is to the enemy's left
  // 1: Player is to the enemy's right
  public GameObject target;
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
  Rigidbody rb;
  
  void Start() {
    speed = 7f;
    seeker = GetComponent<Seeker>();
    rb = GetComponent<Rigidbody>();
  
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
    Transform targetTransform = target.transform;

    if (targetTransform.position.x >= rb.position.x) {
      playerDirection = 1;
    } else {
      playerDirection = -1;
    }

    Vector2 targetPosition = new Vector2(targetTransform.position.x + (5 * -playerDirection), targetTransform.position.y + 5f);

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

    Vector2 direction = (waypoint - (Vector2)rb.position).normalized;
    Vector2 desiredVelocity = direction * targetSpeed;

    Vector2 acceleration = (desiredVelocity - (Vector2)rb.linearVelocity) * speed;

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
      P0 = (Vector2)rb.position;              // start
      P1 = (Vector2)target.transform.position + Vector2.down * 4; // lowest point/peak of stoop
      P2 = new Vector2(
        P1.x + 15f * playerDirection,        // exit forward
        P1.y + 10f                           // go above player after attack
      );

      // stop pathfinding during attack and clear any existing velocity
      rb.linearVelocity = Vector3.zero;
    }

    // Stoop movement - use direct position setting for precision
    stoopT += Time.fixedDeltaTime / stoopDuration;
    stoopT = Mathf.Clamp01(stoopT);

    // Calculate exact position on bezier curve
    Vector2 curveTarget2D = GetBezierPoint(P0, P1, P2, stoopT);
    Vector3 curveTarget = new Vector3(curveTarget2D.x, curveTarget2D.y, rb.position.z);
    
    // Use MovePosition for smooth, collision-respecting movement
    rb.MovePosition(curveTarget);

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
    rb.linearVelocity = Vector3.zero;
    
    // Force immediate path update
    UpdatePath();
  }

  Vector2 GetBezierPoint(Vector2 a, Vector2 b, Vector2 c, float t) {
    float u = 1f - t;
    return (u * u * a) + (2f * u * t * b) + (t * t * c);
  }

  void OnTriggerEnter(Collider other) {
    if (other.CompareTag("Player")) {
      // Attack player
      StartCoroutine(target.GetComponent<PlayerController>().Hurt(damage, transform.position));
    }
  }
}