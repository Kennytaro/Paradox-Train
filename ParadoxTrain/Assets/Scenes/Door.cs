using UnityEngine;

public class Door : PlayerObjectCollision {
  [Header("ONLY USE THIS. IGNORE onTriggerEvent")]
  public string sceneName;

  public override void Interact() {
    // Door prefab will just have the LevelLoader stuff preset
    // Set the scene index in the LevelLoader
    FindFirstObjectByType<LevelLoader>().sceneName = sceneName;
    FindFirstObjectByType<LevelLoader>().LoadNextLevel();
  }
}
