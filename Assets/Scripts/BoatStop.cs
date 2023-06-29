using EventBus;
using UnityEngine;

public class BoatStop : MonoBehaviour {
    private void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.CompareTag("Player")) {
            EventBus<BoatDestinationReachedEvent>.Raise(new BoatDestinationReachedEvent());
        }
    }
}