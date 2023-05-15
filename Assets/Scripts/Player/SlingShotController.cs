using System.Collections;
using System.Collections.Generic;
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

    [SerializeField] private Projectile projectilePrefab;

    // Start is called before the first frame update
    void Start() {

    }

    // Update is called once per frame
    void Update() {
        //update smoothed stick
        Vector2 smoothDelta = stickSmoothed - stickInput;
        stickSmoothed -= smoothDelta * Mathf.Clamp01(stickSmoothSpeed * Time.deltaTime);

        float smoothStickMagnitude = stickSmoothed.magnitude;

        if (stickInput == Vector2.zero && smoothStickMagnitude >= minShootMagnitude) {
            shootDirection = -stickSmoothed.normalized;
            shootForce = Mathf.Lerp(minShootForce, maxShootForce, smoothStickMagnitude);
            Debug.Log("Shoot Direction: " + shootDirection);
            Debug.Log("Shoot Force: " + shootForce);
            stickSmoothed = Vector2.zero;
            Shoot(shootDirection, shootForce);
        }

    }

    void Shoot(Vector2 shootDirection, float shootForce) {
        Projectile projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        projectile.Shoot(shootDirection, shootForce);
    }

    public void OnStickInput(InputAction.CallbackContext context) {
        stickInput = context.ReadValue<Vector2>();
    }

}
