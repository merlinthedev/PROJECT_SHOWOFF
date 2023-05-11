using UnityEngine;
using UnityEngine.Events;

public class Lever : MonoBehaviour {
    public bool isOpen = false;
    public bool IsOpen { get { return isOpen; } }
    private Rigidbody2D rb;

    [SerializeField] private bool defaultState = false;

    //private System.Action<bool> onStateChange = null;
    // Unity Event
    [SerializeField] private UnityEvent<bool> onStateChange = null;

    private void Start() {
        rb = GetComponent<Rigidbody2D>();
        rb.AddForce(Vector2.right * (defaultState ? 1 : -1), ForceMode2D.Impulse);
    }

    private void Update() {
        if (transform.hasChanged) {
            var angle = transform.rotation.eulerAngles.z;
            var newIsOpen = angle > 180;

            if (newIsOpen != isOpen) {
                isOpen = newIsOpen;
                onStateChange?.Invoke(isOpen);
            }
            //Debug.Log(angle, this);
        }
    }

}
