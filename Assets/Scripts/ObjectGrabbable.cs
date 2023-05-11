using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectGrabbable : MonoBehaviour
{
    private Rigidbody2D objectRigidbody;
    private Transform objectGrabpointTransform;
    [SerializeField] HingeJoint2D joint;
    [SerializeField] Rigidbody2D playerRb;
    private void Awake() 
    {
        objectRigidbody = GetComponent<Rigidbody2D>();
        joint.enabled = false;
    }
    public void Grab()
    {
        //this.objectGrabpointTransform = objectGrabPointTransform;
        joint.enabled = true;
        joint.connectedBody = playerRb;
    }
    public void Drop()
    {

        joint.connectedBody = null;
        joint.enabled = false;
    }

    private void FixedUpdate()
    {
        if(objectGrabpointTransform != null)
        {
            objectRigidbody.MovePosition(objectGrabpointTransform.position);
            //objectRigidbody.useGravity = false;
        }
    }
}
