using System;
using UnityEngine;

public class Water : MonoBehaviour {
    [SerializeField] private Collider2D triggerCollider;

    private void OnTriggerEnter2D(Collider2D other) {
        if (!other.gameObject.CompareTag("Player")) {
            return;
        }

        var player = other.gameObject.GetComponent<PlayerMovementController>();

        if (player == null) {
            Debug.LogError("Player has no PlayerController component", this);
            return;
        }
        
        Debug.Log("InWater is set to true");
        player.SetInWater(true);
    }

    private void OnTriggerStay2D(Collider2D other) {
        if (!other.gameObject.CompareTag("Player")) {
            return;
        }

        var player = other.gameObject.GetComponent<PlayerMovementController>();

        if (player == null) {
            Debug.LogError("Player has no PlayerController component", this);
            return;
        }

        if (!player.IsInWater()) {
            Debug.Log("InWater set to true from stay");
            player.SetInWater(true);
            
        }
    }

    private void OnTriggerExit2D(Collider2D other) {
        if (!other.gameObject.CompareTag("Player")) {
            return;
        }

        var player = other.gameObject.GetComponent<PlayerMovementController>();

        if (player == null) {
            Debug.LogError("Player has no PlayerController component", this);
            return;
        }


        Utils.Instance.InvokeDelayed(0.25f, () => {
            Debug.Log("InWater has been set to false after 0.5f seconds");
            player.SetInWater(false);
        });
    }
}