using UnityEngine;

public class Water : MonoBehaviour {
    [SerializeField] private Collider2D triggerCollider;

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.CompareTag("Player")) {
            var player = other.gameObject.GetComponent<BetterPlayerMovement>();

            if (player == null) {
                //Debug.LogError("Player has no BetterPlayerMovement component", this);
                return;
            }

            player.setInWater(true);

            Debug.Log("InWater is set to true");
        }

        if (other.gameObject.CompareTag("Boat")) {
            other.GetComponent<Boat>().BoatInWater();
        }

    }

    private void OnTriggerStay2D(Collider2D other) {
        if (other.gameObject.CompareTag("Player")) {
            var player = other.gameObject.GetComponent<BetterPlayerMovement>();

            if (player == null) {
                //Debug.LogError("Player has no BetterPlayerMovement component", this);
                return;
            }

            if (!player.isInWater()) {
                player.setInWater(true);
            }
        }

        if (other.gameObject.CompareTag("Log")) {
            var log = other.gameObject.GetComponent<ObjectGrabbable>();

            if (log == null) {
                //Debug.LogError("Log has no ObjectGrabbable component", this);
                return;
            }

            if (!log.isWater) {
                //Debug.Log("Log is in water");
                log.isWater = true;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other) {
        if (other.gameObject.CompareTag("Player")) {
            var player = other.gameObject.GetComponent<BetterPlayerMovement>();

            if (player == null) {
                //Debug.LogError("Player has no BetterPlayerMovement component", this);
                return;
            }

            player.setInWater(false);
        }

        if (other.gameObject.CompareTag("Log")) {
            var log = other.gameObject.GetComponent<ObjectGrabbable>();

            if (log == null) {
                //Debug.LogError("Log has no ObjectGrabbable component", this);
                return;
            }

            if (log.isWater) {
                Debug.Log("Log is not in water");
                log.isWater = false;
            }
        }
    }
}