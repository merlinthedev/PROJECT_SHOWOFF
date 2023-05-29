using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class RopePart : MonoBehaviour
{
    public Rigidbody2D rb;
    public HingeJoint2D joint;
    public bool isAnchored = false;
    public bool isLast = false;
    public float ropeProgress;
    public RopeController ropeController;
    public int ropePartIndex;
    public float ropeLength;
    public float ropeDamping;
    public float ropeStiffness;
    public bool startEnabled;
}
