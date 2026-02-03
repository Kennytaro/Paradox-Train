using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour {
  private static WaitForSeconds _waitForSeconds1 = new WaitForSeconds(1);
  public Animator transitionAnimator;
  public string sceneName;

  public void LoadNextLevel() {
    StartCoroutine(LoadLevel());
  }

  IEnumerator LoadLevel() {
    transitionAnimator.SetTrigger("Start");

    yield return _waitForSeconds1;

    SceneManager.LoadScene(sceneName);
  }
}