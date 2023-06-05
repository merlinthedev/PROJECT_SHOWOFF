using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour {
    [SerializeField] private float movementSpeed = 5f;
    
    private Vector2 movementInput;
    private Vector3 oldPosition = Vector3.zero;
    
    private void FixedUpdate() {
        
        
        float x = this.movementInput.x;
        
        
    }


    public void OnMove(InputAction.CallbackContext context) {
        this.movementInput = context.ReadValue<Vector2>();
    }
}