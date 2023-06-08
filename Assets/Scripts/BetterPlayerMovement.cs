using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class BetterPlayerMovement : MonoBehaviour {
    [Header("HORIZONTAL MOVEMENT")] [SerializeField]
    private Rigidbody2D m_Rigidbody2D;

    [SerializeField] private CapsuleCollider2D m_CapsuleCollider2D;
    [SerializeField] private PhysicsMaterial2D m_PlayerPhysicsMaterial2D;
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
        slopeAngle = 180;
    }

    private void horizontalMovement() {
        if (isGrounded && slopeAngle > maximumGroundAngle) {
            isGrounded = false;
            m_PlayerPhysicsMaterial2D.friction = 0f;
        } else {
            m_PlayerPhysicsMaterial2D.friction = 1f;
        }

        float rawXMovement = movementInput.x;

        if (rawXMovement != 0 || isGrounded) {
            float desiredHorizontalSpeed = rawXMovement * maximumHorizontalSpeed;
            float velocityGap = desiredHorizontalSpeed - m_Rigidbody2D.velocity.x;

            float acceleration = isGrounded ? groundAcceleration : airAcceleration;
            float accelerationThisFrame = acceleration * Time.fixedDeltaTime;
            float accelerationSign = Mathf.Sign(velocityGap);
            float accelerationMagnitude = Mathf.Min(Mathf.Abs(velocityGap), accelerationThisFrame);

            Vector2 accelerationVector = new Vector2(accelerationMagnitude * accelerationSign, 0f);

            // rotate acceleration vector to match slope using the slope normal
            if (isGrounded && slopeAngle < maximumGroundAngle) {
                accelerationVector = Quaternion.FromToRotation(Vector2.up, slopeNormal) * accelerationVector;
            }

            m_Rigidbody2D.AddForce(accelerationVector, ForceMode2D.Impulse);
        }
    }

    private bool isGrounded;
    public float slopeAngle;
    private Vector2 slopeNormal;
    private float lastGroundedTime;

    private void OnCollisionStay2D(Collision2D other) {
        if (Utils.IsInLayerMask(other.gameObject.layer, groundLayer)) {
            isGrounded = true;

            //find most vertical facing contact point
            ContactPoint2D mostVerticalContactPoint = other.contacts[0];
            for (int i = 1; i < other.contactCount; i++) {
                ContactPoint2D contactPoint = other.contacts[i];
                if (Vector2.Dot(contactPoint.normal, Vector2.up) >
                    Vector2.Dot(mostVerticalContactPoint.normal, Vector2.up)) {
                    mostVerticalContactPoint = contactPoint;
                }
            }

            Vector2 groundNormal = mostVerticalContactPoint.normal;

            float newSlopeAngle = Vector2.Angle(Vector2.up, groundNormal);
            if (newSlopeAngle < slopeAngle) {
                slopeAngle = newSlopeAngle;
                slopeNormal = groundNormal;
            }

            lastGroundedTime = Time.time;
        }
    }

    private Vector2 movementInput;

    public void OnMove(InputAction.CallbackContext context) {
        movementInput = context.ReadValue<Vector2>();
    }
}