using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class BetterPlayerMovement : MonoBehaviour {
    [Header("HORIZONTAL MOVEMENT")] [SerializeField]
    private Rigidbody2D m_Rigidbody2D;

    [SerializeField] private CapsuleCollider2D m_CapsuleCollider2D;
    [SerializeField] private PhysicsMaterial2D m_PlayerPhysicsMaterial2D;
    [SerializeField] private float maximumHorizontalSpeed = 4f;
    [SerializeField] private float groundAcceleration = 20f;
    [SerializeField] private float airAcceleration = 10f;
    [SerializeField] private float maximumGroundAngle = 20f;
    [SerializeField] private bool affectGroundHorizontalOnly = true;
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

    [Header("JUMPING")] [SerializeField] private float maxJumpHeight;
    [SerializeField] private float maxJumpTime;
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
        jumping();

        isGrounded = false;
        slopeAngle = 180;
        jumpButtonPressedThisFrame = false;
    }

    private void horizontalMovement() {
        if (isGrounded) {
            //m_PlayerPhysicsMaterial2D.friction = 1f;
        } else {
            m_PlayerPhysicsMaterial2D.friction = 0f;
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

    public enum JumpState {
        CanJump,
        Jumping,
        Falling
    }

    public JumpState currentJumpState = JumpState.Falling;
    private float jumpStartHeight;
    private float jumpStartTime;


    private void jumping() {
        switch (currentJumpState) {
            case JumpState.CanJump:
                bool canJump = (hasJumpBuffer && jumpButtonPressed) ||
                               (hasCoyoteJump && jumpButtonPressedThisFrame);
                if (canJump) {
                    jumpStartHeight = transform.position.y;
                    jumpStartTime = Time.time;
                    currentJumpState = JumpState.Jumping;
                    m_Rigidbody2D.velocity = new Vector2(m_Rigidbody2D.velocity.x, jumpSpeed);
                }

                if (!isGrounded && !hasCoyoteJump) {
                    currentJumpState = JumpState.Falling;
                }

                break;

            case JumpState.Jumping:
                m_Rigidbody2D.velocity = new Vector2(m_Rigidbody2D.velocity.x, jumpSpeed);
                bool endJump = !jumpButtonPressed ||
                               // slopeAngle > 90 ||
                               Time.time > jumpStartTime + maxJumpTime ||
                               transform.position.y > jumpStartHeight + maxJumpHeight;
                if (endJump) {
                    currentJumpState = JumpState.Falling;
                }

                break;

            case JumpState.Falling:
                if (isGrounded) {
                    currentJumpState = JumpState.CanJump;
                }

                m_Rigidbody2D.AddForce(Vector2.down * (9.81f * 4), ForceMode2D.Force);

                //clamp fall speed
                if (m_Rigidbody2D.velocity.y < -maximumFallSpeed) {
                    m_Rigidbody2D.velocity = new Vector2(m_Rigidbody2D.velocity.x, -maximumFallSpeed);
                }

                break;
        }
    }

    private bool hasJumpBuffer => isGrounded && (jumpButtonPressedTime + jumpBufferTime > Time.time);
    private bool hasCoyoteJump => !isGrounded && (lastGroundedTime + coyoteTime > Time.time);


    [Header("DEBUG")] public bool isGrounded = false;
    public float slopeAngle = 180;
    private Vector2 slopeNormal = Vector2.up;
    private float lastGroundedTime = 0f;

    private void OnCollisionStay2D(Collision2D other) {
        if (Utils.IsInLayerMask(other.gameObject.layer, groundLayer)) {
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
            isGrounded = slopeAngle < maximumGroundAngle;
        }
    }

    private Vector2 movementInput = Vector2.zero;

    public void OnMove(InputAction.CallbackContext context) {
        movementInput = context.ReadValue<Vector2>();
    }

    public bool jumpButtonPressed = false;
    private bool jumpButtonPressedThisFrame = false;
    private float jumpButtonPressedTime = 0f;

    public void OnJump(InputAction.CallbackContext context) {
        if (context.started) {
            jumpButtonPressedTime = Time.time;
            jumpButtonPressedThisFrame = true;
        }

        if (context.performed) {
            jumpButtonPressed = true;
        }

        if (context.canceled) {
            jumpButtonPressed = false;
        }
    }
}