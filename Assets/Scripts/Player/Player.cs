using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {
    // Will keep track of inventory later
    private bool hasProjectile = false;
    private GameObject projectile = null;
    [SerializeField] private Transform holdTransform;

    public bool HasProjectile() {
        return this.hasProjectile;
    }

    public void SetProjectileFlag(bool value) {
        this.hasProjectile = value;
    }
    
    public void ResetProjectile() {
        this.hasProjectile = false;
        this.projectile = null;
    }

    public GameObject GetProjectile() {
        return this.projectile;
    }

    public Transform GetHoldingTransform() {
        return this.holdTransform;
    }

    private void OnTriggerStay2D(Collider2D other) {
        if (other.gameObject.CompareTag("Pickup")) {
            var x = this.GetComponent<PlayerEventHandler>();
            if (x == null) {
                Debug.Log("Player has no PlayerEventHandler");
                return;
            }

            if (!x.Grabbing) {
                Debug.Log("Player is not grabbing");
                return;
            }

            var pickup = other.gameObject.GetComponent<IPickup>();
            pickup.OnPickup(this);
            this.hasProjectile = true;
            this.projectile = other.gameObject;
        }
    }
}