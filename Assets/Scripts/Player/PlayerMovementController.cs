using UnityEngine;
using UnityEngine.InputSystem;
using UnityEditor;
using UnityEngine.Events;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

[SelectionBase]
public class PlayerMovementController : MonoBehaviour {
    [Header("Movement")] [SerializeField] private float maxSpeed = 8f;
    [SerializeField] private float maxAirSpeed = 4f;
    [SerializeField] private float acceleration = 90f;
    [SerializeField] private float deceleration = 60f;
    [SerializeField] private Vector2 forceScale = new Vector2(1f, 1f);
    [SerializeField] private float gravityScaleDrop = 10f;
    [SerializeField] private LayerMask groundLayerMask;
    [SerializeField] private LayerMask checkForValidPositions;
    [SerializeField] private Vector2 movementInput;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Collider2D col;
    [SerializeField] private bool movementControlDisabled = false;
    [SerializeField] private float maxClimbAngle = 30;


    [Header("Visuals")] [SerializeField] private Transform visualsTransform;
    [SerializeField] private float playerRadius = 1f;
    private Vector3 defaultVisualScale;


    [Header("Jump")] [SerializeField] private bool isGrounded = false;
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private float groundCheckRaycastDistance = 0.5f;
    [SerializeField] private Collider2D groundCollider;
    [SerializeField] private Transform groundCheckTransform;

    [Header("Ledge")] [SerializeField] private float maxLedgeHeight = 2f;
    [SerializeField] private float ledgeGrabDelay = 0.2f;
    [SerializeField] private float ledgeCheckDistance = 0.6f;
    [SerializeField] private float ledgeFreezeTime = 0.5f;
    private float lastLedgeGrab = 0f;
    private bool moved = false;

    [Header("Rope")] [SerializeField] private FixedJoint2D ropeJoint;
    [SerializeField] private float ropeGrabTimeout = 0.5f;
    [SerializeField] private float ropeSpeedMultiplier = 1.5f;
    private bool isOnRope = false;
    private RopeController rope;
    private float ropeProgress = 0f;
    private float lastRopeRelease = 0f;

    [Header("Water")] [SerializeField] private float waterMovementSpeedDebuff = 0.5f;
    [SerializeField] private float waterGravityScale = 0.5f;
    [SerializeField] private float waterJumpForceDebuff = 0.5f;
    [SerializeField] private bool inWater = false;
    [SerializeField] private float waterLedgeDelay = 0.5f;
    private float lastWaterLeaveTime;


    [Header("Needs to move")] [SerializeField] private Player player;
    private Vector3 rawMovement;
    [SerializeField] private float fallClamp = -20f;
    [SerializeField] private float minFallSpeed = 30f;
    [SerializeField] private float maxFallSpeed = 50f;
    private float fallSpeed = 0f;
    private Vector3 velocity;
    private Vector3 lastPosition;
    private FrameInput Input;

    [Header("Events")] [SerializeField] private UnityEvent OnLedgeClimb;
    [SerializeField] private UnityEvent OnWhistle;
    private bool isMoving = false;

    [Header("Cheese")] public float cheeseStrength = 2f;
    public bool isCheesing = false;

    private void Start() {
        defaultVisualScale = visualsTransform.localScale;
    }

    private void FixedUpdate() {
        velocity = (transform.position - lastPosition) / Time.fixedDeltaTime;

        lastPosition = transform.position;

        gatherInput();

        isMoving = movementInput.x != 0 || Input.X != 0;

        player.GetPlayerAudioController().PlayWalkingSound(isMoving);

        calculateMovement();
        calculateJumpApex();
        calculateGravity();
        calculateJump();

        applyMovement();

        UpdateVisuals();

        return;

        #region ledge

        // Ledge stuff, ADD GROUNDED CHECK ONCE IT IS REFACTORED
        if (!this.inWater && rb.velocity.y <= 0 && !this.player.GetPlayerEventHandler().Grabbing &&
            !isOnRope &&
            this.lastRopeRelease + 0.5f < Time.time) {
            Debug.Log("Going to check ledge.");
            checkLedge();
        }

        #endregion

    }

    #region movement

    private void gatherInput() {
        Input = new FrameInput {
            JumpDown = UnityEngine.Input.GetButtonDown("Jump"),
            JumpUp = UnityEngine.Input.GetButtonUp("Jump"),
            X = UnityEngine.Input.GetAxisRaw("Horizontal")
        };
        if (Input.JumpDown) {
            lastJumpPressed = Time.time;
        }
    }

    private void applyMovement() {
        currentHorizontalSpeed *= (inWater ? waterMovementSpeedDebuff : 1f);
        rawMovement = new Vector3(currentHorizontalSpeed, currentVerticalSpeed);

        var move = rawMovement * Time.fixedDeltaTime;
        this.rb.velocity = move;

        // if (this.groundCollider != null) {
        //     var groundRB = this.groundCollider.attachedRigidbody;
        //     if (groundRB != null && isGrounded) {
        //         var force = Vector2.right * currentHorizontalSpeed * forceScale.x * rb.mass;
        //         // Apply the opposite force to the ground
        //         // Debug.Log("Applying force to ground: " + force + " , " + groundRB + " , " + groundRB.gameObject.name);
        //         if (groundRB.gameObject.GetComponent<ObjectGrabbable>() != null) {
        //             if (groundRB.gameObject.GetComponent<ObjectGrabbable>().isWater) {
        //                 groundRB.AddForce(-force);
        //             }
        //         }
        //     }
        // }

    }

    private float currentHorizontalSpeed;
    private float currentVerticalSpeed;

    private void calculateMovement() {
        if (Input.X != 0) {
            currentHorizontalSpeed += Input.X * acceleration * Time.fixedDeltaTime;
            currentHorizontalSpeed = Mathf.Clamp(currentHorizontalSpeed, -maxSpeed, maxSpeed);
        } else {
            currentHorizontalSpeed = Mathf.MoveTowards(currentHorizontalSpeed, 0, deceleration * Time.fixedDeltaTime);
        }

        /*
         * Rope stuff 
         */
        if (isOnRope) {
            if (movementInput.y != 0) {
                ropeProgress -= (movementInput.y * ropeSpeedMultiplier * Time.fixedDeltaTime) / rope.RopeLength;
                ropeProgress = Mathf.Clamp01(ropeProgress);

                Vector2 ropePosition = rope.GetRopePoint(ropeProgress);
                rb.position = ropePosition;
                ropeJoint.connectedBody = rope.GetRopePart(ropeProgress).rigidBody;
                ropeJoint.connectedAnchor = ropePosition;
            }
        }

    }

    private void calculateGravity() {
        if (isGrounded) {
            if (currentVerticalSpeed < 0) currentVerticalSpeed = 0;
        } else {
            var m_FallSpeed = endedJumpEarly && currentVerticalSpeed > 0 ? fallSpeed * jumpEndEarlyGravityModifier : fallSpeed;

            currentVerticalSpeed -= m_FallSpeed * Time.fixedDeltaTime;

            currentVerticalSpeed = Mathf.Clamp(currentVerticalSpeed, fallClamp, Mathf.Infinity);
        }
    }

    [SerializeField] private float jumpHeight = 30f;
    [SerializeField] private float jumpApexThreshold = 10f;
    [SerializeField] private float coyoteTimeThreshold = 0.1f;
    [SerializeField] private float jumpBuffer = 0.1f;
    [SerializeField] private float jumpEndEarlyGravityModifier = 3f;

    private bool coyoteUsable;
    private bool endedJumpEarly = true;
    private float apexPoint;
    private float lastJumpPressed;

    private float timeLeftGrounded;

    private bool canUseCoyote => coyoteUsable && !isGrounded && timeLeftGrounded + coyoteTimeThreshold > Time.time;
    private bool hasBufferedJump => isGrounded && lastJumpPressed + jumpBuffer > Time.time;

    private bool jumpingThisFrame;

    private void calculateJumpApex() {
        if (!isGrounded) {
            apexPoint = Mathf.InverseLerp(jumpApexThreshold, 0, Mathf.Abs(velocity.y));
            fallSpeed = Mathf.Lerp(minFallSpeed, maxFallSpeed, apexPoint);
        } else {
            apexPoint = 0;
        }
    }

    private void calculateJump() {
        if (Input.JumpDown && canUseCoyote || hasBufferedJump) {
            currentVerticalSpeed = jumpHeight;
            endedJumpEarly = false;
            coyoteUsable = false;
            timeLeftGrounded = float.MinValue;
            jumpingThisFrame = true;
        } else {
            jumpingThisFrame = false;
        }

        if (!isGrounded && Input.JumpUp && !endedJumpEarly && velocity.y > 0) {
            endedJumpEarly = true;
        }
    }

    #endregion

    #region ledge

    private void checkLedge() {
        //direction the player is facing horizontally
        Vector2 direction = new Vector2(movementInput.x, 0);
        Debug.Log("dir: " + direction.ToString());

        //raycast forwards to check if we hit a ledge
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, ledgeCheckDistance, groundLayerMask);

        // if we didn't hit anything, return
        if (hit.collider == null) {
            Debug.LogError("Did not find ledge collider returning.");
            return;
        }

        //move slightly into the ledge and up
        var circlePosition = new Vector2(hit.point.x + (playerRadius * Mathf.Sign(movementInput.x)),
            hit.point.y + maxLedgeHeight);

        // at circlePosition, check if that point is a valid position for our player object
        Collider2D[] colliders = Physics2D.OverlapCircleAll(circlePosition, playerRadius, checkForValidPositions);

        if (colliders.Length > 0) {
            Debug.LogError("IDK what the fuck this does but it failed.");
            return;
        }

        direction = Vector2.down;

        //raycast downwards to find where the top of the ledge is
        RaycastHit2D downHit = Physics2D.Raycast(circlePosition, direction, maxLedgeHeight, groundLayerMask);

        if (downHit.collider == null) {
            Debug.LogError("Did not find anywhere to cast down to.");
            return;
        }

        //teleport to the top of the ledge
        //TODO: change this with an animation instead so it looks better

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


            rb.bodyType = RigidbodyType2D.Kinematic;
            col.enabled = false;


            Debug.Log("Invoking ledge climb ending.");
            Utils.Instance.InvokeDelayed(ledgeFreezeTime, () =>
            {
                rb.bodyType = RigidbodyType2D.Dynamic;
                col.enabled = true;
            });


            Debug.Log("Setting last ledge grab time.");
            lastLedgeGrab = Time.time + ledgeFreezeTime;
            OnLedgeClimb?.Invoke();
        });
    }

    #endregion

    #region visuals

    private void UpdateVisuals() {
        if (visualsTransform == null) {
            return;
        }

        //flip our visuals if we are goinf in the other direction
        if (movementInput.x > 0) {
            visualsTransform.localScale = defaultVisualScale;
        } else if (movementInput.x < 0) {
            visualsTransform.localScale = Vector3.Scale(defaultVisualScale, new Vector3(-1, 1, 1));
        }

        this.player.GetPlayerProjectileController().UpdateProjectile();
    }

    #endregion

    private void OnTriggerEnter2D(Collider2D collision) {
        //rope check
        if (collision.gameObject.CompareTag("Rope")) {
            //if we're not already on a rope
            if (!isOnRope && Time.time > lastRopeRelease + ropeGrabTimeout) {
                //Debug.Log("On Rope");
                //set the rope we're on
                rope = collision.gameObject.GetComponentInParent<RopeController>();
                //set how far we are along the rope
                ropeProgress = rope.GetRopeProgress(transform.position);
                rb.position = rope.GetRopePoint(ropeProgress);
                //fix our joint to the rope
                ropeJoint.enabled = true;
                ropeJoint.connectedBody = rope.GetRopePart(ropeProgress).rigidBody;
                //set the player's onRope bool to true
                isOnRope = true;
            }
        }
    }

    #region Input

    public void DoMove(InputAction.CallbackContext context) {
        movementInput = context.ReadValue<Vector2>();
    }

    public void DoJump(InputAction.CallbackContext context) {
        if (context.performed) {
            if (isGrounded || isOnRope) {
                //Debug.LogWarning("Jump");
                // rb.AddForce(Vector2.up * jumpForce * (inWater ? waterJumpForceDebuff : 1) * (isCheesing ? cheeseStrength : 1f),
                //     ForceMode2D.Impulse);
                var groundRB = this.groundCollider.attachedRigidbody;
                if (this.isCheesing) {
                    this.rb.AddForce(Vector2.up * this.jumpForce * this.cheeseStrength, ForceMode2D.Impulse);
                } else if (this.inWater) {
                    this.rb.AddForce(Vector2.up * this.jumpForce * this.waterJumpForceDebuff, ForceMode2D.Impulse);
                } else {
                    this.rb.AddForce(Vector2.up * this.jumpForce, ForceMode2D.Impulse);
                    if (groundRB != null)
                        groundRB.AddForceAtPosition(Vector2.down * this.jumpForce, this.rb.position);
                }
            }

            if (isOnRope) {
                //Debug.LogWarning("Jump");
                ropeJoint.enabled = false;
                isOnRope = false;
                lastRopeRelease = Time.time;
            }

            player.GetPlayerAudioController().PlayJumpSound();
        }
    }

    #endregion

    #region debug

    private void OnDrawGizmos() {
        // draw circle around player based on radius
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, playerRadius);

        //if not in play mode
        if (!Application.isPlaying) {
            //if selected
            if (Selection.activeGameObject == gameObject) {
                //draw a line in the direction the player is facing
                Gizmos.color = Color.red;
                Gizmos.DrawLine(transform.position, transform.position + Vector3.right * ledgeCheckDistance);
                //draw a line downwards from the ledge check position
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(transform.position + Vector3.right * ledgeCheckDistance,
                    transform.position + Vector3.right * ledgeCheckDistance + Vector3.up * maxLedgeHeight);
                //draw a circle at the ledge check position
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(
                    transform.position + Vector3.right * ledgeCheckDistance + Vector3.up * maxLedgeHeight,
                    playerRadius);
            }
        } else {
            //if selected
            if (Selection.activeGameObject == gameObject) {
                //draw a line in the direction the player is facing
                Gizmos.color = Color.red;
                Gizmos.DrawLine(transform.position,
                    transform.position + (Vector3.right * Mathf.Sign(movementInput.x) * ledgeCheckDistance));
                //draw a line downwards from the ledge check position
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(transform.position + Vector3.right * ledgeCheckDistance,
                    transform.position + Vector3.right * ledgeCheckDistance + Vector3.up * maxLedgeHeight);
                //draw a circle at the ledge check position
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(
                    transform.position + Vector3.right * ledgeCheckDistance + Vector3.up * maxLedgeHeight,
                    playerRadius);

                Gizmos.color = Color.magenta;
                Gizmos.DrawLine(transform.position, transform.position + Vector3.right * movementInput.x);
            }
        }
    }

    #endregion

    #region getter & setter

    public bool IsGrounded() {
        return this.isGrounded;
    }

    public bool IsInWater() {
        return this.inWater;
    }

    public void SetInWater(bool inWater) {
        this.inWater = inWater;
    }

    #endregion

}