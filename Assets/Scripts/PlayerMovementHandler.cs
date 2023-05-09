using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovementHandler : MonoBehaviour {

    [SerializeField] private Rigidbody rb;
    [SerializeField] private float movementMultiplier = 5f;

    private Vector2 movementVector = Vector2.zero;

    private void FixedUpdate() {
        Vector3 move = new Vector3(movementVector.x, 0, movementVector.y);

        rb.AddForce(move * movementMultiplier);
    }

    // Callback context
    public void OnMove(InputAction.CallbackContext context) {
        movementVector = context.ReadValue<Vector2>();
    }
}
