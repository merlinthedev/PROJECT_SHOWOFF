using UnityEngine;

public class Stone : AProjectile, IPickup {
    public void OnPickup(Player player) {
        this.transform.SetParent(player.transform);
        this.gameObject.SetActive(false);
    }

    public void OnDrop() {
        throw new System.NotImplementedException();
    }

    public void OnThrow() {
        this.gameObject.SetActive(true);
        this.transform.SetParent(null);
        var newCollider2D = this.gameObject.GetComponent<Collider2D>();

        // Exclude player layer from the collider
        var x = LayerMask.NameToLayer("Player");
        newCollider2D.excludeLayers = 1 << x;

        var newRigidbody2D = this.gameObject.AddComponent<Rigidbody2D>();
        newCollider2D.isTrigger = false;
    }

    public void ToString() {
        Debug.Log("Picked up a stone");
    }
}