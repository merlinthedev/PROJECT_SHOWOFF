using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tutorial : MonoBehaviour
{
    [SerializeField] private int tutorialNumber;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite[] sprites;
    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.gameObject.CompareTag("Tutorial")) {
            TriggerTutorial(collision.GetComponent<TutorialTrigger>().tutorialNumber);
        }
    }

    private void OnTriggerExit2D(Collider2D collision) {
        if (collision.gameObject.CompareTag("Tutorial")) {
            spriteRenderer.enabled = false;
        }
    }

    private void TriggerTutorial(int TutorialNumber) {
        tutorialNumber = TutorialNumber;
        spriteRenderer.enabled = true;
        spriteRenderer.sprite = sprites[tutorialNumber];
    }


}
