using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("")]
[RequireComponent(typeof(Rigidbody2D))]
public class RopePart : MonoBehaviour
{
    public RopeController Root;
    public Rigidbody2D rigidBody;
    public HingeJoint2D joint;
    public bool isAnchored = false;
    public Joint2D Anchor;
    public float ropeProgress;
    
    public int ropePartIndex;

    public void SetStiffness(float stiffness) {
        if(stiffness == 0) {
            joint.useLimits = false;
        } else {
            joint.useLimits = true;
            float angleLimit = 90 * ((Mathf.Cos(Mathf.Clamp01(stiffness) * Mathf.PI)) + 1);
            joint.limits = new JointAngleLimits2D() { min = -angleLimit, max = angleLimit };
        }
    }

    public void SetDamping(float damping) {
        rigidBody.drag = damping;
    }
}
