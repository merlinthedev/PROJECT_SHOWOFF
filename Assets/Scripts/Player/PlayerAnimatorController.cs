using UnityEngine;

public class PlayerAnimatorController : MonoBehaviour {
    [SerializeField] private Animator animator;
    [SerializeField] private BetterPlayerMovement playerMovementController;
    private int animationJumpTrigger;
    private int animationGroundedTrigger;
    private int animationClimbTrigger;


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
    }

    // Update is called once per frame
    void Update() {
        animator.SetFloat("xSpeed", Mathf.Abs(playerMovementController.m_Rigidbody2D.velocity.x));
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

    public void ResetSpeed() {
        animator.speed = 1f;
    }

    public void RopeClimb() {
        animator.SetTrigger(animationClimbTrigger);
    }

    public void Jump() {
        animator.SetTrigger(animationJumpTrigger);
    }

    public void Ground() {
        animator.SetTrigger(animationGroundedTrigger);
    }
}