using UnityEngine;
using UnityEngine.Events;

public class PlayerObjectCollision : MonoBehaviour {
  public string interactionName = "Box Interaction";
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
    Debug.Log(isCollidingWithPlayer + " | " + wantsToInteract);

    if (isCollidingWithPlayer && wantsToInteract) {
      Debug.Log("Talking!");
      onTriggerEvent.Invoke();
    }
  }
}
