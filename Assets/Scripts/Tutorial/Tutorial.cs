using EventBus;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tutorial : MonoBehaviour
{
    [SerializeField] private int tutorialNumber;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite[] sprites;
    [SerializeField] private GameObject player;
    private bool fadeIn = false;
    private bool inTutorial = false;

    private void Update() {

    }
    private void FixedUpdate() {
        if (fadeIn) {
            if (spriteRenderer.color.a <= 1) spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, spriteRenderer.color.a + 0.02f);
        } else if (!fadeIn) {
            if (spriteRenderer.color.a >= 0) {
                spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, spriteRenderer.color.a - 0.02f);
                if (spriteRenderer.color.a < 0.02) spriteRenderer.enabled = false;
            }
        }
        if (!inTutorial) {
            DisableTutorial();
        }
    }
    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.gameObject.CompareTag("Tutorial")) {
            CallTutorial(collision);
            if(inTutorial) DisableTutorial();
        }
    }

    private void OnTriggerStay2D(Collider2D collision) {
        if (collision.gameObject.CompareTag("Tutorial")) {
            if (!inTutorial) CallTutorial(collision);
        }
    }

    private void OnTriggerExit2D(Collider2D collision) {
        if (collision.gameObject.CompareTag("Tutorial")) {
            DisableTutorial();
        }
    }

    private void TriggerTutorial(int TutorialNumber) {
        tutorialNumber = TutorialNumber;
        spriteRenderer.enabled = true;
        spriteRenderer.sprite = sprites[tutorialNumber];
        fadeIn = true;
    }
    public void CallTutorial(Collider2D collision) {
        inTutorial = true;
        if (!collision.GetComponent<TutorialTrigger>().mustHoldStone) {
            if (!player.GetComponent<PlayerProjectileController>().HasProjectile()) {
                TriggerTutorial(collision.GetComponent<TutorialTrigger>().tutorialNumber);
            }
        } 
        else {
            if (player.GetComponent<PlayerProjectileController>().HasProjectile()) {
                TriggerTutorial(collision.GetComponent<TutorialTrigger>().tutorialNumber);
            }
        }
    }

    public void DisableTutorial() {

        inTutorial = false;
        fadeIn = false;
    }


}
