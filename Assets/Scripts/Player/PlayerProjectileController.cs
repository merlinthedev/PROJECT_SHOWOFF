using JetBrains.Annotations;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class PlayerProjectileController : MonoBehaviour {
    [SerializeField] private Player player;

    private bool hasProjectile = false;
    private GameObject projectile = null;
    [SerializeField] private Transform holdTransform;
    [SerializeField] private bool grabbing = false;

    private void Update() {
        UpdateProjectile();
    }

    private void UpdateProjectile() {
        if (projectile == null) return;
        projectile.transform.position = holdTransform.position;
    }

    public bool HasProjectile() {
        return hasProjectile;
    }

    public void SetProjectileFlag(bool value) {
        hasProjectile = value;
    }

    public void ResetProjectile() {
        hasProjectile = false;
        projectile = null;
    }

    public GameObject GetProjectile() {
        return projectile;
    }

    public Transform GetHoldingTransform() {
        return holdTransform;
    }

    IPickup p;
    GameObject g;

    private void OnTriggerStay2D(Collider2D other) {
        if (other.gameObject.CompareTag("Pickup")) {
            if (!grabbing) {
                return;
            }

            player.GetPlayerAnimatorController().Pickup();
            p = other.gameObject.GetComponent<IPickup>();
            g = other.gameObject;

            freezePlayerAfterRockPickup();
        }
    }

    public void AnimateRock() {
        hasProjectile = true;
        p.OnPickup(player);
        projectile = g;
    }

    public void OnGrab(InputAction.CallbackContext callbackContext) {
        if (callbackContext.started) {
            grabbing = true;
        }

        if (callbackContext.canceled) {
            grabbing = false;
        }
    }

    private void freezePlayerAfterRockPickup() {
        player.GetPlayerController().canMove = false;
        player.GetPlayerController().setVelocity(Vector2.zero);
        player.GetPlayerController().m_Rigidbody2D.isKinematic = true;
    }
}