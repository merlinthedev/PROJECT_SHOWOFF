using UnityEngine;

public abstract class AProjectile : MonoBehaviour {
    // Shoot method
    public virtual void Shoot(Vector2 direction, float force) {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();


        if (rb == null) {
            Debug.LogError("Projectile has no rigidbody");

            return;
        }

        Debug.Log("Shooting AProjectile");

        rb.AddForce(direction * force, ForceMode2D.Impulse);
    }


}