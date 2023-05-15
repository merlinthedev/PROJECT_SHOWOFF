using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SlingShotController : MonoBehaviour
{
    public Vector2 stickInput;
    public Vector2 stickSmoothed;
    public float stickSmoothSpeed = 1f;
    public Vector2 shootDirection;
    public float stickSpeed;
    public float minShootMagnitude = 0.1f;

    [SerializeField] private float maxShootForce = 10f;
    [SerializeField] private float minShootForce = 1f;
    private float shootForce = 0f;

    [SerializeField] private Projectile projectilePrefab;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 smoothDelta = stickSmoothed - stickInput;

        stickSmoothed -= smoothDelta * Mathf.Clamp01(stickSmoothSpeed * Time.deltaTime);

        float smoothStickMagnitude = stickSmoothed.magnitude;

        if(stickInput == Vector2.zero && smoothStickMagnitude >= minShootMagnitude) {
            shootDirection = -stickSmoothed.normalized;
            shootForce = Mathf.Lerp(minShootForce, maxShootForce, smoothStickMagnitude);
            Debug.Log("Shoot Direction: " + shootDirection);
            Debug.Log("Shoot Force: " + shootForce);
            stickSmoothed = Vector2.zero;
        }

    }

    public void OnStickInput(InputAction.CallbackContext context) {
        stickInput = context.ReadValue<Vector2>();
    }
    
}
