using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

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

    [Header("Jump")]
    public bool isGrounded = false;
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private float groundCheckRaycastDistance = 0.5f;

    private Ledge ledge;
    private bool moved = false;
    private void FixedUpdate() {

        if (!movementControlDisabled) {
            move();
        }

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

        ledge = null;

        // Gravity
        if (ledge == null) {
            rb.AddForce(Vector2.down * gravityScaleDrop * rb.mass);
        }

    }

    private void adjustPlayerPosition(Vector2 targetPosition) {
        if (!moved) {
            Debug.LogWarning("Moving");
            moved = true;

            // move player to the edge of the ledge

            transform.position = targetPosition;

            Debug.LogWarning("Moving done");

            this.rb.bodyType = RigidbodyType2D.Dynamic;

        }
    }

    private void checkLedge() {
        Debug.LogWarning("Checking Ledge");
        if (transform.localScale.x > 0) {
            // shoot raycast to the right to check for ledges
            RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, Vector2.right, 0.7f, groundLayerMask);
            foreach (RaycastHit2D hit in hits) {
                if (hit.collider.gameObject != gameObject) {
                    Debug.Log(hit.collider.gameObject.name, hit.collider.gameObject);
                    if (hit.collider.gameObject.GetComponent<Ledge>() != null) {
                        Debug.LogWarning("Found Ledge");

                        this.rb.bodyType = RigidbodyType2D.Kinematic;
                        this.rb.velocity = Vector2.zero;

                        // TODO: Move to ledge
                        this.ledge = hit.collider.gameObject.GetComponent<Ledge>();
                        var targetPosition = new Vector2(hit.point.x,
                            // top of the box collider + half of our height
                            hit.collider.gameObject.transform.position.y + hit.collider.gameObject.GetComponent<BoxCollider2D>().size.y / 2 + col.bounds.extents.y
                            );

                        adjustPlayerPosition(targetPosition);

                    }
                    break;
                }
            }
        } else {
            // shoot raycast to the left to check for ledges
            RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, Vector2.left, 0.7f, groundLayerMask);
            foreach (RaycastHit2D hit in hits) {
                if (hit.collider.gameObject != gameObject) {
                    Debug.Log(hit.collider.gameObject.name, hit.collider.gameObject);
                    if (hit.collider.gameObject.GetComponent<Ledge>() != null) {
                        Debug.LogWarning("Found Ledge");

                        this.rb.bodyType = RigidbodyType2D.Kinematic;
                        this.rb.velocity = Vector2.zero;

                        this.ledge = hit.collider.gameObject.GetComponent<Ledge>();
                        var targetPosition = new Vector2(hit.point.x,
                            // top of the box collider + half of our height
                            hit.collider.gameObject.transform.position.y + hit.collider.gameObject.GetComponent<BoxCollider2D>().size.y / 2 + col.bounds.extents.y
                            );

                        adjustPlayerPosition(targetPosition);
                    }
                    break;
                }
            }
        }
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
        // flip the character
        if (movementInput.x > 0) {
            transform.localScale = new Vector3(1, 1, 1);
        } else if (movementInput.x < 0) {
            transform.localScale = new Vector3(-1, 1, 1);
        }
    }

    public void DoJump(InputAction.CallbackContext context) {
        if (context.performed && isGrounded) {
            //Debug.LogWarning("Jump");
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }
    }


}