using UnityEngine;
using UnityEngine.InputSystem;
using UnityEditor;
using UnityEngine.Events;

public class PlayerController : MonoBehaviour {
    [Header("Movement")]
    [SerializeField] private float maxSpeed = 8f;
    [SerializeField] private float maxAirSpeed = 4f;
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float deceleration = 5f;
    [SerializeField] private AnimationCurve accelerationCurve;
    [SerializeField] private Vector2 forceScale = new Vector2(1f, 1f);
    [SerializeField] private float gravityScaleDrop = 10f;

    [SerializeField] private LayerMask groundLayerMask;

    public Vector2 movementInput;
    public Rigidbody2D rb;
    public Collider2D col;
    public bool movementControlDisabled = false;

    [Header("Visuals")]
    [SerializeField] private Transform visualsTransform;
    private Vector3 defaultVisualScale;

    [Header("Jump")]
    public bool isGrounded = false;
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private float groundCheckRaycastDistance = 0.5f;

    private bool moved = false;
    [SerializeField] private float maxLedgeHeight = 2f;
    [SerializeField] private float ledgeCheckDistance = 0.6f;
    [SerializeField] private float ledgeFreezeTime = 0.5f;
    [SerializeField] private float playerRadius = 1f;
    private float lastLedgeGrab = 0f;

    [Header("Events")]
    [SerializeField] private UnityEvent OnLedgeClimb;

    private void Start() {
        defaultVisualScale = visualsTransform.localScale;
    }

    private void FixedUpdate() {

        if (movementControlDisabled || Time.time < lastLedgeGrab) {
            this.rb.velocity = Vector3.zero;
            Debug.Log("Can't move.", this);
            return;
        }

        move();

        //if (!movementControlDisabled || Time.time > lastLedgeGrab) {
        //    move();
        //}

        // Groundcheck
        isGrounded = false;

        RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, Vector2.down, groundCheckRaycastDistance, groundLayerMask);
        foreach (RaycastHit2D hit in hits) {
            if (hit.collider.gameObject != gameObject) {
                //Debug.Log(hit.collider.gameObject.name, hit.collider.gameObject);
                isGrounded = true;
                moved = false;
                break;
            }
        }

        // Ledge stuff
        if (!isGrounded) {
            checkLedge();
        }


        rb.AddForce(Vector2.down * gravityScaleDrop * rb.mass);

        UpdateVisuals();
    }

    private void UpdateVisuals() {
        if (visualsTransform == null) {
            return;
        }

        if (movementInput.x > 0) {
            visualsTransform.localScale = defaultVisualScale;
        } else if (movementInput.x < 0) {
            visualsTransform.localScale = Vector3.Scale(defaultVisualScale, new Vector3(-1,1,1));
        }
    }

    private void checkLedge() {
        //direction the player is facing horizontally
        Vector2 direction = Vector2.right * Mathf.Sign(movementInput.x);

        //raycast forwards to check if we hit a ledge
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, ledgeCheckDistance, groundLayerMask);

        // if we didn't hit anything, return
        if (hit.collider == null) {
            return;
        }

        //move slightly into the ledge and up
        var circlePosition = new Vector2(hit.point.x + (playerRadius * Mathf.Sign(movementInput.x)), hit.point.y + maxLedgeHeight);

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
        this.transform.position = new Vector2(downHit.point.x, downHit.point.y + playerRadius);
        lastLedgeGrab = Time.time + ledgeFreezeTime;
        OnLedgeClimb?.Invoke();

    }

    private void move() {
        float targetSpeed = movementInput.x * (isGrounded ? maxSpeed : maxAirSpeed);
        float speedDifference = targetSpeed - rb.velocity.x;
        float accelerationRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : deceleration;
        float movement = Mathf.Pow(Mathf.Abs(speedDifference) * accelerationRate, accelerationCurve.Evaluate(Mathf.Abs(speedDifference) * accelerationRate)) * Mathf.Sign(speedDifference);

        rb.AddForce(Vector2.right * movement * forceScale.x * rb.mass);

    }

    public void DoMove(InputAction.CallbackContext context) {
        movementInput = context.ReadValue<Vector2>();
    }


    public void DoJump(InputAction.CallbackContext context) {
        if (context.performed && isGrounded) {
            //Debug.LogWarning("Jump");
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }

    }

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
                Gizmos.DrawLine(transform.position + Vector3.right * ledgeCheckDistance, transform.position + Vector3.right * ledgeCheckDistance + Vector3.up * maxLedgeHeight);
                //draw a circle at the ledge check position
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(transform.position + Vector3.right * ledgeCheckDistance + Vector3.up * maxLedgeHeight, playerRadius);
            }
        }
        else {
            //if selected
            if (Selection.activeGameObject == gameObject) {
                //draw a line in the direction the player is facing
                Gizmos.color = Color.red;
                Gizmos.DrawLine(transform.position, transform.position + (Vector3.right * Mathf.Sign(movementInput.x) * ledgeCheckDistance));
                //draw a line downwards from the ledge check position
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(transform.position + Vector3.right * ledgeCheckDistance, transform.position + Vector3.right * ledgeCheckDistance + Vector3.up * maxLedgeHeight);
                //draw a circle at the ledge check position
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(transform.position + Vector3.right * ledgeCheckDistance + Vector3.up * maxLedgeHeight, playerRadius);
            }
        }

    }
}