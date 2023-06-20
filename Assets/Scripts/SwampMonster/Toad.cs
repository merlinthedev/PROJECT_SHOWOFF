using System;
using UnityEngine;

public class Toad : SwampMonster {
    [Header("ACTIVATION BOX")] [SerializeField]
    private Collider2D collisionTriggerCollider;

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

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.GetComponent<IReactor>() == null) {
            return;
        }

        shouldMove = true;
        lastActivationTime = Time.time;
        monsterAnimator.SetTrigger("Run");
        Debug.Log("Setting trigger to run.");

        var pointOfImpact = other.ClosestPoint(transform.position);
        // get what side the pointOfImpact is on compared to our transform
        var signedAngle = Vector2.SignedAngle(Vector2.up, pointOfImpact - (Vector2)transform.position);

        direction = Mathf.Sign(signedAngle);
    }
}