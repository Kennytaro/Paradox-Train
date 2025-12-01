using UnityEngine;
using Pathfinding;

public class MeleeEnemy : Enemy {
  public float jumpNodeHeightRequirement = 0.8f;

  public override void Start() {
    base.Start();

    InvokeRepeating("UpdatePath", 0, 0.2f);
    jumpForce = 0.75f;
  }

  public override void FixedUpdate() {
    if (path == null) {
      velocity.y += gravity * Time.fixedDeltaTime;
      base.FixedUpdate();
      return;
    }

    reachedEndOfPath = currentWaypointIndex >= path.vectorPath.Count;
    if (reachedEndOfPath) {
      velocity.x = 0; // Stop moving horizontally
      velocity.y += gravity * Time.fixedDeltaTime; // Still fall
      base.FixedUpdate();
      return;
    }
    
    Move();
    Jump();

    base.FixedUpdate();
  }

  void OnPathComplete(Path p) {
    if (!p.error) {
      path = p;
      currentWaypointIndex = 0;
    }
  }

  void UpdatePath() {
    if (isAttacking) return;
    if (target.transform.position.x >= transform.position.x) {
      playerDirection = 1;
    } else {
      playerDirection = -1;
    }

    Vector2 floorBelowEnemy = target.transform.position;
    Vector2 floorBelowPlayer = target.transform.position;

    Vector3 startPos = transform.position;
    Vector3 endPos = target.transform.position;
    startPos.z = 0;
    endPos.z = 0;

    RaycastHit hit;
    if (Physics.Raycast(startPos, Vector3.down, out hit, 10f, 1 << LayerMask.NameToLayer("Colliders"))) {
      floorBelowEnemy = hit.point;
    }

    if (Physics.Raycast(endPos, Vector3.down, out hit, 10f, 1 << LayerMask.NameToLayer("Colliders"))) {
      floorBelowPlayer = hit.point;
    }

    if (seeker.IsDone()) {
      seeker.StartPath(floorBelowEnemy, floorBelowPlayer, OnPathComplete);
    }
  }

  void Jump() {
    if (controller.isGrounded) {
      // Reset gravity accumulation when on ground
      Vector2 waypoint = path.vectorPath[currentWaypointIndex];
      
      // Check vertical distance specifically
      float verticalDistance = waypoint.y - transform.position.y;

      // Only jump if the node is significantly higher than us
      if (verticalDistance > 0) {
        velocity.y = jumpForce;
      }
    } else {
      // Apply Gravity
      velocity.y += gravity * Time.fixedDeltaTime;
    }
  }

  void Move() {
    Vector2 waypoint = path.vectorPath[currentWaypointIndex];

    Vector2 floorBelowEnemy = target.transform.position;
    Vector3 startPos = transform.position;
    startPos.z = 0;

    RaycastHit hit;
    if (Physics.Raycast(startPos, Vector3.down, out hit, 10f, 1 << LayerMask.NameToLayer("Colliders"))) {
      floorBelowEnemy = hit.point;
    }

    Vector2 direction = (waypoint - (Vector2)floorBelowEnemy).normalized;
    velocity.x = direction.x * speed * Time.fixedDeltaTime;

    // Check distance to next node
    float distance = Vector2.Distance(waypoint, transform.position);
    if (distance < nextWaypointDistance) {
      currentWaypointIndex++;
    }
  }
}