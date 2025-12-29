using System.Collections;
using System.Diagnostics;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Animator))]
public class Entity : MonoBehaviour {
  public float facingDirection = 1;  // -1: Left | 1: Right
  public CharacterController controller;
  public SpriteRenderer spriteRenderer;
  public Animator anim;
  public Stopwatch gameTimer = new Stopwatch();
  public float jumpForce = 0.5f;
  public float gravity = -9.81f / 2;

  public Vector2 velocity;

  public float health = 100f;
  public float speed = 10f;

  float lastHurt = float.NegativeInfinity;  // For temporary immunity after hit
  public readonly float hurtCooldown = 50f;
  
  public virtual void Start() {
    controller = GetComponent<CharacterController>();
    spriteRenderer = GetComponent<SpriteRenderer>();
    anim = GetComponent<Animator>();
    gameTimer.Start();
  }

  public virtual void FixedUpdate() {
    bool facingRight = facingDirection == 1;
    GetComponent<SpriteRenderer>().flipX = !facingRight;

    controller.Move(velocity);
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