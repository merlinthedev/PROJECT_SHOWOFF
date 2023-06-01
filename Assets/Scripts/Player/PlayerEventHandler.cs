using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerEventHandler : MonoBehaviour {
    [SerializeField] private LayerMask _objectLayer;
    [SerializeField] private Transform objectGrabPointTranform;
    [SerializeField] private HingeJoint2D grabAnchor;

    private GameObject nearObject;
    private RaycastHit2D nearObjectHit;

    private int empty = 0;
    private bool grabbing = false;

    public bool Grabbing {
        get => grabbing;
    }

    // Update is called once per frame
    void FixedUpdate() {
        if (empty >= 4) {
            nearObject = null;
            empty = 0;
        }

        //up, down, left, right directions
        Vector2[] raycastDirection = { Vector2.up, Vector2.up, Vector2.left, Vector2.right };
        for (int i = 0; i < 4; i++) {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, raycastDirection[i], 1f, _objectLayer);
            if (hit.collider != null) {
                nearObject = hit.collider.gameObject;
                nearObjectHit = hit;
            } else {
                empty++;
            }
        }
    }

    public void OnGrab(InputAction.CallbackContext callbackContext) {
        return;
        if (callbackContext.started) {
            grabbing = true;
            if (this.nearObject != null) {
                grabAnchor.anchor = grabAnchor.transform.InverseTransformPoint(nearObjectHit.point);
                grabAnchor.connectedBody = nearObject.GetComponent<Rigidbody2D>();
                grabAnchor.enabled = true;
            }

            Debug.Log("Started grabbing context");
        }

        if (callbackContext.canceled) {
            grabbing = false;
            grabAnchor.connectedBody = null;
            grabAnchor.enabled = false;

            Debug.Log("Stopped grabbing context");
        }
    }

    public void Death() {
        gameObject.transform.position = GameObject.FindGameObjectsWithTag("Respawn")[0].transform.position;
    }
}