using UnityEngine;

public class Toad : SwampMonster {
    [Header("ACTIVATION")] [SerializeField]
    private float raycastDistance = 5f;

    [SerializeField] private float activationCooldown = 6f;

    [Header("MOVEMENT")] [SerializeField] private Rigidbody2D m_Rigidbody2D;
    [SerializeField] private float movementSpeed = 5f;
    [SerializeField] private float groundAcceleration = 20f;

    [Header("BOUNDS")] [SerializeField] private Transform leftBound;
    [SerializeField] private Transform rightBound;
    private Vector2 initialPosition;

    private bool shouldMove = false;
    private float lastActivationTime = 0f;
    private float moveTime = 2f;


    private void FixedUpdate() {
        if (Time.time > lastActivationTime + moveTime) {
            if (shouldMove) {
                shouldMove = false;
                monsterAnimator.SetTrigger("Walk");
                Debug.Log("Setting trigger to walk.");
                direction = 0f;
            }
        }

        if (Time.time > lastActivationTime + activationCooldown) {
            raycasting();
        }

        if (shouldMove) {
            Debug.Log("Should move.");
            move();

            if (!outOfBounds) {
                boundsCheck();
            } else {
                // check when we go back in bounds
                if (transform.position.x > leftBound.position.x && transform.position.x < rightBound.position.x) {
                    outOfBounds = false;
                }
            }
        }
    }

    [SerializeField] private bool outOfBounds = false;
    [SerializeField] private float direction = 0f;
    [SerializeField] private LayerMask playerMask;

    private void move() {
        if (direction == 0) return;

        float desiredVelocity = direction * movementSpeed;
        float velocityGap = desiredVelocity - m_Rigidbody2D.velocity.x;

        float acceleration = groundAcceleration;
        float accelerationThisFrame = acceleration * Time.fixedDeltaTime;
        float accelerationSign = Mathf.Sign(velocityGap);
        float accelerationMagnitude = Mathf.Min(Mathf.Abs(velocityGap), accelerationThisFrame);

        Vector2 accelerationVector = new() { x = accelerationMagnitude * accelerationSign, y = 0 };
        GetComponent<SpriteRenderer>().flipX = direction < 0;

        m_Rigidbody2D.AddForce(accelerationVector, ForceMode2D.Impulse);
    }

    private void boundsCheck() {
        outOfBounds = transform.position.x < leftBound.position.x || transform.position.x > rightBound.position.x;

        if (outOfBounds) {
            // flip the direction
            direction *= -1;
        }
    }

    private void raycasting() {
        var left = Physics2D.Raycast(transform.position, Vector3.left, raycastDistance, playerMask);
        var right = Physics2D.Raycast(transform.position, Vector3.right, raycastDistance, playerMask);

        Debug.DrawRay(transform.position, Vector3.left * raycastDistance, Color.red);
        Debug.DrawRay(transform.position, Vector3.right * raycastDistance, Color.red);

        if (left || right) {
            Debug.Log("Raycast hit.");
            if (!shouldMove) {
                shouldMove = true;
                monsterAnimator.SetTrigger("Run");
                Debug.Log("Setting trigger to Run.");
                lastActivationTime = Time.time;

                direction = left ? 1f : -1f;
            }
        }
    }
}