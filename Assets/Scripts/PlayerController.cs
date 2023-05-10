using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float maxSpeed = 8f;
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float deceleration = 5f;
    [SerializeField] private AnimationCurve accelerationCurve;
    [SerializeField] private Vector2 forceScale = new Vector2(1f, 1f);
    [SerializeField] private float gravityScaleDrop = 10f;


    private Vector2 movementInput;
    private float movementControlDisabledTimer = 0f;
    [SerializeField] private Rigidbody2D rb;

    [Header("Jump")]
    private bool isGrounded = false;
    [SerializeField] private float jumpForce = 10f;

    private void FixedUpdate()
    {

        float targetSpeed = movementInput.x * maxSpeed;
        float speedDifference = targetSpeed - rb.velocity.x;
        float accelerationRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : deceleration;
        float movement = Mathf.Pow(Mathf.Abs(speedDifference) * accelerationRate, accelerationCurve.Evaluate(Mathf.Abs(speedDifference) * accelerationRate)) * Mathf.Sign(speedDifference);

        rb.AddForce(Vector2.right * movement * forceScale.x * rb.mass);


        // Groundcheck
        isGrounded = false;

        RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, Vector2.down, 1f, LayerMask.GetMask("Ground"));
        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider.gameObject != gameObject)
            {
                Debug.Log(hit.collider.gameObject.name, hit.collider.gameObject);
                isGrounded = true;
                break;
            }
        }



        // Gravity
        rb.AddForce(Vector2.down * gravityScaleDrop * rb.mass);

    }

    public void DoMove(InputAction.CallbackContext context)
    {
        movementInput = context.ReadValue<Vector2>();
    }

    public void DoJump(InputAction.CallbackContext context)
    {
        if (context.performed && isGrounded)
        {
            Debug.LogWarning("Jump");
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }
    }


}