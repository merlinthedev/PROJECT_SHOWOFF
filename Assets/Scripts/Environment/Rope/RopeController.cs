using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.U2D;

public class RopeController : MonoBehaviour {
    [SerializeField] List<RopePart> ropeParts = new();
    public List<RopePart> RopeParts { get { return ropeParts; } }

    public float RopeLength { get; private set; }

    [SerializeField] float ropeDamping = 1f;
    [HideInInspector] float ropeStiffness = 0f;
    [SerializeField] float climbSpeedMultiplier = 1f;
    public float ClimbSpeedMultiplier { get => climbSpeedMultiplier; }
    [SerializeField] bool startEnabled = true;

    public bool RopeEnabled {
        get {
            if (ropeParts == null) return false;
            foreach (var part in ropeParts) {
                if (part.GetComponent<Rigidbody2D>().bodyType == RigidbodyType2D.Dynamic) return true;
            }

            return false;
        }
        set {
            if (ropeParts == null) return;
            if (ropeParts.Count == 0) return;
            foreach (var part in ropeParts) {
                part.GetComponent<Rigidbody2D>().bodyType = value ? RigidbodyType2D.Dynamic : RigidbodyType2D.Kinematic;
            }

            ropeParts[0].GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
        }
    }


    private void Start() {
        RopeEnabled = startEnabled;

        // calculate the length of the rope
        for (int i = 0; i < ropeParts.Count - 1; i++) {
            RopeLength += Vector2.Distance(ropeParts[i].transform.position, ropeParts[i + 1].transform.position);
        }
    }

    public Vector2 GetRopePoint(float ropeProgress) {
        //returns the point along the rope curve at the given progress (0 to 1)
        //we assume a constant length between rope parts
        int ropePartCount = ropeParts.Count;
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

        for (int i = 0; i < ropeParts.Count - 1; i++) {
            var ropePart = ropeParts[i];
            var nextRopePart = ropeParts[i + 1];
            var partVector = ((Vector2)(nextRopePart.transform.position) - (Vector2)(ropePart.transform.position));
            var relPosition = point - (Vector2)ropePart.transform.position;
            float posDot = Mathf.Clamp01(Vector2.Dot(partVector.normalized, relPosition) / partVector.magnitude);

            Vector2 closestPartPoint =
                Vector2.Lerp(ropePart.transform.position, nextRopePart.transform.position, posDot);

            if (Vector2.Distance(point, closestPartPoint) < Vector2.Distance(point, closestPoint)) {
                closestPartProgress = i + posDot;
                closestPoint = closestPartPoint;
            }
        }

        float progress = closestPartProgress / (ropeParts.Count - 1);
        progress = Mathf.Clamp01(progress);
        return progress;
    }

    public RopePart GetRopePart(float ropeProgress) {
        if (ropeParts.Count == 0) return null;
        //returns the point along the rope curve at the given progress (0 to 1)
        //we assume a constant length between rope parts
        int ropePartCount = ropeParts.Count;
        //make sure float is within bounds
        ropeProgress = Mathf.Clamp01(ropeProgress) * (ropePartCount - 1);
        //at which rope part are we
        int ropeIndex = Mathf.FloorToInt(ropeProgress);
        //find the rope part we are below
        if (ropeIndex < ropeParts.Count - 1)
            return ropeParts[ropeIndex + 1];
        else
            return RopeParts[ropeIndex];
    }

    public RopePart GetRopePart(Vector2 point) {
        return GetRopePart(GetRopeProgress(point));
    }

    public RopePart getClosestRopePart(Vector2 point) {
        RopePart closestPart = null;
        float closestDistance = float.PositiveInfinity;

        foreach (var part in ropeParts) {
            float distance = Vector2.Distance(point, part.transform.position);
            if (distance < closestDistance) {
                closestDistance = distance;
                closestPart = part;
            }
        }

        return closestPart;
    }

    public int GetPartIndex(GameObject part) {
        for (int i = 0; i < ropeParts.Count; i++) {
            if (ropeParts[i].gameObject == part) return i;
        }

        return -1;
    }

    public void LockRopePart(RopePart part) {
        //check part is in our chain
        if (!ropeParts.Contains(part)) return;
        //check part is not already locked
        if (part.isAnchored) return;
        //check part is not the first part
        if (part == ropeParts[0]) return;

        //create rope anchor on rope position
        var anchor = new GameObject(part.gameObject.name + " Anchor");
        anchor.transform.parent = part.transform;
        anchor.transform.position = part.transform.position;

        //create joint
        var joint = anchor.AddComponent<HingeJoint2D>();
        joint.autoConfigureConnectedAnchor = false;
        joint.connectedBody = part.rigidBody;
        joint.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
        joint.anchor = Vector2.zero;
        joint.connectedAnchor = Vector2.zero;


        part.joint = joint;
        part.isAnchored = true;
    }

    public void UnlockRopePart(RopePart part) {
        //check part is in our chain
        if (!ropeParts.Contains(part)) return;
        //check part is not already unlocked
        if (!part.isAnchored) return;

        //destroy joint

        // Q: Can I check whether I should use Destroy()  or DestroyImmediate()?
        // A: DestroyImmediate() is used in the editor, Destroy() is used in the game
        if (Application.isPlaying) {
            Destroy(part.joint.gameObject);
        } else {
            DestroyImmediate(part.joint.gameObject);
        }

        part.joint = null;
        part.isAnchored = false;
    }

#if UNITY_EDITOR

    public void ClearRope() {
        //clear the list of RopeParts
        ropeParts = new();
        //delete all children gameObjects
        while (transform.childCount > 0) {
            DestroyImmediate(transform.GetChild(0).gameObject);
        }
    }

    public void UpdateRopeParts() {
        //loop over all rope parts and apply the damping to the linearDamping on the RigidBody2D
        foreach (RopePart part in ropeParts) {
            part.SetDamping(ropeDamping);
            part.SetStiffness(ropeStiffness);
        }
    }

    private void OnValidate() {
        RopeEnabled = startEnabled;
        UpdateRopeParts();
    }

    //on attach to gameobject
    private void Reset() {
        ClearRope();
    }

    public enum RopeSpriteType {
        None,
        Segmented,
        Skinned
    }

    [System.Serializable]
    public class CreatorConfiguration {
        //rope creator
        public float ropeLength = 1f;
        public int ropePartCount = 2;
        public RopeSpriteType ropeSpriteType = RopeSpriteType.None;
        public Sprite ropeSprite = null;
        public Sprite ropeStartSprite = null;
        public Sprite ropeEndSprite = null;
        public float initialRopeCurveAngle = 0f;
        public Vector2 SpriteSize = Vector2.one;
        public Vector2 SpriteOffset = Vector2.zero;
    }

    [HideInInspector] public CreatorConfiguration creatorConfiguration = new CreatorConfiguration();

#endif
}