using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : AProjectile {
    [SerializeField] private float lifetime = 5f;
    private float spawnTime = 0f;
    

    private void OnTriggerEnter2D(Collider2D collider) {
        //check if not player
        if (collider.gameObject.CompareTag("Player")) {
            return;
        }

        collider.attachedRigidbody?.AddForceAtPosition(GetComponent<Rigidbody2D>().velocity, transform.position,
            ForceMode2D.Impulse);
        Destroy(gameObject);
    }
}