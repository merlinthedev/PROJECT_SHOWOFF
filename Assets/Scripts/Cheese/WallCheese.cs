using UnityEngine;

public class WallCheese : MonoBehaviour {

    [SerializeField] private Collider2D cheeseCollider;


    private void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("Player")) {
            var pc = other.GetComponent<PlayerMovementController>();
            pc.isCheesing = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other) {
        if (other.CompareTag("Player")) {
            var pc = other.GetComponent<PlayerMovementController>();
            pc.isCheesing = false;
        }

    }

}