using UnityEngine;
using UnityEngine.Events;

public class PlayerObjectCollision : MonoBehaviour {
  public UnityEvent onTriggerEvent;
  bool isCollidingWithPlayer = false;
  bool wantsToInteract = false;

  void OnTriggerEnter(Collider other) {
    if (other.CompareTag("Player")) {
      isCollidingWithPlayer = true;
    }
  }

  void OnTriggerExit(Collider other) {
    if (other.CompareTag("Player")) {
      isCollidingWithPlayer = false;
    }
  }

  void Update() {
    wantsToInteract = Input.GetButton("Interact");
  }

  void FixedUpdate() {
    if (isCollidingWithPlayer && wantsToInteract) {
      Interact();
    }
  }

  public virtual void Interact() {
    onTriggerEvent.Invoke();
  }
}
