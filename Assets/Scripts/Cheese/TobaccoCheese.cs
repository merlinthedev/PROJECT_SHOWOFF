using EventBus;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TobaccoCheese : MonoBehaviour {

    [SerializeField] private Player player;

    private void Start() {
        player.GetPlayerProjectileController().grabbing = true;
        player.GetPlayerAnimatorController().Pickup();

    }

    private void OnEnable() {
        EventBus<TobaccoPickupEvent>.Subscribe(onTobaccoPickedUp);
    }

    private void OnDisable() {
        EventBus<TobaccoPickupEvent>.Unsubscribe(onTobaccoPickedUp);

    }

    private void onTobaccoPickedUp(TobaccoPickupEvent e) {
        player.GetPlayerProjectileController().grabbing = false;
    }
}
