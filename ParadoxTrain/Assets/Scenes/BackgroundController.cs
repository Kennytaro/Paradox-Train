using UnityEngine;

public class BackgroundController : MonoBehaviour {
  private int childrenCount = 0;
  private float startPos, length;

  public GameObject cam;
  
  void Start() {
    childrenCount = transform.childCount;
    startPos = transform.position.x;
    length = gameObject.transform.GetChild(0).GetChild(0).GetComponent<SpriteRenderer>().bounds.size.x;
  }

  void FixedUpdate() {
    for (int i = 0; i < childrenCount; i++) {
      float parallaxEffect = (float)i / childrenCount / childrenCount;
      float distance = cam.transform.position.x * parallaxEffect;
      float movement = cam.transform.position.x * (1 - parallaxEffect);

      gameObject.transform.GetChild(i).position = new Vector3(startPos + distance, transform.position.y, transform.position.z);
      
      if (movement > startPos + length) {
        startPos += length;
      } else if (movement < startPos - length) {
        startPos -= length;
      }
    }
  }
}
