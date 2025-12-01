using UnityEngine;
using Pathfinding;

[RequireComponent(typeof(Seeker))]
public class BirdEnemy : Enemy {  
  float stoopT = 0f;
  float stoopDuration = 10f; // tweak
  bool stooping = false;
  Vector2 P0, P1, P2;

  void Start() {
    speed = 5f;
    seeker = GetComponent<Seeker>();
  
    InvokeRepeating("UpdatePath", 0, 0.2f);
  }

  void Update() {
    if (path == null) return;
    if (currentWaypointIndex >= path.vectorPath.Count) return;
  }

  public override void FixedUpdate() {
    // Add a small delay after stoop before checking path again
    if (stooping) {
      velocity = CalculateStoopVelocity();
    } else if (!reachedEndOfPath) {
      velocity = CalculatePathFollowingVelocity();
    } else {
      velocity = CalculateStoopVelocity();
    }
    
    // Apply velocity to rigidbody
    // rb.linearVelocity = velocity;
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

    // if (targetTransform.position.x >= rb.position.x) {
    //   playerDirection = 1;
    // } else {
    //   playerDirection = -1;
    // }

    Vector2 targetPosition = new Vector2(targetTransform.position.x + (5 * -playerDirection), targetTransform.position.y + 5f);

    if (seeker.IsDone()) {
      // seeker.StartPath(rb.position, targetPosition, OnPathComplete);
    }
  }

  Vector2 CalculatePathFollowingVelocity() {
    if (path == null) return Vector2.zero;

    reachedEndOfPath = currentWaypointIndex >= path.vectorPath.Count;
    if (reachedEndOfPath) return Vector2.zero;

    Vector2 waypoint = path.vectorPath[currentWaypointIndex];
    // float distance = Vector2.Distance(rb.position, waypoint);

    // deadzone to not jitter when near target
    // float deadZone = 1f;
    // if (distance < deadZone) {
    //   // Gradually slow down when approaching waypoint
    //   Vector2 slowedVelocity = Vector2.Lerp(velocity, Vector2.zero, 6f * Time.fixedDeltaTime);
    //   currentWaypointIndex++;
    //   return slowedVelocity;
    // }

    // deceleration logic
    // float slowRadius = 3f;
    // float targetSpeed = Mathf.Clamp01(distance / slowRadius) * speed;

    // Vector2 direction = (waypoint - (Vector2)rb.position).normalized;
    // Vector2 desiredVelocity = direction * targetSpeed;

    // Smooth velocity changes
    // Vector2 acceleration = (desiredVelocity - velocity) * speed;
    // Vector2 newVelocity = velocity + acceleration * Time.fixedDeltaTime;

    // speed limiter
    // newVelocity = Vector2.ClampMagnitude(newVelocity, speed);

    // if (distance < nextWaypointDistance) {
    //   currentWaypointIndex++;
    // }
    
    // return newVelocity;
    return Vector2.zero;
  }

  // Bird attack - now returns velocity instead of directly moving
  Vector2 CalculateStoopVelocity() {
    // Only run once to avoid overriding path
    if (!stooping) {
      stooping = true;
      stoopT = 0f;

      // Cache the curve points
      // P0 = (Vector2)rb.position;              // start
      P1 = (Vector2)target.transform.position + Vector2.down * 4; // lowest point/peak of stoop
      P2 = new Vector2(
        P1.x + 15f * playerDirection,        // exit forward
        P1.y + 10f                           // go above player after attack
      );
    }

    // Stoop movement - calculate velocity based on bezier curve with smooth acceleration/deceleration
    stoopT += Time.fixedDeltaTime / stoopDuration;
    stoopT = Mathf.Clamp01(stoopT);

    // Calculate the eased t value for smooth acceleration and deceleration
    float easedT;
    if (stoopT < 0.5f) {
      // First half: accelerate (ease in)
      easedT = 2f * stoopT * stoopT;
    } else {
      // Second half: decelerate (ease out)
      easedT = 1f - 2f * (1f - stoopT) * (1f - stoopT);
    }

    // Get positions at current and next eased frames
    Vector2 currentPos = GetBezierPoint(P0, P1, P2, easedT);
    
    // Calculate next eased t value
    float nextT = Mathf.Clamp01(stoopT + Time.fixedDeltaTime / stoopDuration);
    float nextEasedT;
    if (nextT < 0.5f) {
      nextEasedT = 2f * nextT * nextT;
    } else {
      nextEasedT = 1f - 2f * (1f - nextT) * (1f - nextT);
    }
    
    Vector2 nextPos = GetBezierPoint(P0, P1, P2, nextEasedT);
    
    // Velocity is the difference between positions
    Vector2 stoopVelocity = (nextPos - currentPos) / Time.fixedDeltaTime;

    // Alternative approach: Direct velocity control with easing
    // This gives you more direct control over the speed curve
    /*
    float speedMultiplier;
    if (stoopT < 0.5f) {
      // Accelerate toward vertex (0 to max speed)
      speedMultiplier = Mathf.Lerp(0f, 1f, stoopT * 2f);
    } else {
      // Decelerate after vertex (max speed to 0)
      speedMultiplier = Mathf.Lerp(1f, 0f, (stoopT - 0.5f) * 2f);
    }
    
    // Get direction from bezier derivative
    Vector2 tangent = GetBezierTangent(P0, P1, P2, easedT);
    Vector2 stoopVelocity = tangent.normalized * speedMultiplier * maxStoopSpeed;
    */

    // End stoop attack
    if (stoopT >= 1f) {
      ResetAfterStoop();
    }
    
    return stoopVelocity;
  }

  // Helper method to calculate the tangent/derivative of the bezier curve
  // This gives you the direction of movement at any point on the curve
  Vector2 GetBezierTangent(Vector2 a, Vector2 b, Vector2 c, float t) {
    // Derivative of quadratic bezier: B'(t) = 2(1-t)(b-a) + 2t(c-b)
    return 2f * (1f - t) * (b - a) + 2f * t * (c - b);
  }
  
  void ResetAfterStoop() {
    stooping = false;
    isAttacking = false;
    reachedEndOfPath = false;
    path = null;
    currentWaypointIndex = 0;
    
    // Force immediate path update
    UpdatePath();
  }

  Vector2 GetBezierPoint(Vector2 a, Vector2 b, Vector2 c, float t) {
    float u = 1f - t;
    return (u * u * a) + (2f * u * t * b) + (t * t * c);
  }
}