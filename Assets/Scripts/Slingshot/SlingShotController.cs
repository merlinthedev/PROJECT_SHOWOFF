using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.InputSystem;

public class SlingShotController : MonoBehaviour {
    private Vector2 stickInput;
    private Vector2 stickSmoothed;
    [SerializeField] private float stickSmoothSpeed = 1f;
    private Vector2 shootDirection;
    private float stickSpeed;
    [SerializeField] private float minShootMagnitude = 0.1f;

    [SerializeField] private float maxShootForce = 10f;
    [SerializeField] private float minShootForce = 1f;
    private float shootForce = 0f;

    [SerializeField] private SlingShotTrajectoryPreview slingShotTrajectoryPreview;
    [SerializeField] private Player player;


    // Start is called before the first frame update


    // Update is called once per frame
    void Update() {
        if (stickInput == Vector2.zero || player.GetPlayerProjectileController().GetProjectileScript().canThrow == false) {
            slingShotTrajectoryPreview.ClearPredictionLine();
        }

        aim();
    }


    private void aim() {
        if (player.GetPlayerProjectileController().GetProjectileScript() == null) return;
        if (player.GetPlayerController().IsOnRope() || !player.GetPlayerProjectileController().GetProjectileScript().canThrow) return;
        //update smoothed stick
        Vector2 smoothDelta = stickSmoothed - stickInput;
        stickSmoothed -= smoothDelta * Mathf.Clamp01(stickSmoothSpeed * Time.deltaTime);

        float smoothStickMagnitude = stickSmoothed.magnitude;

        shootDirection = stickSmoothed.normalized;
        shootForce = Mathf.Lerp(minShootForce, maxShootForce, smoothStickMagnitude);


        if (shootDirection == Vector2.zero) {
            return;
        }


        if (player.GetPlayerProjectileController().HasProjectile()) {
            slingShotTrajectoryPreview.DrawPredictionLine(shootDirection * shootForce, transform.position);
        }


        if (stickInput == Vector2.zero && smoothStickMagnitude >= minShootMagnitude) {
            // Debug.LogError("Shoot Direction: " + shootDirection);
            // Debug.LogError("Shoot Force: " + shootForce);

            stickSmoothed = Vector2.zero;
            shoot(shootDirection, shootForce);
        }
    }

    private void shoot(Vector2 shootDirection, float shootForce) {
        // Get player script from parent object
        var player = transform.parent.GetComponent<Player>();

        if (player == null) {
            Debug.Log("player does not exist");
            return;
        }

        if (!player.GetPlayerProjectileController().HasProjectile()) {
            Debug.Log("player does not have projectile");
            return;
        }

        var projectile = transform.parent.GetComponentInChildren<AProjectile>();
        var x = projectile.GetComponent<IPickup>();
        if (x == null) {
            Debug.Log("x is null");
            return;
        }

        slingShotTrajectoryPreview.ClearPredictionLine();
        player.GetPlayerAnimatorController().Throw();

        Utils.Instance.InvokeDelayed(0.15f, () => {
            x.OnThrow(player);
            projectile.Shoot(shootDirection, shootForce);
            player.GetPlayerProjectileController().SetProjectileFlag(false);
        });
    }

    public void OnStickInput(InputAction.CallbackContext context) {
        stickInput = context.ReadValue<Vector2>();
    }
}