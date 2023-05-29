using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerAnimatorController : MonoBehaviour
{
    [SerializeField] Animator animator;
    [SerializeField] Rigidbody2D rb;
    [FormerlySerializedAs("playerController")] [SerializeField] PlayerMovementController playerMovementController;

    // Start is called before the first frame update
    void Start()
    {
        if(animator == null) {
            animator = GetComponent<Animator>();
        }

        if (rb == null) {
            rb = GetComponent<Rigidbody2D>();
        }

        if (playerMovementController == null) {
            playerMovementController = GetComponent<PlayerMovementController>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        animator.SetFloat("xSpeed", Mathf.Abs(rb.velocity.x));
        animator.SetFloat("ySpeed", rb.velocity.y);
        animator.SetBool("Grounded", playerMovementController.IsGrounded());
        
    }
}
