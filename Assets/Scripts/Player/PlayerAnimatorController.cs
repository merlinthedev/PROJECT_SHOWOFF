using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerAnimatorController : MonoBehaviour {
    [SerializeField] private Animator animator;
    [SerializeField] private PlayerController playerMovementController;

    // Start is called before the first frame update
    void Start() {
        if (animator == null) {
            animator = GetComponent<Animator>();
        }

        if (playerMovementController == null) {
            playerMovementController = GetComponent<PlayerController>();
        }
    }

    // Update is called once per frame
    void Update() {
        animator.SetFloat("xSpeed", Mathf.Abs(playerMovementController.velocity.x));
        animator.SetFloat("ySpeed", playerMovementController.velocity.y);
        animator.SetBool("Grounded", playerMovementController.Grounded);

    }
}