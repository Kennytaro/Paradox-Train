using Unity.Cinemachine;
using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(Camera))]
public class CameraScaler : MonoBehaviour {
  public Vector2 referenceResolution = new Vector2(1920, 1080);

  private CinemachineCamera cam;
  private int lastScreenWidth;
  private int lastScreenHeight;

  void Awake() {
    cam = FindFirstObjectByType<CinemachineCamera>();
    UpdateCamera();
  }

  void Update() {
    // Detect resize
    if (Screen.width != lastScreenWidth || Screen.height != lastScreenHeight) {
      UpdateCamera();
    }
  }

  void UpdateCamera() {
    lastScreenWidth = Screen.width;
    lastScreenHeight = Screen.height;

    float targetAspect = referenceResolution.x / referenceResolution.y;
    float windowAspect = (float)Screen.width / Screen.height;

    if (windowAspect >= targetAspect) {
      // Wider than reference -> scale by HEIGHT
      cam.Lens.OrthographicSize = referenceResolution.y / 200f;
    }
    else {
      // Taller than reference -> scale by WIDTH
      float difference = targetAspect / windowAspect;
      cam.Lens.OrthographicSize = (referenceResolution.y / 200f) * difference;
    }
  }
}
