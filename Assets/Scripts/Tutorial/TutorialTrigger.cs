using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialTrigger : MonoBehaviour
{
    public int tutorialNumber;
    public bool repeatable;

    private void OnTriggerExit2D(Collider2D collision) {
        if (collision.gameObject.CompareTag("Player")) {
            if(!repeatable) {
                gameObject.SetActive(false);
            }
        }
    }
}
