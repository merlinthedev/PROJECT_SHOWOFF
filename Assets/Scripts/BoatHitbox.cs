using UnityEngine;

public class BoatHitbox : MonoBehaviour {
    public bool playerInBoat = false;
    private Rigidbody2D playerRigidbody;

    private void FixedUpdate() {
        if (playerInBoat) {
            playerRigidbody.AddForce(Vector2.down * 10, ForceMode2D.Force);
        }
    }

    private void OnTriggerStay2D(Collider2D collision) {
        // Debug.Log(collision.gameObject.name);
        // if (collision.gameObject.tag == "Player") {
        //     playerInBoat = true;
        // }

        if (collision.gameObject.CompareTag("Player")) {
            playerRigidbody = collision.gameObject.GetComponent<Rigidbody2D>();
            playerInBoat = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision) {
        // if (collision.gameObject.tag == "Player") {
        //     playerInBoat = false;
        // }

        if (collision.gameObject.CompareTag("Player")) {
            playerRigidbody = null;
            playerInBoat = false;
        }
    }
}