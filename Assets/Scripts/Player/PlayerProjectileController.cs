using UnityEngine;

public class PlayerProjectileController : MonoBehaviour {
    [SerializeField] private Player player;

    private bool hasProjectile = false;
    private GameObject projectile = null;
    [SerializeField] private Transform holdTransform;

    public void UpdateProjectile() {
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

    private void OnTriggerStay2D(Collider2D other) {
        if (other.gameObject.CompareTag("Pickup")) {
            var x = GetComponent<PlayerEventHandler>();
            if (x == null) {
                Debug.Log("Player has no PlayerEventHandler");
                return;
            }

            if (!x.Grabbing) {
                Debug.Log("Player is not grabbing");
                return;
            }

            var pickup = other.gameObject.GetComponent<IPickup>();
            pickup.OnPickup(player);
            hasProjectile = true;
            projectile = other.gameObject;
        }
    }
}