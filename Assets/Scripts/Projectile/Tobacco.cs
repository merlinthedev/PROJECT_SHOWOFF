using EventBus;
using System;
using UnityEngine;


public class Tobacco : AProjectile, IPickup {

    private void OnEnable() {
        EventBus<VodyanoyLocationEvent>.Subscribe(onVodyanoyLocationReceived);
    }

    private void OnDisable() {
        EventBus<VodyanoyLocationEvent>.Unsubscribe(onVodyanoyLocationReceived);
    }

    private void onVodyanoyLocationReceived(VodyanoyLocationEvent e) {
        Vector2 direction = e.location - transform.position;
        direction.Normalize();
        float force = 30f;

        Debug.Log("Shooting at vod");

        base.Shoot(direction, force);
    }

    public void OnPickup(Player player) {
        transform.SetParent(player.transform);
        transform.position = player.GetPlayerProjectileController().GetHoldingTransform().position;

        EventBus<TobaccoPickupEvent>.Raise(new TobaccoPickupEvent());
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


    public override void Shoot(Vector2 direction, float force) {
        Debug.Log("Shooting the override function");
        EventBus<TobaccoThrowEvent>.Raise(new TobaccoThrowEvent());
    }

}