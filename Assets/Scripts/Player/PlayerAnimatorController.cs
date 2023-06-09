using UnityEngine;

public class PlayerAnimatorController : MonoBehaviour {
    [SerializeField] private Animator animator;
    [SerializeField] private BetterPlayerMovement playerMovementController;

    // Start is called before the first frame update
    void Start() {
        if (animator == null) {
            animator = GetComponent<Animator>();
        }

        if (playerMovementController == null) {
            playerMovementController = GetComponent<BetterPlayerMovement>();
        }
    }

    // Update is called once per frame
    void Update() {
        animator.SetFloat("xSpeed", Mathf.Abs(playerMovementController.m_Rigidbody2D.velocity.x));
        animator.SetFloat("ySpeed", playerMovementController.m_Rigidbody2D.velocity.y);
        animator.SetBool("Grounded", playerMovementController.isGrounded);

    }
}