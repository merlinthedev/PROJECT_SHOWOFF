using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class BetterPlayerMovement : MonoBehaviour {
    [Header("HORIZONTAL MOVEMENT")] [SerializeField]
    private Rigidbody2D m_Rigidbody2D;

    [SerializeField] private CapsuleCollider2D m_CapsuleCollider2D;
    [SerializeField] private float maximumHorizontalSpeed = 10f;
    [SerializeField] private float groundAcceleration;
    [SerializeField] private float airAcceleration;
    [SerializeField] private float maximumGroundAngle;
    [SerializeField] private bool affectGroundHorizontalOnly;
    [SerializeField] private LayerMask groundLayer;

    [Header("LEDGE GRABBING")] [SerializeField]
    private float ledgeGrabDistance;

    [SerializeField] private float ledgeGrabHeight;
    [SerializeField] private float ledgeGrabDelay;
    [SerializeField] private float ledgeFreezeTime;
    [SerializeField] private float ledgeWidth;

    [Header("PUSHING")] [SerializeField] private float pushForce;
    [SerializeField] private float maxObjectMass;
    [SerializeField] private float objectDistance;

    [Header("JUMPING")] [SerializeField] private float maxHeight;
    [SerializeField] private float coyoteTime;
    [SerializeField] private float jumpSpeed;
    [SerializeField] private float jumpBufferTime;
    [SerializeField] private float maximumFallSpeed;

    [Header("ROPE CLIMBING")] [SerializeField]
    private float ropeClimbingSpeed;

    [SerializeField] private float ropeGrabTimeout;
    [SerializeField] private float jumpHorizontalImpulse;


    private void FixedUpdate() {
        horizontalMovement();

        isGrounded = false;
    }

    private void horizontalMovement() {
        float rawXMovement = movementInput.x;

        float desiredHorizontalSpeed = rawXMovement * maximumHorizontalSpeed;
        float velocityGap = desiredHorizontalSpeed - m_Rigidbody2D.velocity.x;

        float acceleration = isGrounded ? groundAcceleration : airAcceleration;
        float accelerationThisFrame = acceleration * Time.fixedDeltaTime;
        float accelerationSign = Mathf.Sign(velocityGap);
        float accelerationMagnitude = Mathf.Min(Mathf.Abs(velocityGap), accelerationThisFrame);

        Vector2 accelerationVector = new Vector2(accelerationMagnitude * accelerationSign, 0f);
        m_Rigidbody2D.AddForce(accelerationVector, ForceMode2D.Impulse);
    }

    private bool isGrounded;

    private void OnCollisionStay2D(Collision2D other) {
        if (Utils.IsInLayerMask(other.gameObject.layer, groundLayer)) {
            isGrounded = true;
        }

    }

    private Vector2 movementInput;

    public void OnMove(InputAction.CallbackContext context) {
        movementInput = context.ReadValue<Vector2>();
    }
}