using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using Color = UnityEngine.Color;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class PlayerController : MonoBehaviour, IPlayerController {
    // Public for external hooks
    public Vector3 velocity { get; private set; }
    public FrameInput Input { get; private set; }
    public bool JumpingThisFrame { get; private set; }
    public bool landingThisFrame { get; private set; }
    public Vector3 rawMovement { get; private set; }
    public bool Grounded => colDown;

    private Vector3 lastPosition;
    private float currentHorizontalSpeed, currentVerticalSpeed;
    public bool canMove = true;

    private bool active;
    void Awake() => Invoke(nameof(activate), 0.5f);

    void activate() {
        this.active = true;
        a = this.fallClamp;
        b = this.minFallSpeed;
        c = this.maxFallSpeed;
    }

    private void Update() {
        if (!active) return;
        velocity = (transform.position - lastPosition) / Time.deltaTime;
        lastPosition = transform.position;

        gatherInput();
        runCollisionChecks();

        // ledge grabbing time
        ledgeGrabbing();

        if (!canMove) return;
        calculateWalk();
        calculateJumpApex();
        calculateGravity();
        calculateJump();

        moveCharacter();
    }

    #region Ledge Grabbing

    [Header("LEDGE GRABBING")] [SerializeField]
    private float maxLedgeHeight = 2f;

    [SerializeField] private float ledgeGrabDelay = 0.2f;
    [SerializeField] private float ledgeCheckDistance = 0.6f;
    [SerializeField] private float ledgeFreezeTime = 0.5f;
    [SerializeField] private UnityEvent onLedgeGrab;
    private float lastLedgeGrab = 0f;
    private float playerRadius = 0.5f;

    private void ledgeGrabbing() {
        // if we are grounded, return
        if (this.colDown) return;

        Vector2 direction = new Vector2(this.Input.X, 0);

        //raycast forwards to check if we hit a ledge
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, ledgeCheckDistance, groundLayer);

        // if we didn't hit anything, return
        if (hit.collider == null) {
            // Debug.LogError("Did not find ledge collider returning.");
            return;
        }

        //move slightly into the ledge and up
        var circlePosition = new Vector2(hit.point.x + (playerRadius * Mathf.Sign(rawMovement.x)),
            hit.point.y + maxLedgeHeight);

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
        RaycastHit2D downHit = Physics2D.Raycast(circlePosition, direction, maxLedgeHeight, groundLayer);

        if (downHit.collider == null) {
            Debug.LogError("Did not find anywhere to cast down to.");
            return;
        }

        // teleport to the top of the ledge
        // TODO: change this with an animation instead so it looks better

        //this.transform.position = new Vector2(downHit.point.x, downHit.point.y + playerRadius);
        var ledgeCorner = new Vector3(transform.position.x, downHit.point.y + playerRadius, 0);
        Debug.Log("Found corner for ledging.");

        Utils.Instance.InvokeDelayed(ledgeGrabDelay, () => {
            var path = new LTBezierPath(new Vector3[] {
                transform.position, ledgeCorner, ledgeCorner,
                new Vector3(downHit.point.x, downHit.point.y + playerRadius, 0)
            });
            Debug.Log("Calling LT.move");
            LeanTween.move(gameObject, path, ledgeFreezeTime);

            // CAN MOVE = FALSE;
            canMove = false;


            Debug.Log("Invoking ledge climb ending.");
            Utils.Instance.InvokeDelayed(ledgeFreezeTime, () => {
                canMove = true;
            });


            Debug.Log("Setting last ledge grab time.");
            lastLedgeGrab = Time.time + ledgeFreezeTime;
            // onLedgeGrab?.Invoke();
        });
    }

    #endregion

    #region Gather Input

    private void gatherInput() {
        Input = new FrameInput {
            JumpDown = UnityEngine.Input.GetButtonDown("Jump"),
            JumpUp = UnityEngine.Input.GetButtonUp("Jump"),
            X = UnityEngine.Input.GetAxisRaw("Horizontal"),
            Y = UnityEngine.Input.GetAxisRaw("Vertical")
        };
        if (Input.JumpDown) {
            lastJumpPressed = Time.time;
        }
    }

    #endregion

    #region Collisions

    [Header("COLLISION")] [SerializeField] private Bounds characterBounds;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private int detectorCount = 3;
    [SerializeField] private float detectionRayLength = 0.1f;
    [SerializeField] [Range(0.1f, 0.3f)] private float rayBuffer = 0.1f; // Prevents side detectors hitting the ground

    private RayRange raysUp, raysRight, raysDown, raysLeft;
    private bool colUp, colRight, colDown, colLeft;

    private float timeLeftGrounded;

    // We use these raycast checks for pre-collision information
    private void runCollisionChecks() {
        // Generate ray ranges. 
        calculateRayRanged();

        // Ground
        landingThisFrame = false;
        bool groundedCheck = runDetection(raysDown);
        if (colDown && !groundedCheck) timeLeftGrounded = Time.time; // Only trigger when first leaving
        else if (!colDown && groundedCheck) {
            coyoteUsable = true; // Only trigger when first touching
            landingThisFrame = true;
        }

        colDown = groundedCheck;

        // The rest
        colUp = runDetection(raysUp);
        colLeft = runDetection(raysLeft);
        colRight = runDetection(raysRight);

        bool runDetection(RayRange range) {
            return evaluateRayPositions(range)
                .Any(point => Physics2D.Raycast(point, range.Dir, detectionRayLength, groundLayer));
        }
    }

    private void calculateRayRanged() {
        // This is crying out for some kind of refactor. 
        var b = new Bounds(transform.position + characterBounds.center, characterBounds.size);

        raysDown = new RayRange(b.min.x + rayBuffer, b.min.y, b.max.x - rayBuffer, b.min.y, Vector2.down);
        raysUp = new RayRange(b.min.x + rayBuffer, b.max.y, b.max.x - rayBuffer, b.max.y, Vector2.up);
        raysLeft = new RayRange(b.min.x, b.min.y + rayBuffer, b.min.x, b.max.y - rayBuffer, Vector2.left);
        raysRight = new RayRange(b.max.x, b.min.y + rayBuffer, b.max.x, b.max.y - rayBuffer, Vector2.right);
    }


    private IEnumerable<Vector2> evaluateRayPositions(RayRange range) {
        for (int i = 0; i < detectorCount; i++) {
            float t = (float)i / (detectorCount - 1);
            yield return Vector2.Lerp(range.Start, range.End, t);
        }
    }

    private void OnDrawGizmos() {
        // Bounds
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position + characterBounds.center, playerRadius);

        // Rays
        if (!Application.isPlaying) {
            calculateRayRanged();
            Gizmos.color = Color.blue;
            foreach (var range in new List<RayRange> { raysUp, raysRight, raysDown, raysLeft }) {
                foreach (var point in evaluateRayPositions(range)) {
                    Gizmos.DrawRay(point, range.Dir * detectionRayLength);
                }
            }
        }

        if (!Application.isPlaying) return;

        // Draw the future position. Handy for visualizing gravity
        Gizmos.color = Color.magenta;
        var move = new Vector3(currentHorizontalSpeed, currentVerticalSpeed) * Time.deltaTime;
        // Gizmos.DrawWireCube(transform.position + characterBounds.center + move, characterBounds.size);
        Gizmos.DrawWireSphere(transform.position + characterBounds.center + move, playerRadius);
    }

    #endregion

    #region Rope

    [Header("Rope")] [SerializeField] private FixedJoint2D ropeJoint;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private float ropeGrabTimeout = 0.5f;
    [SerializeField] private float ropeSpeedMultiplier = 1.5f;
    private bool isOnRope = false;
    private RopeController rope;
    private float ropeProgress = 0f;
    private float lastRopeRelease = 0f;

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

    #endregion

    #region Walk

    [Header("WALKING")] [SerializeField] private float acceleration = 90;
    [SerializeField] private float moveClamp = 13;
    [SerializeField] private float deceleration = 60f;
    [SerializeField] private float apexBonus = 2;

    private void calculateWalk() {
        if (this.isOnRope) {
            this.currentHorizontalSpeed = 0;
            if (Input.Y != 0) {
                this.ropeProgress -= (Input.Y * this.ropeSpeedMultiplier * Time.deltaTime) / this.rope.RopeLength;
                this.ropeProgress = Mathf.Clamp01(ropeProgress);

                Vector2 ropePosition = this.rope.GetRopePoint(this.ropeProgress);
                this.rb.position = ropePosition;
                this.ropeJoint.connectedBody = this.rope.GetRopePart(this.ropeProgress).rigidBody;
            }

            return;
        }

        if (Input.X != 0) {
            // Set horizontal move speed
            currentHorizontalSpeed += Input.X * acceleration * Time.deltaTime;

            // clamped by max frame movement
            currentHorizontalSpeed = Mathf.Clamp(currentHorizontalSpeed, -moveClamp, moveClamp);

            // Apply bonus at the apex of a jump
            var m_ApexBonus = Mathf.Sign(Input.X) * this.apexBonus * apexPoint;
            currentHorizontalSpeed += m_ApexBonus * Time.deltaTime;
        } else {
            // No input. Let's slow the character down
            currentHorizontalSpeed = Mathf.MoveTowards(currentHorizontalSpeed, 0, deceleration * Time.deltaTime);
        }

        if (currentHorizontalSpeed > 0 && colRight || currentHorizontalSpeed < 0 && colLeft) {
            // Don't walk through walls
            currentHorizontalSpeed = 0;
        }
    }

    #endregion

    #region Gravity

    [Header("GRAVITY")] [SerializeField] private float fallClamp = -40f;
    [SerializeField] private float minFallSpeed = 80f;
    [SerializeField] private float maxFallSpeed = 120f;
    private float fallSpeed;

    private void calculateGravity() {
        if (colDown) {
            // Move out of the ground
            if (currentVerticalSpeed < 0) currentVerticalSpeed = 0;
        } else {
            if (this.isOnRope) {
                this.currentVerticalSpeed = 0;
                return;
            }

            // Add downward force while ascending if we ended the jump early
            var m_FallSpeed = endedJumpEarly && currentVerticalSpeed > 0
                ? fallSpeed * jumpEndEarlyGravityModifier
                : fallSpeed;

            // Fall
            currentVerticalSpeed -= m_FallSpeed * Time.deltaTime;

            // Clamp
            if (currentVerticalSpeed < fallClamp) currentVerticalSpeed = fallClamp;
        }
    }

    #endregion

    #region Jump

    [Header("JUMPING")] [SerializeField] private float jumpHeight = 30;
    [SerializeField] private float jumpApexThreshold = 10f;
    [SerializeField] private float coyoteTimeThreshold = 0.1f;
    [SerializeField] private float jumpBuffer = 0.1f;
    [SerializeField] private float jumpEndEarlyGravityModifier = 3;

    private bool canJump = true;

    private bool coyoteUsable;
    private bool endedJumpEarly = true;
    private float apexPoint; // Becomes 1 at the apex of a jump
    private float lastJumpPressed;
    private bool canUseCoyote => coyoteUsable && !colDown && timeLeftGrounded + coyoteTimeThreshold > Time.time;
    private bool hasBufferedJump => colDown && lastJumpPressed + jumpBuffer > Time.time;

    private void calculateJumpApex() {
        if (!colDown) {
            // Gets stronger the closer to the top of the jump
            apexPoint = Mathf.InverseLerp(jumpApexThreshold, 0, Mathf.Abs(velocity.y));
            fallSpeed = Mathf.Lerp(minFallSpeed, maxFallSpeed, apexPoint);
        } else {
            apexPoint = 0;
        }
    }

    private void calculateJump() {
        // Jump if: grounded or within coyote threshold || sufficient jump buffer
        if (!this.canJump) return;

        if (Input.JumpDown && (canUseCoyote || hasBufferedJump || this.isOnRope || this.inWater)) {
            currentVerticalSpeed = this.inWater ? this.jumpHeight / 2 : this.jumpHeight;
            endedJumpEarly = false;
            coyoteUsable = false;
            timeLeftGrounded = float.MinValue;
            JumpingThisFrame = true;

            if (this.isOnRope) {
                this.ropeJoint.enabled = false;
                this.isOnRope = false;
                this.lastRopeRelease = Time.time;
            }
        } else {
            JumpingThisFrame = false;
        }

        // End the jump early if button released
        if (!colDown && Input.JumpUp && !endedJumpEarly && velocity.y > 0) {
            // _currentVerticalSpeed = 0;
            endedJumpEarly = true;
        }

        if (colUp) {
            if (currentVerticalSpeed > 0) currentVerticalSpeed = 0;
        }
    }

    public void setCanJump(bool value) {
        this.canJump = value;
    }

    #endregion

    #region Water

    private bool inWater = false;
    private bool isTraveling = true;
    private float a, b, c;

    public void onWaterEnter() {
        this.inWater = true;

        this.disableManualGravity();
    }

    public void onWaterExit() {
        this.inWater = false;

        this.enableManualGravity();
    }

    public bool isInWater() {
        return this.inWater;
    }

    public void setTraveling(bool value) {
        this.isTraveling = value;
    }

    private void disableManualGravity() {
        this.fallClamp = 0;
        this.minFallSpeed = 0;
        this.maxFallSpeed = 0;

        this.rb.gravityScale = 1;
    }

    private void enableManualGravity() {
        this.fallClamp = a;
        this.minFallSpeed = b;
        this.maxFallSpeed = c;

        this.rb.gravityScale = 0;
    }

    private void OnCollisionStay2D(Collision2D other) {
        if (other.gameObject.CompareTag("Boat")) {
            var boat = other.gameObject.GetComponent<Boat>();
            if (boat == null) return;

            if (boat.playerInBoat && this.isTraveling) {
                Debug.Log("Player in boat.", this);
                this.canJump = false;
            }
        }
    }

    #endregion

    #region Move

    [Header("MOVE")]
    [SerializeField, Tooltip("Raising this value increases collision accuracy at the cost of performance.")]
    private int freeColliderIterations = 10;

    // We cast our bounds before moving to avoid future collisions
    private void moveCharacter() {
        Vector3 pos = transform.position + characterBounds.center;
        rawMovement = new Vector3(currentHorizontalSpeed, currentVerticalSpeed); // Used externally
        Vector3 move = rawMovement * Time.deltaTime;
        Vector3 furthestPoint = pos + move;

        // check furthest movement. If nothing hit, move and don't do extra checks
        // Collider2D hit = Physics2D.OverlapBox(furthestPoint, characterBounds.size, 0, groundLayer);
        Collider2D hit = Physics2D.OverlapCircle(furthestPoint, playerRadius, groundLayer);
        if (!hit) {
            transform.position += move;
            // this.rb.position += (Vector2)move;
            return;
        }

        // otherwise increment away from current pos; see what closest position we can move to
        var positionToMoveTo = transform.position;
        for (int i = 1; i < freeColliderIterations; i++) {
            // increment to check all but furthestPoint - we did that already
            var t = (float)i / freeColliderIterations;
            var posToTry = Vector2.Lerp(pos, furthestPoint, t);

            Collider2D c2d = Physics2D.OverlapCircle(posToTry, this.playerRadius, this.groundLayer);
            if (c2d) {
                if (c2d.gameObject.GetComponent<Rigidbody2D>() != null) {
                    // c2d.gameObject.GetComponent<Rigidbody2D>()
                    //     .AddForce(new Vector2(this.currentHorizontalSpeed * 100, 0), ForceMode2D.Force);
                    Debug.Log("Should be adding force here.");
                }

                transform.position = positionToMoveTo;
                // this.rb.position = positionToMoveTo;

                // We've landed on a corner or hit our head on a ledge. Nudge the player gently
                if (i == 1) {
                    if (currentVerticalSpeed < 0) currentVerticalSpeed = 0;
                    var dir = transform.position - hit.transform.position;
                    transform.position += dir.normalized * move.magnitude;
                    // this.rb.position += ((Vector2)dir.normalized * move.magnitude);
                }

                return;
            }


            positionToMoveTo = posToTry;
        }
    }

    #endregion
}

public struct FrameInput {
    public float X;
    public float Y;
    public bool JumpDown;
    public bool JumpUp;
}

public interface IPlayerController {
    Vector3 velocity { get; }
    FrameInput Input { get; }
    bool JumpingThisFrame { get; }
    bool landingThisFrame { get; }
    Vector3 rawMovement { get; }
    bool Grounded { get; }
}

public struct RayRange {
    public RayRange(float x1, float y1, float x2, float y2, Vector2 dir) {
        Start = new Vector2(x1, y1);
        End = new Vector2(x2, y2);
        Dir = dir;
    }

    public readonly Vector2 Start, End, Dir;
}