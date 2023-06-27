using UnityEngine;

public class Tutorial : MonoBehaviour
{
    [SerializeField] private int tutorialNumber;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite[] sprites;
    [SerializeField] private GameObject player;
    private bool inTutorial = false;

    private void FixedUpdate() {
        if(!inTutorial) {
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
        spriteRenderer.enabled = false;
        inTutorial = false;
    }


}
