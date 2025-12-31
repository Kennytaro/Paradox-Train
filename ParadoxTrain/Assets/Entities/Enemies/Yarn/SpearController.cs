using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SpearController : MonoBehaviour {
  readonly float damage = 10f;
  public float speed = 10f;

  private float dist, nextX, baseY, height;
  private Vector2 startPos, targetPos;
  public bool launching = false;

  void Start() {
    startPos = transform.position;
    GetComponent<SpriteRenderer>().flipY = true;
  }

  void Update() {
    if (Input.GetButtonDown("Fire1")) {
      Launch();
    }
  }

  void FixedUpdate() {
    if (!launching) return;
  
    dist = targetPos.x - startPos.x;
    nextX = Mathf.Lerp(transform.position.x, targetPos.x, speed * Time.fixedDeltaTime);
    baseY = Mathf.Lerp(startPos.y, targetPos.y, (nextX - startPos.x) / dist);
    height = (float)(2 * (nextX - startPos.x) * (nextX - targetPos.x) / (-0.25 * dist * dist));

    Vector3 movePosition = new Vector3(nextX, baseY + height, transform.position.z);
    transform.rotation = LookAtTargetBottom(transform, targetPos);
    transform.position = movePosition;
  }

  Quaternion LookAtTargetBottom(Transform self, Vector2 target) {
    Vector2 direction = target - (Vector2)self.position;

    float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

    // Rotate so DOWN (-Y) points at target
    return Quaternion.Euler(0f, 0f, angle - 90f);
  }


  public void Launch() {
    if (launching) return;

    targetPos = FindFirstObjectByType<PlayerController>().transform.position;
    launching = true;
  }

  void OnTriggerEnter(Collider other) {
    if (other.CompareTag("Player")) {
      // Attack player
      StartCoroutine(FindFirstObjectByType<PlayerController>().GetComponent<PlayerController>().Hurt(damage, transform.position));
      launching = false;
      startPos = transform.position;
    }
  }
}
