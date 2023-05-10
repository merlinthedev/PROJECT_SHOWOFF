using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class PlayerController : MonoBehaviour {
    [SerializeField] private PlayerInput input;
    [SerializeField] private Rigidbody2D rb;

    [SerializeField]
    [Range(0, 10)]
    private float speed = 5;

    private Vector2 moveInput;

    // Start is called before the first frame update
    void Start() {
        //input = GetComponent<PlayerInput>();
        //rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update() {
        rb.AddForce(moveInput * speed);
    }

    public void OnMove(InputAction.CallbackContext context) {
        moveInput = context.ReadValue<Vector2>();
    }
}
