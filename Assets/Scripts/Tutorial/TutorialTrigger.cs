using EventBus;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialTrigger : MonoBehaviour {
    public int tutorialNumber;
    public bool repeatable;
    public bool mustHoldStone;

    private void OnEnable() {
        EventBus<TobaccoThrowEvent>.Subscribe(onThrow);
    }

    private void OnDisable() {
        EventBus<TobaccoThrowEvent>.Unsubscribe(onThrow);

    }

    private void OnTriggerExit2D(Collider2D collision) {
        if (collision.gameObject.CompareTag("Player")) {
            if (!repeatable) {
                gameObject.SetActive(false);
            }
        }
    }

    private void onThrow(TobaccoThrowEvent e) {
        this.DeleteTutorial();
    }

    public void DeleteTutorial() {
        Destroy(gameObject);
    }
}
