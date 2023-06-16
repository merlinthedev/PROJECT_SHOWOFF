using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerProjectileController : MonoBehaviour {
    [SerializeField] private Player player;

    private bool hasProjectile = false;
    private GameObject projectile = null;
    [SerializeField] private Transform holdTransform;

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
            var x = GetComponent<PlayerEventHandler>();
            if (x == null) {
                Debug.Log("Player has no PlayerEventHandler");
                return;
            }

            if (!x.Grabbing) {
                return;
            }

            player.GetPlayerAnimatorController().Pickup();
            p = other.gameObject.GetComponent<IPickup>();
            g = other.gameObject;

        }

    }

    public void AnimateRock() {
        hasProjectile = true;
        p.OnPickup(player);
        projectile = g;
    }
}