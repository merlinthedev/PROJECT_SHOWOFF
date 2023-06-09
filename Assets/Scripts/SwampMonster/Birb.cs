using FMODUnity;
using UnityEngine;

public class Birb : SwampMonster {
    [SerializeField] private Collider2D collisionTriggerCollider;
    [SerializeField] private float flyDistance = 50f;
    [SerializeField] private float flyTime = 10f;
    [SerializeField] EventReference flySound;
    private Vector3 flyDirection;

    private bool isFlying = false;

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.GetComponent<IReactor>() == null) {
            return;
        }

        if (isFlying) {
            return;
        }

        flyDirection = -(other.transform.position - transform.position).normalized;

        isFlying = true;

        // Get the direction to fly
        
        RuntimeManager.PlayOneShot(flySound, transform.position);
        monsterAnimator.SetTrigger("GoFly");

        var signedAngle = Mathf.Clamp(Vector2.SignedAngle(Vector2.up, flyDirection), -45f, 45f);

        flyDirection = Quaternion.Euler(0, 0, signedAngle) * Vector2.up;

        var spriteRenderer = GetComponent<SpriteRenderer>();

        // flip the sprite basic on the direction
        if (!spriteRenderer.flipX && signedAngle > 0) {
            spriteRenderer.flipX = true;
        } else if (spriteRenderer.flipX && signedAngle < 0) {
            spriteRenderer.flipX = false;
        }


        Debug.Log("Birb is flying away + " + signedAngle);

        // move to the target
        moveTowardsTarget();
    }

    private void moveTowardsTarget() {
        // Tween to the target
        var target = transform.position + flyDirection * flyDistance;
        LeanTween.move(gameObject, target, flyTime).setEase(LeanTweenType.easeInOutQuad).setOnComplete(() => {
            Destroy(gameObject);
        });
    }

    public void OnDrawGizmosSelected() {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position, collisionTriggerCollider.bounds.size * 1.01f);
    }
}