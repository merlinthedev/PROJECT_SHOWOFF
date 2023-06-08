using UnityEngine;
using UnityEngine.InputSystem;

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
    private RopeController rope;

    [SerializeField] private FixedJoint2D ropeJoint;
    [SerializeField] private float ropeClimbingSpeed;
    [SerializeField] private float ropeGrabTimeout = 0.5f;
    [SerializeField] private float jumpHorizontalImpulse;


    private void FixedUpdate() {
        if (canMove) {
            horizontalMovement();
            jumping();
        }

        ledgeGrab();
        pushObject();
        ropeMovement();

        isGrounded = false;
        slopeAngle = 180;
        jumpButtonPressedThisFrame = false;
    }

    private void pushObject() {
        if (movementInput.x == 0) return;
        Vector2 rayPosition = new Vector2(transform.position.x, transform.position.y - m_CapsuleCollider2D.size.y / 3f);
        RaycastHit2D forwardCheck =
            Physics2D.Raycast(rayPosition, new Vector2(movementInput.x, 0), objectDistance, groundLayer);

        if (forwardCheck.collider == null) return;
        Rigidbody2D objectRigidbody = forwardCheck.collider.attachedRigidbody;
        if (objectRigidbody == null) return;
        if (objectRigidbody.mass > maxObjectMass) return;

        objectRigidbody.AddForce(new Vector2(movementInput.x * pushForce * (maxObjectMass / objectRigidbody.mass), 0));
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(new Vector2(transform.position.x, transform.position.y - m_CapsuleCollider2D.size.y / 3f),
            Vector2.right * objectDistance);
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
                               (hasCoyoteJump && jumpButtonPressedThisFrame) ||
                               (isOnRope && jumpButtonPressedThisFrame);
                if (canJump) {
                    jumpStartHeight = transform.position.y;
                    jumpStartTime = Time.time;

                    if (isOnRope) {
                        isOnRope = false;
                        ropeJoint.enabled = false;
                        ropeJoint.connectedBody = null;
                        lastRopeRelease = Time.time;
                    }

                    currentJumpState = JumpState.Jumping;
                    m_Rigidbody2D.velocity = new Vector2(m_Rigidbody2D.velocity.x, jumpSpeed);
                }

                if (!isGrounded && !hasCoyoteJump && !isOnRope) {
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
                if (isGrounded || isOnRope) {
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

    private bool canMove = true;
    private float lastLedgeGrab;

    private void ledgeGrab() {
        // if we are grounded, return
        if (isGrounded) return;

        float playerRadius = m_CapsuleCollider2D.size.x / 2f;

        Vector2 direction = new Vector2(movementInput.x, 0);

        //raycast forwards to check if we hit a ledge
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, ledgeGrabDistance, groundLayer);

        // if we didn't hit anything, return
        if (hit.collider == null) {
            // Debug.LogError("Did not find ledge collider returning.");
            return;
        }

        //move slightly into the ledge and up
        var circlePosition = new Vector2(
            hit.point.x + (playerRadius * Mathf.Sign(movementInput.x)),
            hit.point.y + ledgeGrabHeight);

        // at circlePosition, check if that point is a valid position for our player object
        Collider2D[] colliders = Physics2D.OverlapCircleAll(circlePosition, playerRadius, groundLayer);

        if (colliders.Length > 0) {
            Debug.LogError("IDK what the fuck this does but it failed.");
            for (int i = 0; i < colliders.Length; i++) {
                Debug.Log("Collider found: " + colliders[i].gameObject.name);
            }

            return;
        }

        direction = Vector2.down;

        //raycast downwards to find where the top of the ledge is
        RaycastHit2D downHit = Physics2D.Raycast(circlePosition, direction, ledgeGrabHeight, groundLayer);

        if (downHit.collider == null) {
            Debug.LogError("Did not find anywhere to cast down to.");
            return;
        }

        // teleport to the top of the ledge
        // TODO: change this with an animation instead so it looks better

        //this.transform.position = new Vector2(downHit.point.x, downHit.point.y + playerRadius);
        var ledgeCorner = new Vector3(transform.position.x, downHit.point.y + playerRadius, 0);
        Debug.Log("Found corner for ledging.");

        Utils.Instance.InvokeDelayed(ledgeGrabDelay, () =>
        {
            var path = new LTBezierPath(new Vector3[] {
                transform.position, ledgeCorner, ledgeCorner, new Vector3(downHit.point.x, downHit.point.y + playerRadius, 0)
            });
            Debug.Log("Calling LT.move");
            LeanTween.move(gameObject, path, ledgeFreezeTime);

            // CAN MOVE = FALSE;
            canMove = false;


            Debug.Log("Invoking ledge climb ending.");
            Utils.Instance.InvokeDelayed(ledgeFreezeTime, () =>
            {
                canMove = true;
                m_Rigidbody2D.velocity = Vector2.zero;
            });


            Debug.Log("Setting last ledge grab time.");
            lastLedgeGrab = Time.time + ledgeFreezeTime;
            // onLedgeGrab?.Invoke();
        });
    }

    private void ropeMovement() {
        if (isOnRope) {
            if (movementInput.y != 0) {
                ropeProgress -= (movementInput.y * ropeClimbingSpeed * rope.ClimbSpeedMultiplier * Time.deltaTime) /
                                rope.RopeLength;
                ropeProgress = Mathf.Clamp01(ropeProgress);

                Vector2 ropePosition = rope.GetRopePoint(ropeProgress);
                m_Rigidbody2D.position = ropePosition;
                ropeJoint.connectedBody = rope.GetRopePart(ropeProgress).rigidBody;
            }

            return;
        }
    }

    [Header("DEBUG")] public bool isGrounded = false;
    public float slopeAngle = 180;
    private Vector2 slopeNormal = Vector2.up;
    private float lastGroundedTime = 0f;


    [Header("ROPE")] private float lastRopeRelease;
    private bool isOnRope = false;
    private float ropeProgress;

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.CompareTag("Rope")) {
            //if we're not already on a rope
            if (!isOnRope && Time.time > lastRopeRelease + ropeGrabTimeout) {
                //Debug.Log("On Rope");
                //set the rope we're on
                rope = other.gameObject.GetComponentInParent<RopeController>();
                //set how far we are along the rope
                ropeProgress = rope.GetRopeProgress(transform.position);
                m_Rigidbody2D.position = rope.GetRopePoint(ropeProgress);
                //fix our joint to the rope
                ropeJoint.enabled = true;
                ropeJoint.connectedBody = rope.GetRopePart(ropeProgress).rigidBody;
                //set the player's onRope bool to true
                isOnRope = true;
            }
        }
    }

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