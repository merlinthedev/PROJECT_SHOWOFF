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

    public bool shouldIdleOnStart = false;

    private int stopWalkTriggerHash;
    private int startBlowTriggerHash;
    private int startIdleHash;

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
        startBlowTriggerHash = Animator.StringToHash("StartBlow");
        startIdleHash = Animator.StringToHash("StartIdle");

        if(shouldIdleOnStart) {
            animator.SetTrigger(startIdleHash);
        }
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
        player.canMove = true;
        player.inBoat = false;
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.gameObject.GetComponent<Tobacco>() != null) {
            animator.SetTrigger(startBlowTriggerHash);
        }
    }
}