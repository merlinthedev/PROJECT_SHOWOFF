using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.U2D;

public class RopeController : MonoBehaviour {
    [SerializeField] Transform ropeRoot;
    public Transform RopeRoot { get { return ropeRoot; } }
    [SerializeField] GameObject[] ropeParts;

    [SerializeField] float ropeDamping = 0f;
    [SerializeField] bool startEnabled = true;

    public bool RopeEnabled {
        get {
            foreach (var part in ropeParts) {
                if (part.GetComponent<Rigidbody2D>().simulated) return true;
            }
            return false;
        }
        set {
            foreach (var part in ropeParts) {
                part.GetComponent<Rigidbody2D>().simulated = value;
            }
        }
    }

    private void Start() {
        RopeEnabled = startEnabled;
    }

    public Vector2 GetRopePoint(float ropeProgress) {
        //returns the point along the rope curve at the given progress (0 to 1)
        //we assume a constant length between rope parts
        int ropePartCount = ropeParts.Length;
        //make sure float is within bounds
        ropeProgress = Mathf.Clamp01(ropeProgress) * (ropePartCount - 1);
        //at which rope part are we
        int ropeIndex = Mathf.FloorToInt(ropeProgress);
        //how far are we along one rope part
        float partProgress = ropeProgress % 1;
        //find the rope part we are below
        var ropePart = ropeParts[ropeIndex];

        //if we are at the end of the rope
        if (ropeIndex >= ropePartCount - 1) {
            return ropePart.transform.position;
        }
        var nextRopePart = ropeParts[ropeIndex + 1];
        return Vector2.Lerp(ropePart.transform.position, nextRopePart.transform.position, partProgress);
    }

    public float GetRopeProgress(Vector2 point) {
        //return the progress along the rope with the given position

        float closestPartProgress = 0;
        Vector2 closestPoint = Vector2.positiveInfinity;

        for (int i = 0; i < ropeParts.Length - 1; i++) {
            var ropePart = ropeParts[i];
            var nextRopePart = ropeParts[i + 1];
            var partVector = ((Vector2)(nextRopePart.transform.position) - (Vector2)(ropePart.transform.position));
            var relPosition = point - (Vector2)ropePart.transform.position;
            float posDot = Mathf.Clamp01(Vector2.Dot(partVector.normalized, relPosition) / partVector.magnitude);

            Vector2 closestPartPoint = Vector2.Lerp(ropePart.transform.position, nextRopePart.transform.position, posDot);

            if (Vector2.Distance(point, closestPartPoint) < Vector2.Distance(point, closestPoint)) {
                closestPartProgress = i + posDot;
                closestPoint = closestPartPoint;
            }
        }

        float progress = closestPartProgress / (ropeParts.Length - 1);
        progress = Mathf.Clamp01(progress);
        return progress;
    }

    public Rigidbody2D GetRopePart(float ropeProgress) {
        //returns the point along the rope curve at the given progress (0 to 1)
        //we assume a constant length between rope parts
        int ropePartCount = ropeParts.Length;
        //make sure float is within bounds
        ropeProgress = Mathf.Clamp01(ropeProgress) * (ropePartCount - 1);
        //at which rope part are we
        int ropeIndex = Mathf.FloorToInt(ropeProgress);
        //find the rope part we are below
        var ropePart = ropeParts[ropeIndex];

        return ropePart.GetComponent<Rigidbody2D>();
    }

#if UNITY_EDITOR
    public void FindRope() {
        List<GameObject> newRopeParts = new List<GameObject>();
        GameObject current = ropeRoot.gameObject;
        newRopeParts.Add(current);
        //go to parts child untill there is none or the first child is marked to be ignored
        while (current.transform.childCount > 0) {
            current = current.transform.GetChild(0).gameObject;
            if (current.name.StartsWith("[NOROPE]")) break;
            newRopeParts.Add(current);
        }

        ropeParts = newRopeParts.ToArray();
    }

    public void SetupRope() {
        //go through all rope parts, ensure there is a RigidBody2D and a HingeJoint2D on there
        //next link the hinge joint to the part's child

        for (int i = 0; i < ropeParts.Length - 1; i++) {
            GameObject part = ropeParts[i];
            GameObject child = ropeParts[i + 1];

            Rigidbody2D rb = part.GetComponent<Rigidbody2D>();
            if (rb == null) {
                rb = part.AddComponent<Rigidbody2D>();
            }

            HingeJoint2D hj = part.GetComponent<HingeJoint2D>();
            if (hj == null) {
                hj = part.AddComponent<HingeJoint2D>();
            }

            Rigidbody2D rbChild = child.GetComponent<Rigidbody2D>();
            if (rbChild == null) {
                rbChild = child.AddComponent<Rigidbody2D>();
            }

            hj.connectedBody = rbChild;
        }

        //remove the hinge joint from the last part
        HingeJoint2D lastHJ = ropeParts[ropeParts.Length - 1].GetComponent<HingeJoint2D>();
        if (lastHJ != null) {
            DestroyImmediate(lastHJ);
        }
    }

    public void ClearRope() {
        //clear the list of RopeParts
        ropeParts = new GameObject[0];
    }

    public void UpdateRopeParts() {
        //loop over all rope parts and apply the damping to the linearDamping on the RigidBody2D
        foreach (GameObject part in ropeParts) {
            Rigidbody2D rb = part.GetComponent<Rigidbody2D>();
            if (rb != null) {
                rb.drag = ropeDamping;
            }
        }
    }

    private void OnValidate() {
        UpdateRopeParts();
    }
#endif
}
