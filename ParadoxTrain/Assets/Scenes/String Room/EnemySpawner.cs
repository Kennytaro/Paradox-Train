using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour {
  public GameObject[] enemies;
  private bool waiting = false;

  void Update() {
    if (!waiting) {
      StartCoroutine(Spawn());
    }
  }

  IEnumerator Spawn() {
    waiting = true;
    foreach (var enemy in enemies) {
      Instantiate(enemy, transform.position, Quaternion.identity);
    }

    yield return new WaitForSeconds(10);
    waiting = false;
  }
}
