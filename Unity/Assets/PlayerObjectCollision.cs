using UnityEngine;
using UnityEngine.Events;

public class PlayerObjectCollision : MonoBehaviour {
  public string interactionName = "Box Interaction";
  public UnityEvent onTriggerEvent;
  bool isCollidingWithPlayer = false;

  void OnTriggerEnter(Collider other) {
    isCollidingWithPlayer = other.CompareTag("Player");
  }

  void OnTriggerExit(Collider other) {
    isCollidingWithPlayer = other.CompareTag("Player");
  }

  void FixedUpdate() {
    bool wantsToInteract = Input.GetButtonDown("Interact");

    if (isCollidingWithPlayer && wantsToInteract) {
      onTriggerEvent.Invoke();
    }
  }
}
