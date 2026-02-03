using UnityEngine;

public class Bobble : MonoBehaviour {
  [Header("Bobble Amounts")]
  public float verticalAmplitude = 0.3f;
  public float horizontalAmplitude = 0.15f;

  [Header("Bobble Speeds")]
  public float verticalSpeed = 1.5f;
  public float horizontalSpeed = 1.0f;

  private Vector3 startPosition;

  void Start() {
    startPosition = transform.position;
  }

  void Update() {
    float verticalOffset = Mathf.Sin(Time.time * verticalSpeed) * verticalAmplitude;
    float horizontalOffset = Mathf.Cos(Time.time * horizontalSpeed) * horizontalAmplitude;

    transform.position = startPosition + new Vector3(horizontalOffset, verticalOffset, 0f);
  }
}
