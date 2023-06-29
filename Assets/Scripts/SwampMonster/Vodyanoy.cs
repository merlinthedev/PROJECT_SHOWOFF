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

    [SerializeField] private GameObject visualReference;
    [SerializeField] private Animator animator;

    private int stopWalkTriggerHash;

    //When the object is enabled, it will move to the riseTarget
    private void OnEnable() {
        EventBus<VodyanoyAwakeEvent>.Subscribe(vodyanoyMove);
        EventBus<TobaccoThrowEvent>.Subscribe(onTobaccoThrow);
    }

    private void OnDisable() {
        EventBus<VodyanoyAwakeEvent>.Unsubscribe(vodyanoyMove);
        EventBus<TobaccoThrowEvent>.Unsubscribe(onTobaccoThrow);


    }

    private void onTobaccoThrow(TobaccoThrowEvent e) {
        EventBus<VodyanoyLocationEvent>.Raise(new VodyanoyLocationEvent {
            location = this.gameObject.transform.position
        });
    }

    private void Start() {
        stopWalkTriggerHash = Animator.StringToHash("StopWalk");
    }

    private void vodyanoyMove(VodyanoyAwakeEvent e) {
        visualReference.SetActive(true);
        Move();
    }

    //Move the object to the riseTarget
    private void Move() {

        LeanTween.move(gameObject, targets[0].transform.position, moveDuration).setEase(LeanTweenType.easeInOutSine)
            .setOnComplete(EnablePlayerJump);
    }

    private void EnablePlayerJump() {
        Debug.Log("EnablePlayerJump");
        EventBus<VodyanoyFinishedWalkingEvent>.Raise(new VodyanoyFinishedWalkingEvent());

        animator.SetTrigger(stopWalkTriggerHash);

        player.noJumpAllowed = false;
    }
}