using EventBus;
using UnityEngine;

public class BoatStop : MonoBehaviour {
    [SerializeField] private bool isLast;
    [SerializeField] private int index;

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.CompareTag("Player")) {
            if (index == 0) {
                EventBus<VodyanoyAwakeEvent>.Raise(new VodyanoyAwakeEvent());
            }

            EventBus<BoatDestinationReachedEvent>.Raise(new BoatDestinationReachedEvent {
                index = this.index,
                last = isLast,
            });

            Debug.Log("Reached boat stop: " + index);
        }
    }
}