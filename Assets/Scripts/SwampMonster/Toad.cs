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
            }
        }

        if (shouldMove) {
            move();
        }
    }

    private void boundsCheck() {
    }

    private float direction = 0f;

    private void move() {
        if (direction == 0) return;
        float desiredVelocity = direction * movementSpeed;
        float velocityGap = desiredVelocity - m_Rigidbody2D.velocity.x;

        float acceleration = groundAcceleration;
        float accelerationThisFrame = acceleration * Time.fixedDeltaTime;
        float accelerationSign = Mathf.Sign(velocityGap);
        float accelerationMagnitude = Mathf.Min(Mathf.Abs(velocityGap), accelerationThisFrame);

        Vector2 accelerationVector = new() { x = accelerationMagnitude * accelerationSign, y = 0 };

        m_Rigidbody2D.AddForce(accelerationVector, ForceMode2D.Impulse);
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.GetComponent<IReactor>() == null) {
            return;
        }

        shouldMove = true;
        lastActivationTime = Time.time;
        monsterAnimator.SetTrigger("Run");

        var pointOfImpact = other.ClosestPoint(transform.position);
        // get what side the pointOfImpact is on compared to our transform
        var signedAngle = Vector2.SignedAngle(Vector2.up, pointOfImpact - (Vector2)transform.position);

        direction = Mathf.Sign(signedAngle);

        // flip sprite
        GetComponent<SpriteRenderer>().flipX = direction < 0;
    }
}