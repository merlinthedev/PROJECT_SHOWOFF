using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoatHitbox : MonoBehaviour
{
    public bool playerInBoat = false;
    private void OnTriggerStay2D(Collider2D collision) {
        Debug.Log(collision.gameObject.name);
        if (collision.gameObject.tag == "Player") {
            playerInBoat = true;
        } 
    }
    private void OnTriggerExit2D(Collider2D collision) {
        if (collision.gameObject.tag == "Player") {
            playerInBoat = false;
        }
    }
}
