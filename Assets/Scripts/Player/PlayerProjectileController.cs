using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerProjectileController : MonoBehaviour {
    [SerializeField] private Player player;

    private bool hasProjectile = false;
    private GameObject projectile = null;
    private AProjectile projectileScript = null;
    [SerializeField] private Transform holdTransform;
    [SerializeField] public bool grabbing = false;

    [SerializeField] private GameObject startingPickup = null;

    private void Start() {
        if (startingPickup != null) {
            IPickup pickup = startingPickup.GetComponent<IPickup>();
            if (pickup == null) { return; }

            hasProjectile = true;
            projectile = startingPickup;
            projectileScript = startingPickup.GetComponent<AProjectile>();
            pickup.OnPickup(player);
        }
    }

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

    public AProjectile GetProjectileScript() {
        return projectileScript;
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

            if (hasProjectile) return;

            player.GetPlayerAnimatorController().Pickup();
            p = other.gameObject.GetComponent<IPickup>();
            g = other.gameObject;

            Debug.Log("Picked up a pickup!");

            freezePlayerAfterRockPickup();
        }

    }

    public void AnimateRock() {
        hasProjectile = true;
        p.OnPickup(player);
        projectile = g;
        projectileScript = g.GetComponent<AProjectile>();
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