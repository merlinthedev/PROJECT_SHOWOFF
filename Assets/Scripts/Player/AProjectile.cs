using UnityEngine;

public abstract class AProjectile : MonoBehaviour {
    // Shoot method
    public void Shoot(Vector2 direction, float force) {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        
        if (rb == null) {
            Debug.LogError("Projectile has no rigidbody");
            return;
        }

        rb.AddForce(direction * force, ForceMode2D.Impulse);
    }
}