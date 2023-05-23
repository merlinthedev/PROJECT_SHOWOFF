using System;
using UnityEngine;

public class Water : MonoBehaviour {
    [SerializeField] private Collider2D triggerCollider;

    private void OnTriggerEnter2D(Collider2D other) {
        if (!other.gameObject.CompareTag("Player")) {
            return;
        }

        var player = other.gameObject.GetComponent<PlayerController>();

        if (player == null) {
            Debug.LogError("Player has no PlayerController component", this);
            return;
        }
        
        player.SetInWater(true, -1);
    }

    private void OnTriggerStay2D(Collider2D other) {
        if (!other.gameObject.CompareTag("Player")) {
            return;
        }

        var player = other.gameObject.GetComponent<PlayerController>();

        if (player == null) {
            Debug.LogError("Player has no PlayerController component", this);
            return;
        }
        
        if(!player.IsInWater()) player.SetInWater(true, -1);
    }

    private void OnTriggerExit2D(Collider2D other) {
        if (!other.gameObject.CompareTag("Player")) {
            return;
        }

        var player = other.gameObject.GetComponent<PlayerController>();

        if (player == null) {
            Debug.LogError("Player has no PlayerController component", this);
            return;
        }


        Utils.Instance.InvokeDelayed(0.5f, () => {
            player.SetInWater(false, Time.time);
        });
    }
}