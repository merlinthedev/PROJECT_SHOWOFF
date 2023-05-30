using UnityEngine;
using UnityEngine.InputSystem;
using UnityEditor;
using UnityEngine.Events;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

[SelectionBase]
public class PlayerController : MonoBehaviour {
    [Header("Movement")] [SerializeField] private float maxSpeed = 8f;
    [SerializeField] private float maxAirSpeed = 4f;
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float deceleration = 5f;
    [SerializeField] private AnimationCurve accelerationCurve;
    [SerializeField] private Vector2 forceScale = new Vector2(1f, 1f);
    [SerializeField] private float gravityScaleDrop = 10f;

    [SerializeField] private LayerMask groundLayerMask;

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

    [Header("Ledge")]
    [SerializeField] private float maxLedgeHeight = 2f;

    [SerializeField] private float ledgeCheckDistance = 0.6f;
    [SerializeField] private float ledgeFreezeTime = 0.5f;
    private float lastLedgeGrab = 0f;
    private bool moved = false;

    [Header("Rope")]
    [SerializeField] private FixedJoint2D ropeJoint;

    [SerializeField] private float ropeGrabTimeout = 0.5f;
    [SerializeField] private float ropeSpeedMultiplier = 1.5f;
    private bool isOnRope = false;
    private RopeController rope;
    private float ropeProgress = 0f;
    private float lastRopeRelease = 0f;

    [Header("Water")]
    [SerializeField] private float waterMovementSpeedDebuff = 0.5f;

    [SerializeField] private float waterGravityScale = 0.5f;
    [SerializeField] private float waterJumpForceDebuff = 0.5f;
    [SerializeField] private bool inWater = false;
    [SerializeField] private float waterLedgeDelay = 0.5f;
    private float lastWaterLeaveTime;


    [Header("Needs to move")]
    [SerializeField]
    private Player player;


    [Header("Events")] [SerializeField] private UnityEvent OnLedgeClimb;
    [SerializeField] private UnityEvent OnWhistle;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;

    [SerializeField] private AudioClip jumpSound;
    [SerializeField] private AudioClip walkSound;
    private bool walkSoundPlaying = false;
    private bool isMoving = false;

    [Header("Cheese")]
    public float cheeseStrength = 2f;

    public bool isCheesing = false;

    private void Start() {
        defaultVisualScale = visualsTransform.localScale;
    }

    private void FixedUpdate() {
        if (movementControlDisabled || Time.time < lastLedgeGrab) {
            this.rb.velocity = Vector3.zero;
            // Debug.Log("Can't move.", this);
            return;
        }

        isMoving = movementInput.x != 0;

        if (isMoving && !walkSoundPlaying) {
            audioSource.clip = walkSound;
            audioSource.volume = 0.4f;
            audioSource.Play();
            walkSoundPlaying = true;
        } else if (!isMoving && walkSoundPlaying) {
            audioSource.Stop();
            walkSoundPlaying = false;
        }

        float x = move();

        if (SurfaceCheck()) {
            applyMovement(x);
        }

        #region groundCheck

        // Groundcheck
        isGrounded = false;

        RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, Vector2.down, groundCheckRaycastDistance,
            groundLayerMask);
        foreach (RaycastHit2D hit in hits) {
            if (hit.collider.gameObject != gameObject) {
                //Debug.Log(hit.collider.gameObject.name, hit.collider.gameObject);
                isGrounded = true;
                moved = false;
                this.groundCollider = hit.collider;
                break;
            }
        }

        #endregion

        #region ledge

        // Ledge stuff
        if (!isGrounded && !this.inWater && rb.velocity.y <= 0 && !GetComponent<PlayerEventHandler>().Grabbing && !isOnRope && this.lastRopeRelease + 0.5f < Time.time) {
            checkLedge();
        }

        #endregion

        //gravity
        rb.AddForce(Vector2.down * (gravityScaleDrop * rb.mass * (inWater ? waterGravityScale : 1)));

        UpdateVisuals();
    }

    #region movement

    private bool SurfaceCheck() {
        // Save the bottom side of our collider to a vector2
        Vector2 bottom = new Vector2(col.bounds.center.x, col.bounds.min.y + 0.1f);
        RaycastHit2D hit = Physics2D.Raycast(bottom, Vector2.right * movementInput.x, 0.7f, LayerMask.GetMask("Ground"));
        if (hit.collider == null) {
            return true;
        }

        float angle = Vector2.Angle(Vector2.up, hit.normal);
        // Debug.Log("Hit " + hit.collider + " , " + hit.normal + " , " + angle);

        return angle < maxClimbAngle;
    }

    private void applyMovement(float x) {
        this.rb.sharedMaterial.friction = movementInput.x == 0 ? 1 : 0;
        x *= (inWater ? waterMovementSpeedDebuff : 1f);

        // Debug.Log("Applying movement: " + x);
        //Debug.Log("Current rb force: " + this.rb.velocity);
        if (this.groundCollider != null) {
            var groundRB = this.groundCollider.attachedRigidbody;
            if (groundRB != null && isGrounded) {
                // Q: Is there any way to calculate the exact force needed to keep the attachedRigidbody in place?
                // A: No, but we can calculate the force needed to keep the player in place, and then apply the opposite force to the ground.
                //    This is not perfect, but it's good enough for now.
                //    The reason this is not perfect is because the player is not always in the center of the groundCollider.
                //    This means that the force applied to the ground will not always be applied to the center of the groundCollider.
                //    This is not a problem for now, but it might be in the future.

                // Calculate the force needed to keep the player in place
                var force = Vector2.right * x * forceScale.x * rb.mass;
                // Apply the opposite force to the ground
                groundRB.AddForce(-force);

            }
        }

        this.rb.AddForce(Vector2.right * x * forceScale.x * rb.mass);


    }

    private float move() {
        float targetSpeed = movementInput.x * (isGrounded ? maxSpeed : maxAirSpeed) * (inWater ? waterMovementSpeedDebuff : 1);
        float speedDifference = targetSpeed - rb.velocity.x;
        float accelerationRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : deceleration;
        float movement =
            Mathf.Pow(Mathf.Abs(speedDifference) * accelerationRate,
                accelerationCurve.Evaluate(Mathf.Abs(speedDifference) * accelerationRate)) *
            Mathf.Sign(speedDifference);


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
                return 0;
            }
        }

        return movement;
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
            return;
        }

        //move slightly into the ledge and up
        var circlePosition = new Vector2(hit.point.x + (playerRadius * Mathf.Sign(movementInput.x)),
            hit.point.y + maxLedgeHeight);

        // at circlePosition, check if that point is a valid position for our player object
        Collider2D[] colliders = Physics2D.OverlapCircleAll(circlePosition, playerRadius, groundLayerMask);

        if (colliders.Length > 0) {
            return;
        }

        direction = Vector2.down;

        //raycast downwards to find where the top of the ledge is
        RaycastHit2D downHit = Physics2D.Raycast(circlePosition, direction, maxLedgeHeight, groundLayerMask);

        if (downHit.collider == null) {
            return;
        }

        //teleport to the top of the ledge
        //TODO: change this with an animation instead so it looks better

        //this.transform.position = new Vector2(downHit.point.x, downHit.point.y + playerRadius);
        var ledgeCorner = new Vector3(transform.position.x, downHit.point.y + playerRadius, 0);

        Utils.Instance.InvokeDelayed(.2f, () => {
            var path = new LTBezierPath(new Vector3[] {
                transform.position, ledgeCorner, ledgeCorner, new Vector3(downHit.point.x, downHit.point.y + playerRadius, 0)
            });
            LeanTween.move(gameObject, path, ledgeFreezeTime);


            rb.bodyType = RigidbodyType2D.Kinematic;
            col.enabled = false;


            Utils.Instance.InvokeDelayed(ledgeFreezeTime, () => {
                rb.bodyType = RigidbodyType2D.Dynamic;
                col.enabled = true;
            });


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
            if (jumpSound != null) {
                // AudioSource.PlayClipAtPoint(jumpSound, transform.position);
                audioSource.PlayOneShot(jumpSound);
            }

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