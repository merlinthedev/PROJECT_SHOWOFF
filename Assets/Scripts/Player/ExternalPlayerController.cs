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
        EventBus<NewSceneTriggeredEvent>.Subscribe(nextSceneMovement);
    }

    private void OnDisable() {
        EventBus<VodyanoyFinishedWalkingEvent>.Unsubscribe(eventBasedMovement);
        EventBus<NewSceneTriggeredEvent>.Unsubscribe(nextSceneMovement);
    }

    private void Start() {
        if (shouldIntro) {
            Debug.Log("Introduction cutscene started");
            Utils.Instance.InvokeDelayed(0.5f, () => move(destinations[0].gameObject.transform.position));
            currentDestination = destinations[1];
        } else {
            currentDestination = destinations[1];
        }
    }

    private void move(Vector3 destination) {
        player.GetPlayerController().externalLocomotion(destination, enableMovement);
    }

    private void enableMovement() {
        player.GetPlayerController().canMove = true;
    }

    private void eventBasedMovement(VodyanoyFinishedWalkingEvent e) {
        EventBus<NextJumpIsCutsceneEvent>.Raise(new NextJumpIsCutsceneEvent {
            destination = currentDestination,
        });
    }

    private void nextSceneMovement(NewSceneTriggeredEvent e) {
        move(destinations[destinations.Count - 1].gameObject.transform.position);
    }
}