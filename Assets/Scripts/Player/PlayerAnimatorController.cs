using UnityEngine;
using UnityEngine.Events;

public class PlayerAnimatorController : MonoBehaviour {
    [SerializeField] private Animator animator;
    [SerializeField] private BetterPlayerMovement playerMovementController;
    private int animationJumpTrigger;
    private int animationGroundedTrigger;
    private int animationClimbTrigger;
    private int animationThrowTrigger;
    private int animationPickupTrigger;
    private int animationLedgeClimbTrigger;
    [SerializeField] private UnityEvent OnLand;
    [SerializeField] private float minJumpDuration = 0.3f;


    // Start is called before the first frame update
    void Start() {
        if (animator == null) {
            animator = GetComponent<Animator>();
        }

        if (playerMovementController == null) {
            playerMovementController = GetComponent<BetterPlayerMovement>();
        }

        animationJumpTrigger = Animator.StringToHash("Jump");
        animationGroundedTrigger = Animator.StringToHash("Grounded");
        animationClimbTrigger = Animator.StringToHash("OnClimb");
        animationThrowTrigger = Animator.StringToHash("Throw");
        animationPickupTrigger = Animator.StringToHash("Pickup");
        animationLedgeClimbTrigger = Animator.StringToHash("LedgeClimb");
    }

    // Update is called once per frame
    void Update() {
        if (playerMovementController.canMove) {
            animator.SetFloat("xSpeed", Mathf.Abs(playerMovementController.m_Rigidbody2D.velocity.x));
        }

        animator.SetFloat("ySpeed", playerMovementController.m_Rigidbody2D.velocity.y);
        animator.SetBool("Grounded", playerMovementController.IsGrounded);
        animator.SetFloat("moveInputY", playerMovementController.MoveInput.y);
        animator.SetBool("RopeClimb", playerMovementController.IsClimbing);

        // Change the animator speed based on the player's speed
        // animator.speed = Mathf.Abs(playerMovementController.m_Rigidbody2D.velocity.x) / 2;
        //Q: how do I check whether the animation transition has finished
        //A: use the normalized time of the animation
        // animator.GetCurrentAnimatorStateInfo(0).normalizedTime
    }

    public void playAnimation() {
        // loop the animation for the given time
        animator.SetFloat("xSpeed", 1);
        
    }

    public void ResetSpeed() {
        animator.speed = 1f;
    }

    public void LedgeClimb() {
        animator.SetTrigger(animationLedgeClimbTrigger);
    }

    public void RopeClimb() {
        animator.SetTrigger(animationClimbTrigger);
    }

    public void Jump() {
        animator.SetTrigger(animationJumpTrigger);
    }

    public void Throw() {
        animator.SetTrigger(animationThrowTrigger);
    }

    public void Pickup() {
        animator.SetTrigger(animationPickupTrigger);
    }

    public void Ground(float jumpDuration = 0) {
        animator.SetTrigger(animationGroundedTrigger);
        if (jumpDuration < minJumpDuration) return;
        OnLand?.Invoke();
    }
}