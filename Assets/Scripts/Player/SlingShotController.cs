using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SlingShotController : MonoBehaviour
{
    public Vector2 stickInput;
    public Vector2 shootDirection;
    private float lastInputTime = 0;

    [SerializeField] private float maxShootForce = 10f;
    [SerializeField] private float minShootForce = 1f;

    [SerializeField] private Projectile projectilePrefab;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnStickInput(InputAction.CallbackContext context) {
        Vector2 newStick = context.ReadValue<Vector2>();
        float stickDelta = newStick.magnitude - stickInput.magnitude;
        float deltaTime = Time.time - lastInputTime;
        float stickSpeed = stickDelta / deltaTime;
        if (stickSpeed < -0.5f) {
            shootDirection = newStick;
            Debug.Log("Shoot! " + -stickInput);
        }


        lastInputTime = Time.time;
        stickInput = newStick;
    }
    
}
