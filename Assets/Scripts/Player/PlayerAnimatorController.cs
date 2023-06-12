using UnityEngine;

public class PlayerAnimatorController : MonoBehaviour {
    [SerializeField] private Animator animator;
    [SerializeField] private BetterPlayerMovement playerMovementController;
    private int animationJumpTrigger;
    private int animationGroundedTrigger;

    
    
    
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
    }

    // Update is called once per frame
    void Update() {
        animator.SetFloat("xSpeed", Mathf.Abs(playerMovementController.m_Rigidbody2D.velocity.x));
        animator.SetFloat("ySpeed", playerMovementController.m_Rigidbody2D.velocity.y);
        animator.SetBool("Grounded", playerMovementController.IsGrounded);
        
    }

    public void Jump() {
        animator.SetTrigger(animationJumpTrigger);
    }
    public void Ground() {
        animator.SetTrigger(animationGroundedTrigger);
    }
}