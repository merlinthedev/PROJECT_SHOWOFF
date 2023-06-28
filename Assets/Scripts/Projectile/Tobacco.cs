using UnityEngine;


public class Tobacco : AProjectile, IPickup {

    public void OnPickup(Player player) {
        transform.SetParent(player.transform);
        transform.position = player.GetPlayerProjectileController().GetHoldingTransform().position;
    }

    public void OnDrop() {
        throw new System.NotImplementedException();
    }

    public void OnThrow(Player player) {
        transform.SetParent(null);
        var x = LayerMask.NameToLayer("Player");
        var y = GetComponent<Collider2D>();
        y.excludeLayers = 1 << x;

        Debug.Log("Throwing tobacco!");

        var z = GetComponent<Rigidbody2D>();
        z.bodyType = RigidbodyType2D.Dynamic;

        player.GetPlayerProjectileController().ResetProjectile();

    }


}