using EventBus;
using System;
using UnityEngine;
using System.Collections.Generic;

public class ExternalPlayerController : MonoBehaviour {
    [SerializeField] private Player player;

    // [SerializeField] private GameObject destination;
    [SerializeField] private List<Transform> destinations = new();

    [SerializeField] private bool shouldIntro = false;
    private Transform currentDestination;

    private void OnEnable() {
        EventBus<VodyanoyFinishedWalkingEvent>.Subscribe(eventBasedMovement);
    }

    private void OnDisable() {
        EventBus<VodyanoyFinishedWalkingEvent>.Unsubscribe(eventBasedMovement);
    }

    private void Start() {
        if (shouldIntro) {
            Utils.Instance.InvokeDelayed(0.5f, () => Move(destinations[0].gameObject.transform.position));
        }

        currentDestination = destinations[0];
    }

    private void Move(Vector3 destination) {
        player.GetPlayerController().externalLocomotion(destination);
    }

    private void eventBasedMovement(VodyanoyFinishedWalkingEvent e) {
        EventBus<NextJumpIsCutsceneEvent>.Raise(new NextJumpIsCutsceneEvent {
            destination = currentDestination,
            callback = () => player.GetPlayerController().JumpIntoDestinationMovement(destinations[1])
        });
    }
}