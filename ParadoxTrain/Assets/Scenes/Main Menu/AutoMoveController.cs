using UnityEngine;

public class MainMenuAutoParallax : MonoBehaviour {
  private Transform[] layers;
  private float[] layerSpeeds;
  private float spriteWidth;
  private Vector3 cameraLeftEdge, cameraRightEdge;

  private const float BASE_SPEED = 10f;
  public bool moveLeft = true;

  void Start() {
    // Get all child layers automatically
    int layerCount = transform.childCount;
    layers = new Transform[layerCount];
    layerSpeeds = new float[layerCount];

    // Store all layers and calculate their speeds
    for (int i = 0; i < layerCount; i++) {
      layers[i] = transform.GetChild(i);
      // Calculate speed like in your original script
      layerSpeeds[i] = CalculateLayerSpeed(i, layerCount);
    }

    // Get sprite width from first sprite of first layer
    if (layers.Length > 0 && layers[0].childCount > 0) {
      spriteWidth = layers[0].GetChild(0).GetComponent<SpriteRenderer>().bounds.size.x;
    }

    // Setup camera boundaries
    Camera cam = Camera.main;
    float cameraHeight = 2f * cam.orthographicSize;
    float cameraWidth = cameraHeight * cam.aspect;

    cameraLeftEdge = cam.transform.position - Vector3.right * (cameraWidth * 0.5f);
    cameraRightEdge = cam.transform.position + Vector3.right * (cameraWidth * 0.5f);

    // Position sprites automatically for each layer
    foreach (Transform layer in layers) {
      PositionSprites(layer);
    }
  }

  void Update() {
    float direction = moveLeft ? -1f : 1f;

    // Move each layer at its own speed
    for (int i = 0; i < layers.Length; i++) {
      float layerSpeed = BASE_SPEED * layerSpeeds[i];
      layers[i].Translate(Vector3.right * (direction * layerSpeed * Time.deltaTime));

      // Check and reposition sprites
      for (int j = 0; j < layers[i].childCount; j++) {
        CheckAndRepositionSprite(layers[i].GetChild(j), i);
      }
    }
  }

  void PositionSprites(Transform layer) {
    // Position 4 sprites in a row with equal spacing
    float totalWidth = spriteWidth * layer.childCount;
    float startX = -totalWidth * 0.5f + spriteWidth * 0.5f;

    for (int i = 0; i < layer.childCount; i++) {
      Vector3 localPos = Vector3.zero;
      localPos.x = startX + (i * spriteWidth);
      layer.GetChild(i).localPosition = localPos;
    }
  }

  void CheckAndRepositionSprite(Transform sprite, int layerIndex) {
    Vector3 worldPos = layers[layerIndex].TransformPoint(sprite.localPosition);

    if (moveLeft) {
      // If sprite is completely off left side
      if (worldPos.x + spriteWidth * 0.5f < cameraLeftEdge.x) {
        float rightmost = GetRightmostSpritePosition(layers[layerIndex]);
        Vector3 localPos = sprite.localPosition;
        localPos.x = rightmost + spriteWidth;
        sprite.localPosition = localPos;
      }
    }
    else {
      // If sprite is completely off right side
      if (worldPos.x - spriteWidth * 0.5f > cameraRightEdge.x) {
        float leftmost = GetLeftmostSpritePosition(layers[layerIndex]);
        Vector3 localPos = sprite.localPosition;
        localPos.x = leftmost - spriteWidth;
        sprite.localPosition = localPos;
      }
    }
  }

  float GetRightmostSpritePosition(Transform layer) {
    float rightmost = float.MinValue;
    for (int i = 0; i < layer.childCount; i++) {
      float x = layer.GetChild(i).localPosition.x;
      if (x > rightmost) rightmost = x;
    }
    return rightmost;
  }

  float GetLeftmostSpritePosition(Transform layer) {
    float leftmost = float.MaxValue;
    for (int i = 0; i < layer.childCount; i++) {
      float x = layer.GetChild(i).localPosition.x;
      if (x < leftmost) leftmost = x;
    }
    return leftmost;
  }

  float CalculateLayerSpeed(int layerIndex, int totalLayers) {
    // Same calculation as your original script
    float parallaxEffect = (float)layerIndex / totalLayers / totalLayers;
    return 1f - parallaxEffect;
  }
}