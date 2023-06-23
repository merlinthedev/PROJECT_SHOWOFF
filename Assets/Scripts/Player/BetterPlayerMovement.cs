using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class BetterPlayerMovement : MonoBehaviour {
    [SerializeField] private Player player;

    [Header("HORIZONTAL MOVEMENT")] [SerializeField]
    public Rigidbody2D m_Rigidbody2D;

    [SerializeField] private CapsuleCollider2D m_CapsuleCollider2D;
    [SerializeField] private PhysicsMaterial2D m_PlayerPhysicsMaterial2D;
    [SerializeField] private float maximumHorizontalSpeed = 4f;
    [SerializeField] private float groundAcceleration = 20f;
    [SerializeField] private float airAcceleration = 10f;
    [SerializeField] private float rbStickStrength = 0.5f;
    [SerializeField] private float maximumGroundAngle = 20f;
    [SerializeField] private bool affectGroundHorizontalOnly = true;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask ledgeGrabLayer;
    private bool hasJumpBuffer => isGrounded && (jumpButtonPressedTime + jumpBufferTime > Time.time);
    private bool hasCoyoteJump => !isGrounded && (lastGroundedTime + coyoteTime > Time.time);

    [Header("GROUNDCHECK")] [SerializeField]
    private int rayCount = 3;

    [SerializeField] private float rayLength = 0.1f;
    [SerializeField] private float rayWidth = 0.3f;
    [SerializeField] private Vector2 rayOffset;

    [Header("LEDGE GRABBING")] [SerializeField]
    private float ledgeGrabDistance;

    [SerializeField] private float ledgeGrabHeight = 1.2f;
    [SerializeField] private float ledgeGrabDelay = 0f;
    [SerializeField] private float ledgeFreezeTime = 0.5f;
    [SerializeField] private UnityEvent onLedgeGrab;
    [Header("CAN MOVE")] public bool canMove = true;
    private float lastLedgeGrab;

    [Header("PUSHING")] [SerializeField] private float pushForce = 5f;
    [SerializeField] private float maxObjectMass = 20f;
    [SerializeField] private float objectDistance = 0.7f;

    [Header("JUMPING")] [SerializeField] private float maxJumpHeight = 2f;
    [SerializeField] private float maxJumpTime = 0.4f;
    [SerializeField] private float coyoteTime = 0.2f;
    [SerializeField] private float jumpSpeed = 12f;
    [SerializeField] private float jumpBufferTime = 1.2f;
    [SerializeField] private float maximumFallSpeed = 80f;
    [SerializeField] private float gravityFallingMultiplier = 3f;

    public JumpState currentJumpState = JumpState.Falling;
    private float jumpStartHeight;
    private float jumpStartTime;

    public bool noJumpAllowed = false;

    [Header("ROPE CLIMBING")] [SerializeField]
    private RopeController rope;

    [SerializeField] private HingeJoint2D ropeJoint;
    [SerializeField] private Rigidbody2D ropeFollower;
    [SerializeField] private DistanceJoint2D ropeDistanceJoint;
    [SerializeField] private float ropeClimbingSpeed = 1.5f;
    [SerializeField] private float ropeGrabTimeout = 0.5f;
    [SerializeField] private float jumpHorizontalImpulse;
    [SerializeField] private float rotationCheckUpOffset = 0.5f;
    [SerializeField] private float rotationAngleOffset = 90f;
    private float rotationCheckRopeProgressOffset;
    private bool SmoothToRopeGrab = false;
    [SerializeField] private float ropeGrabSmoothTime = 0.5f;
    [SerializeField] private AnimationCurve ropeGrabSmoothCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    private float ropeGrabStartTime = 0f;
    private Quaternion ropeGrabStartRotation;
    private Vector3 ropeGrabStartPosition;
    [SerializeField] private float minimumJumpInput = -0.2f;

    [Header("VISUAL")] [SerializeField] private Transform visualTransform;
    private Vector3 originalVisualsPosition;
    private Vector3 initialScale;

    [Header("DEBUG")] private bool isGrounded = false;
    public bool IsGrounded = false;
    public float slopeAngle = 180;
    private Vector2 slopeNormal = Vector2.up;
    private float lastGroundedTime = 0f;
    private Collider2D lastGroundedCollider;

    [Header("ROPE")] private float lastRopeRelease;
    [SerializeField] private bool isOnRope = false;
    private float ropeProgress;


    private void updateVisuals() {
        if (visualTransform == null) return;

        IsGrounded = isGrounded;

        if (movementInput.x > 0) {
            visualTransform.localScale = initialScale;
        } else if (movementInput.x < 0) {
            visualTransform.localScale = Vector3.Scale(initialScale, new Vector3(-1, 1, 1));
        }
    }

    private void Start() {
        initialScale = visualTransform.localScale;
        originalVisualsPosition = visualTransform.localPosition;
        m_PlayerPhysicsMaterial2D.friction = 0f;
    }

    private void FixedUpdate() {
        //cast rays for ground check if our collider isn't touching anything,
        //used because we can slightly jump when walking over small rocks or ledges
        if (!isGrounded) {
            shootGroundRays();
        }

        if (canMove) {
            horizontalMovement();

            jumping();
        }

        ledgeGrab();
        pushObject();
        ropeMovement();

        updateVisuals();

        //reset our grounded state for the next physics step
        isGrounded = false;
        slopeAngle = 300;
        jumpButtonPressedThisFrame = false;
    }

    private void pushObject() {
        if (movementInput.x == 0) return;
        Vector2 rayPosition = new Vector2(transform.position.x,
            transform.position.y - m_CapsuleCollider2D.size.y / 3f + m_CapsuleCollider2D.offset.y);
        RaycastHit2D forwardCheck =
            Physics2D.Raycast(rayPosition, new Vector2(movementInput.x, 0), objectDistance, groundLayer);

        if (forwardCheck.collider == null) return;
        Rigidbody2D objectRigidbody = forwardCheck.collider.attachedRigidbody;
        if (objectRigidbody == null) return;
        if (objectRigidbody.mass > maxObjectMass) return;

        objectRigidbody.AddForce(new Vector2(movementInput.x * pushForce * (maxObjectMass / objectRigidbody.mass), 0));
    }

    private void horizontalMovement() {
        if (lastGroundedCollider != null) {
            if (isGrounded && lastGroundedCollider.attachedRigidbody != null) {
                Vector3 rbVelocity = lastGroundedCollider.attachedRigidbody.velocity;
                if (rbVelocity.magnitude > 0.1f) {
                    Vector2 rbStick = new Vector2(rbVelocity.x, rbVelocity.y) * rbStickStrength;
                    m_Rigidbody2D.AddForce(rbStick, ForceMode2D.Force);
                }
            }
        }

        float rawXMovement = movementInput.x;

        if (rawXMovement != 0 || isGrounded) {
            float desiredHorizontalSpeed =
                rawXMovement * (inWater ? maximumHorizontalSpeed * waterSpeedDebuff : maximumHorizontalSpeed);
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

    public void externalLocomotion(Vector3 destination) {
        canMove = false;

        player.GetPlayerAnimatorController().playAnimation();

        LeanTween.move(this.gameObject, destination, 6f).setEase(LeanTweenType.easeInOutQuad)
            .setOnComplete(() => {
                canMove = true;
            });
    }

    public enum JumpState {
        CanJump,
        Jumping,
        Falling
    }

    private void jumping() {
        switch (currentJumpState) {
            case JumpState.CanJump:
                if (noJumpAllowed) return;

                bool canJump = (hasJumpBuffer && jumpRequested) ||
                               (hasCoyoteJump && jumpButtonPressedThisFrame) ||
                               (isOnRope && jumpButtonPressedThisFrame);
                if (canJump) {
                    jumpStartHeight = transform.position.y - (inWater ? (1 - waterJumpDebuff) * maxJumpHeight : 0f);
                    jumpStartTime = Time.time;
                    jumpRequested = false;

                    if (isOnRope) {
                        ReleaseRope();

                        if (movementInput.y >= minimumJumpInput) {
                            m_Rigidbody2D.velocity = new Vector2(m_Rigidbody2D.velocity.x, jumpSpeed);
                            currentJumpState = JumpState.Jumping;
                        } else {
                            currentJumpState = JumpState.Falling;
                        }
                    } else {
                        m_Rigidbody2D.velocity = new Vector2(m_Rigidbody2D.velocity.x, jumpSpeed);
                        currentJumpState = JumpState.Jumping;
                    }
                }

                if (!isGrounded && !hasCoyoteJump && !isOnRope) {
                    currentJumpState = JumpState.Falling;
                }

                break;

            case JumpState.Jumping:
                m_Rigidbody2D.velocity =
                    new Vector2(m_Rigidbody2D.velocity.x, jumpSpeed * (inWater ? waterJumpDebuff : 1f));
                bool endJump = !jumpButtonPressed ||
                               slopeAngle is > 90 and < 200 ||
                               Time.time > jumpStartTime + maxJumpTime ||
                               transform.position.y > jumpStartHeight + maxJumpHeight;
                if (endJump) {
                    currentJumpState = JumpState.Falling;
                }

                break;

            case JumpState.Falling:
                if (isGrounded || isOnRope) {
                    currentJumpState = JumpState.CanJump;
                    if (isGrounded) {
                        player.GetPlayerAnimatorController().Ground(Time.time - jumpStartTime);
                    }
                }

                m_Rigidbody2D.AddForce(Vector2.down * (9.81f * gravityFallingMultiplier), ForceMode2D.Force);

                //clamp fall speed
                if (m_Rigidbody2D.velocity.y < -maximumFallSpeed) {
                    m_Rigidbody2D.velocity = new Vector2(m_Rigidbody2D.velocity.x, -maximumFallSpeed);
                }

                break;
        }
    }

    private bool isLedgeClimbing = false;


    private void ledgeGrab() {
        // if we are grounded, return
        if (inWater || isGrounded || isOnRope || isLedgeClimbing) return;

        if (m_Rigidbody2D.velocity.y > 0) return;

        float playerRadius = m_CapsuleCollider2D.size.x / 2f;

        Vector2 direction = new Vector2(movementInput.x, 0);

        //raycast forwards to check if we hit a ledge
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, ledgeGrabDistance, ledgeGrabLayer);

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
            Debug.Log("Tried to ledgeGrab but ledge not wide enough.");
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
        isLedgeClimbing = true;
        noJumpAllowed = true;

        Utils.Instance.InvokeDelayed(ledgeGrabDelay, () => {
            var path = new LTBezierPath(new Vector3[] {
                transform.position, ledgeCorner, ledgeCorner,
                new Vector3(downHit.point.x, downHit.point.y + playerRadius, 0)
            });
            Debug.Log("Calling LT.move");
            // Utils.Instance.InvokeDelayed(1f, () => );
            // set the player position to the final ledge position
            canMove = false;
            m_Rigidbody2D.velocity = Vector2.zero;
            m_Rigidbody2D.isKinematic = true;

            Utils.Instance.InvokeDelayed(0.6f, () => LeanTween.move(gameObject, path, ledgeFreezeTime + 0.4f));
            // CAN MOVE = FALSE;


            Debug.Log("Invoking ledge climb ending.");
            Utils.Instance.InvokeDelayed(ledgeFreezeTime, () => {
                canMove = true;
                m_Rigidbody2D.velocity = Vector2.zero;
            });


            Debug.Log("Setting last ledge grab time.");
            lastLedgeGrab = Time.time + ledgeFreezeTime;
            // onLedgeGrab?.Invoke();
            if (!hasTriggered) {
                player.GetPlayerAnimatorController().LedgeClimb();
                hasTriggered = true;
            }

            Utils.Instance.InvokeDelayed(2.05f, () => {
                m_Rigidbody2D.isKinematic = false;
                hasTriggered = false;
                isLedgeClimbing = false;
                noJumpAllowed = false;
            });
        });
    }

    private bool hasTriggered = false;

    public Vector2 MoveInput {
        get {
            return movementInput;
        }
    }

    public bool IsClimbing => isOnRope;

    private void ropeMovement() {
        if (isOnRope) {
            if (movementInput.y != 0) {
                ropeProgress -= (movementInput.y * ropeClimbingSpeed * rope.ClimbSpeedMultiplier * Time.deltaTime) /
                                rope.RopeLength;

                Debug.Log("Rope progress: " + ropeProgress);

                if (ropeProgress is > 1 or < 0) {
                    Debug.Log("Releasing rope.");
                    ReleaseRope();
                    return;
                }

                ropeProgress = Mathf.Clamp01(ropeProgress);


                Vector2 ropePosition = rope.GetRopePoint(ropeProgress);
                ropeFollower.position = ropePosition;
                ropeJoint.connectedBody = rope.GetRopePart(ropeProgress).rigidBody;
            }

            //update player visual on rope (rotate based on rope)
            var topRopePoint = rope.GetRopePoint(ropeProgress + rotationCheckRopeProgressOffset);
            var bottomRopePoint = rope.GetRopePoint(ropeProgress - rotationCheckRopeProgressOffset * 0.01f);

            var ropeDirection = topRopePoint - bottomRopePoint;
            var ropeAngle = Mathf.Atan2(ropeDirection.y, ropeDirection.x) * Mathf.Rad2Deg;

            if (SmoothToRopeGrab) {
                float animationProgress =
                    ropeGrabSmoothCurve.Evaluate(Mathf.Clamp01((Time.time - ropeGrabStartTime) / ropeGrabSmoothTime));
                visualTransform.rotation = Quaternion.Lerp(ropeGrabStartRotation,
                    Quaternion.Euler(0, 0, ropeAngle - rotationAngleOffset), animationProgress);
                visualTransform.position = Vector3.Lerp(ropeGrabStartPosition,
                    Vector3.Lerp(topRopePoint, bottomRopePoint, 0.5f), animationProgress);
                if (animationProgress == 1) SmoothToRopeGrab = false;
            } else {
                visualTransform.rotation = Quaternion.Euler(0, 0, ropeAngle - rotationAngleOffset);
                //position visual exactly between the two points
                visualTransform.position = Vector3.Lerp(topRopePoint, bottomRopePoint, 0.5f);
            }
        }
    }

    private void setRope() {
        if (isOnRope) {
            ropeProgress -= (movementInput.y * ropeClimbingSpeed * rope.ClimbSpeedMultiplier * Time.deltaTime) /
                            rope.RopeLength;
            ropeProgress = Mathf.Clamp01(ropeProgress);

            Vector2 ropePosition = rope.GetRopePoint(ropeProgress);
            ropeFollower.position = ropePosition;
            ropeJoint.connectedBody = rope.GetRopePart(ropeProgress).rigidBody;
        }
    }

    private RopeController lastRope;

    private void TryGrabRope(RopeController newRope) {
        if (newRope == lastRope && Time.time < lastRopeRelease + ropeGrabTimeout) return;
        if (isOnRope) return;

        //set the rope we're on
        rope = newRope;

        //set how far we are along the rope
        ropeProgress = rope.GetRopeProgress(transform.position);
        ropeProgress = Mathf.Clamp01(ropeProgress);

        //fix our joint to the rope
        ropeJoint.enabled = true;
        ropeJoint.connectedBody = rope.GetRopePart(ropeProgress).rigidBody;

        //set the player's onRope bool to true
        isOnRope = true;
        player.GetPlayerAnimatorController().RopeClimb();

        float currentDistance = Vector2.Distance(transform.position, rope.GetRopePoint(ropeProgress));
        ropeDistanceJoint.distance = currentDistance;
        ropeDistanceJoint.enabled = true;
        ropeFollower.bodyType = RigidbodyType2D.Dynamic;
        setRope();
        //tween distance of joint to almost zero
        LeanTween.value(gameObject, SetRopeDistanceJointLength, currentDistance, 0.005f, 3f);

        rotationCheckRopeProgressOffset = -rotationCheckUpOffset / rope.RopeLength;

        ropeGrabStartTime = Time.time;
        ropeGrabStartPosition = visualTransform.position;
        ropeGrabStartRotation = visualTransform.rotation;
        SmoothToRopeGrab = true;
    }

    void SetRopeDistanceJointLength(float value) {
        ropeDistanceJoint.distance = value;
    }

    private void ReleaseRope() {
        isOnRope = false;
        ropeJoint.enabled = false;
        player.GetPlayerAnimatorController().ResetSpeed();
        ropeJoint.connectedBody = null;
        lastRopeRelease = Time.time;
        ropeDistanceJoint.enabled = false;
        ropeFollower.bodyType = RigidbodyType2D.Kinematic;

        visualTransform.rotation = Quaternion.identity;
        visualTransform.localPosition = originalVisualsPosition;
        lastRope = rope;
        rope = null;


        Debug.Log("Off Rope");
        Debug.Log("IsClmbing: " + IsClimbing);
    }


    /*
     *
     * WATER STUFF
     * 
     */

    public bool inWater = false; // Move to water

    [SerializeField] private float waterJumpDebuff =
        0.6f;

    [SerializeField] private float waterSpeedDebuff =
        0.4f;

    public bool isInWater() {
        return inWater;
    }

    public void setInWater(bool value) {
        inWater = value;
    }

    private void shootGroundRays() {
        float raySpacing = rayWidth / (rayCount - 1);
        float rayStart = -rayWidth / 2f;

        for (int i = 0; i < rayCount; i++) {
            Vector2 rayOrigin = new Vector2(rayStart + i * raySpacing, 0) + (Vector2)transform.position + rayOffset;
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, rayLength, groundLayer);
            Debug.DrawRay(rayOrigin, Vector2.down * rayLength, Color.red);
            if (hit.collider != null) {
                if (Utils.IsInLayerMask(hit.collider.gameObject.layer, groundLayer)) {
                    isGrounded = true;
                    lastGroundedTime = Time.time;
                    slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
                    slopeNormal = hit.normal;
                    lastGroundedCollider = hit.collider;

                    //Set animation
                    player.GetPlayerAnimatorController().Ground(0);

                    return;
                }
            }
        }
    }

    public void setVelocity(Vector2 velocity) {
        m_Rigidbody2D.velocity = velocity;
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.CompareTag("Rope")) {
            //if we're not already on a rope
            TryGrabRope(other.gameObject.GetComponentInParent<RopeController>());
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
                lastGroundedCollider = other.collider;
            }

            lastGroundedTime = Time.time;
            isGrounded = slopeAngle < maximumGroundAngle;
        }
    }

    private Vector2 movementInput = Vector2.zero;

    public void OnMove(InputAction.CallbackContext context) {
        movementInput = context.ReadValue<Vector2>();
    }

    private bool jumpRequested = false;
    public bool jumpButtonPressed = false;
    private bool jumpButtonPressedThisFrame = false;
    private float jumpButtonPressedTime = 0f;

    public void OnJump(InputAction.CallbackContext context) {
        if (context.started) {
            jumpButtonPressedTime = Time.time;
            jumpButtonPressedThisFrame = true;
            jumpRequested = true;
        }

        if (context.performed) {
            jumpButtonPressed = true;
            if (!noJumpAllowed) player.GetPlayerAnimatorController().Jump();
        }

        if (context.canceled) {
            jumpButtonPressed = false;
            jumpRequested = false;
        }
    }

    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(new Vector2(transform.position.x,
                transform.position.y - m_CapsuleCollider2D.size.y / 3f + m_CapsuleCollider2D.offset.y),
            Vector2.right * objectDistance);

        Gizmos.color = Color.magenta;
        // Draw ground rays
        float raySpacing = rayWidth / (rayCount - 1);
        float rayStart = -rayWidth / 2f;

        for (int i = 0; i < rayCount; i++) {
            Vector2 rayOrigin = new Vector2(rayStart + i * raySpacing, 0) + (Vector2)transform.position + rayOffset;
            Gizmos.DrawRay(rayOrigin, Vector2.down * rayLength);
        }
    }

    public void setCanMove(bool canMove) {
        m_Rigidbody2D.isKinematic = false;
        this.canMove = canMove;
    }
}