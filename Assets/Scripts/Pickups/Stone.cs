using UnityEngine;

public class Stone : AProjectile, IPickup {
    public void OnPickup(Player player) {
        this.transform.SetParent(player.transform);
        this.transform.position = player.GetHoldingTransform().position;
    }

    public void OnDrop() {
        throw new System.NotImplementedException();
    }

    public void OnThrow(Player player) {
        this.transform.SetParent(null);
        var newCollider2D = this.gameObject.GetComponent<Collider2D>();

        // Exclude player layer from the collider
        var x = LayerMask.NameToLayer("Player");
        newCollider2D.excludeLayers = 1 << x;
        newCollider2D.isTrigger = false;

        var newRigidbody2D = this.gameObject.GetComponent<Rigidbody2D>();

        if (newRigidbody2D == null) {
            Debug.Log("Rigidbody was null", this);
            return;
        }
        
        newRigidbody2D.bodyType = RigidbodyType2D.Dynamic;
        
        player.ResetProjectile();

        Utils.Instance.InvokeDelayed(2.5f, () => {
            // reset exlude layers
            newCollider2D.excludeLayers = 0;
            newCollider2D.isTrigger = true;
            newRigidbody2D.bodyType = RigidbodyType2D.Kinematic;
        });
        
    }

    public void ToString() {
        Debug.Log("Picked up a stone");
    }
}