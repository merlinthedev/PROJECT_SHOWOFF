using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimatorController : MonoBehaviour
{
    [SerializeField] Animator animator;
    [SerializeField] Rigidbody2D rb;
    [SerializeField] PlayerController playerController;

    // Start is called before the first frame update
    void Start()
    {
        if(animator == null) {
            animator = GetComponent<Animator>();
        }

        if (rb == null) {
            rb = GetComponent<Rigidbody2D>();
        }

        if (playerController == null) {
            playerController = GetComponent<PlayerController>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        animator.SetFloat("xSpeed", Mathf.Abs(rb.velocity.x));
        animator.SetFloat("ySpeed", rb.velocity.y);
        animator.SetBool("Grounded", playerController.isGrounded);
        
    }
}
