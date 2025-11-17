using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using TMPro;

public class DialogueManager : MonoBehaviour {
  private Queue<string> sentences;

  public TMP_Text nameText;
  public TMP_Text dialogueText;

  void Start() {
    ToggleVisibility(false);
    sentences = new Queue<string>();
  }

  public void StartDialogue(Dialogue dialogue) {
    // First enable visibility of the dialogue box
    ToggleVisibility(true);

    // Then stop player from being able to move mid dialogue
    FindFirstObjectByType<PlayerController>().canMove = false;

    // Then process dialogue stuff
    nameText.text = dialogue.name;

    sentences.Clear();

    foreach (string sentence in dialogue.sentences) {
      sentences.Enqueue(sentence);
    }

    DisplayNextSentence();
  }

  public void DisplayNextSentence() {
    if (sentences.Count == 0) {
      EndDialogue();
      return;
    }

    if (Input.GetButtonDown("Jump") && dialogueText.text != sentences.Peek()) {
      StopAllCoroutines();
      dialogueText.text = sentences.Peek();
    } else {
      string sentence = sentences.Dequeue();
      StartCoroutine(TypeSentence(sentence));
    }

  }

  IEnumerator TypeSentence(string sentence) {
    dialogueText.text = "";

    foreach (char letter in sentence.ToCharArray()) {
      dialogueText.text += letter;
      yield return null;
    }
  }

  public void EndDialogue() {
    // Disable visibility
    ToggleVisibility(false);

    // And allow player to move again
    FindFirstObjectByType<PlayerController>().canMove = true;
  }

  private void ToggleVisibility(bool visibility) {
    transform.GetChild(0).gameObject.SetActive(visibility);
  }
}
