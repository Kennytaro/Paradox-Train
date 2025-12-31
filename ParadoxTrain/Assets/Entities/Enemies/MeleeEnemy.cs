using Unity.VisualScripting;
using UnityEngine;

public class MeleeEnemy : Enemy {
  public override void Start() {
    base.Start();
    InvokeRepeating("UpdatePath", 0, 0.2f);
    speed = 5;
    jumpForce = 0f;
    distanceFromPlayerToTrack = 30;

    controller.stepOffset = 2;  // Allows enemy to just step over instead of jumping because jumping is hard
  }

  public override void FixedUpdate() {
    Move();
    Attack();

    base.FixedUpdate();
  }

  void Move() {
    velocity.y += gravity * Time.fixedDeltaTime;
    if (controller.isGrounded && velocity.y < 0f) {
      // small clamp to prevent micro-floating
      velocity.y = -0.1f;
    }

    Debug.Log(DistanceFromPlayer());
    if (DistanceFromPlayer() <= 2) {
      velocity.x = 0;
      playerDirection = 0;
      path = null;
      return;
    } 

    velocity.x = playerDirection * speed * Time.fixedDeltaTime;
  }

  void Attack() {
    float distance = DistanceFromPlayer();
    if (distance <= distanceFromReachedGoal) {
      StartCoroutine(target.GetComponent<PlayerController>().Hurt(damage, transform.position));
    }
  }

  void UpdatePath() {
    if (isAttacking) return;
    float distance = DistanceFromPlayer();
    if (distance > distanceFromPlayerToTrack || distance <= distanceFromReachedGoal) {
      velocity.x = 0;
      playerDirection = 0;
      path = null;
      return;
    }
    
    // Update player direction
    if (target.transform.position.x >= transform.position.x) {
      playerDirection = 1;
    } else {
      playerDirection = -1;
    }

    Vector2 floorBelowEnemy = transform.position;
    Vector2 floorBelowPlayer = target.GetComponent<PlayerController>().groundPos;

    if (!controller.isGrounded) {
      Vector3 startPos = transform.position;
      startPos.z = 0;

      if (Physics.Raycast(startPos, Vector3.down, out RaycastHit hit, 10f, 1 << LayerMask.NameToLayer("Colliders"))) {
        floorBelowEnemy = hit.point;
      }
    }

    if (seeker.IsDone()) {
      seeker.StartPath(floorBelowEnemy, floorBelowPlayer, OnPathComplete);
    }
  }
}