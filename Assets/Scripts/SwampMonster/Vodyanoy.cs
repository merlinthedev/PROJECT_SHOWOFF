using EventBus;
using UnityEngine;

public class Vodyanoy : MonoBehaviour {
    [Header("movespeed")] public float riseDuration;

    public float moveDuration;

    [Header("targets")] public GameObject riseTarget;

    public GameObject[] targets;

    public FixedJoint2D fj;
    public GameObject boat;
    public BetterPlayerMovement player;

    //When the object is enabled, it will move to the riseTarget
    private void OnEnable() {
        Move();
    }

    //Move the object to the riseTarget
    private void Move() {
        
        LeanTween.move(gameObject, targets[0].transform.position, moveDuration).setEase(LeanTweenType.easeInOutSine)
            .setOnComplete(EnablePlayerJump);
    }

    private void EnablePlayerJump() {
        Debug.Log("EnablePlayerJump");

        player.noJumpAllowed = false;
    }
}