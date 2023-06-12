using System.Net;
using UnityEngine;
using UnityEditor;
using UnityEngine.U2D;
using Unity.VisualScripting;

[CustomEditor(typeof(RopeController))]
public class RopeControllerEditor : Editor {
    bool editRopeMode = false;
    bool simulateRope = false;
    bool showRopeCreator = false;

    RopeController ropeTarget;

    RopeController.CreatorConfiguration currentConfig;

    //rope editor
    RopePart selectedRopePart;
    bool draggingRopePart = false;

    Sprite lockedSprite = null;
    Sprite unlockedSprite = null;


    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        //begin change check
        EditorGUI.BeginChangeCheck();
        Undo.RecordObject(ropeTarget, "Rope config change");
        RopeController ropeController = ropeTarget;

        bool hasRope = ropeController.RopeParts.Count > 0;
        if (hasRope) {
            //edit rope mode button
            if (editRopeMode) {
                if (GUILayout.Button("Exit edit rope mode")) {
                    editRopeMode = false;
                    Tools.current = Tool.Move;
                }
            } else {
                if (GUILayout.Button("Edit rope mode")) {
                    editRopeMode = true;
                    showRopeCreator = false;
                    Tools.current = Tool.None;
                }
                //simulate rope bool
            }

            simulateRope = EditorGUILayout.Toggle("Simulate rope", simulateRope);

            if (GUILayout.Button("Clear Rope")) {
                ropeController.ClearRope();
            }
        } else {
            showRopeCreator = EditorGUILayout.Foldout(showRopeCreator, "Rope creator") &&
                              ropeController.RopeParts.Count == 0;
            if (showRopeCreator) {
                //indent
                EditorGUI.indentLevel++;

                currentConfig.ropeLength = EditorGUILayout.FloatField("Rope length", currentConfig.ropeLength);

                GUI.enabled = currentConfig.ropeSpriteType != RopeController.RopeSpriteType.Skinned;
                currentConfig.ropePartCount = EditorGUILayout.IntField("Rope part count", currentConfig.ropePartCount);
                GUI.enabled = true;

                currentConfig.initialRopeCurveAngle =
                    EditorGUILayout.FloatField("Initial Rope Angle", currentConfig.initialRopeCurveAngle);

                currentConfig.ropeSpriteType =
                    (RopeController.RopeSpriteType)EditorGUILayout.EnumPopup("Rope sprite type",
                        currentConfig.ropeSpriteType);
                EditorGUI.indentLevel++;
                switch (currentConfig.ropeSpriteType) {
                    case RopeController.RopeSpriteType.None:
                        break;
                    case RopeController.RopeSpriteType.Segmented:
                        currentConfig.ropeSprite = (Sprite)EditorGUILayout.ObjectField("Rope sprite",
                            currentConfig.ropeSprite, typeof(Sprite), false);
                        currentConfig.SpriteSize =
                            EditorGUILayout.Vector2Field("Sprite scale", currentConfig.SpriteSize);
                        currentConfig.SpriteOffset =
                            EditorGUILayout.Vector2Field("Sprite offset", currentConfig.SpriteOffset);
                        break;
                    case RopeController.RopeSpriteType.Skinned:
                        currentConfig.ropeSprite = (Sprite)EditorGUILayout.ObjectField("Rope sprite",
                            currentConfig.ropeSprite, typeof(Sprite), false);
                        if (currentConfig.ropeSprite != null)
                            currentConfig.ropePartCount = currentConfig.ropeSprite.GetBones().Length;
                        break;
                }

                EditorGUI.indentLevel--;

                if (GUILayout.Button("Create rope")) {
                    CreateRope();
                }

                EditorGUI.indentLevel--;
            }
        }

        //if changes, update scene view
        if (EditorGUI.EndChangeCheck()) {
            //draw sceneview again
            SceneView.RepaintAll();
        }
    }

    private void CreateRope() {
        //check if rope already created
        if (ropeTarget.RopeParts.Count > 0) {
            Debug.LogWarning("Rope already created");
            return;
        }

        //create rope
        ropeTarget.ClearRope();

        GameObject previousPart = ropeTarget.gameObject;

        Vector2 partPosition = ropeTarget.transform.position;
        float partAngle = ropeTarget.transform.rotation.eulerAngles.z;
        float partLength = currentConfig.ropeLength / (currentConfig.ropePartCount - 1);

        Vector2 localBoxCenter = Vector2.zero, localBoxSize = Vector2.one;
        //sprite stuff
        if (currentConfig.ropeSprite != null) {
            var spriteBounds = currentConfig.ropeSprite.bounds;
            localBoxCenter = (Vector2)spriteBounds.center + currentConfig.SpriteOffset;
            localBoxSize = (Vector2)spriteBounds.size * currentConfig.SpriteSize;
        }

        RopePart lastPart = null;

        //create all rope parts
        for (int i = 0; i < currentConfig.ropePartCount; i++) {
            RopePart ropePart = new GameObject("Rope part " + i).AddComponent<RopePart>();
            ropePart.transform.parent = previousPart.transform;
            ropePart.transform.position = partPosition;
            ropePart.transform.rotation = Quaternion.Euler(0, 0, partAngle - currentConfig.initialRopeCurveAngle);

            var cc = ropePart.AddComponent<CircleCollider2D>();
            if (i < currentConfig.ropePartCount - 1)
                ropePart.joint = ropePart.AddComponent<HingeJoint2D>();
            ropePart.rigidBody = ropePart.GetComponent<Rigidbody2D>();
            ropePart.Root = ropeTarget;
            cc.isTrigger = true;
            ropePart.tag = "Rope";
            if (lastPart != null) {
                lastPart.joint.connectedBody = ropePart.rigidBody;
            }

            cc.radius = partLength / 3f;

            ropeTarget.RopeParts.Add(ropePart);

            Vector2 add = (Quaternion.Euler(0, 0, partAngle) * Vector2.down) * partLength;
            partPosition += add;
            partAngle += currentConfig.initialRopeCurveAngle;
            previousPart = ropePart.gameObject;
            lastPart = ropePart;

            //sprite stuff
            if (currentConfig.ropeSprite != null &&
                currentConfig.ropeSpriteType == RopeController.RopeSpriteType.Segmented && i > 0) {
                var spriteObject = new GameObject("Sprite");
                spriteObject.transform.parent = ropePart.transform;
                spriteObject.transform.localPosition = localBoxCenter;
                spriteObject.transform.localRotation = Quaternion.identity;
                spriteObject.transform.localScale = Vector3.one;
                var spriteRenderer = spriteObject.AddComponent<SpriteRenderer>();
                spriteRenderer.sprite = currentConfig.ropeSprite;
                spriteRenderer.drawMode = SpriteDrawMode.Sliced;
                spriteRenderer.size = localBoxSize * 2;
            }
        }

        ropeTarget.UpdateRopeParts();
        ropeTarget.RopeEnabled = true;
    }

    //on scene gizmo
    private void OnSceneGUI() {
        if (showRopeCreator) {
            //Creator shows spheres at each (potential) rope part, and lines between them.
            //When sprites are enabled:
            //Segmented sprites: show bounding box of each sprite
            Vector2 partPosition = ropeTarget.transform.position;
            float partAngle = ropeTarget.transform.rotation.eulerAngles.z;
            float partLength = currentConfig.ropeLength / (currentConfig.ropePartCount - 1);


            Vector2 localBoxCenter = Vector2.zero, localBoxSize = Vector2.zero;
            //sprite stuff
            if (currentConfig.ropeSprite != null) {
                var spriteBounds = currentConfig.ropeSprite.bounds;
                localBoxCenter = (Vector2)spriteBounds.center + currentConfig.SpriteOffset;
                localBoxSize = (Vector2)spriteBounds.size * currentConfig.SpriteSize;
            }

            for (int i = 0; i < currentConfig.ropePartCount; i++) {
                Handles.color = Color.white;
                //first draw part gizmo, then update variables
                Handles.DrawWireDisc(partPosition, Vector3.forward, partLength / 3f);

                Handles.color = Color.green;
                if (currentConfig.ropeSpriteType == RopeController.RopeSpriteType.Segmented &&
                    currentConfig.ropeSprite != null && i > 0) {
                    //draw bounding box of potential sprite
                    Quaternion rotationOffset = Quaternion.Euler(0, 0, partAngle - currentConfig.initialRopeCurveAngle);
                    Vector2 center = partPosition + (Vector2)(rotationOffset * localBoxCenter);
                    Vector3 topRight = center + (Vector2)(rotationOffset * (localBoxSize * new Vector2(1, 1)));
                    Vector3 botRight = center + (Vector2)(rotationOffset * (localBoxSize * new Vector2(1, -1)));
                    Vector3 botLeft = center + (Vector2)(rotationOffset * (localBoxSize * new Vector2(-1, -1)));
                    Vector3 topLeft = center + (Vector2)(rotationOffset * (localBoxSize * new Vector2(-1, 1)));

                    Handles.DrawLines(new Vector3[] {
                        topRight, topLeft, topLeft, botLeft, botLeft, botRight, botRight, topRight
                    });
                }

                Handles.color = Color.white;
                Vector2 prevPosition = partPosition;
                Vector2 add = (Quaternion.Euler(0, 0, partAngle) * Vector2.down) * partLength;
                partPosition += add;
                partAngle += currentConfig.initialRopeCurveAngle;

                if (i < currentConfig.ropePartCount - 1)
                    Handles.DrawLine(prevPosition, partPosition);
            }
        }

        Debug.Log("Editing rope.");
        if (editRopeMode) {
            Vector3 mousePosition = Event.current.mousePosition;
            Ray ray = HandleUtility.GUIPointToWorldRay(mousePosition);
            mousePosition = ray.GetPoint(0);
            mousePosition.z = 0;

            //find the rope part closest to the mouse
            var ropePart = ropeTarget.getClosestRopePart(mousePosition);
            if (ropePart != null) {
                if (ropePart.isAnchored) {
                    Vector3 ropePartPosition =
                        Handles.PositionHandle(ropePart.joint.transform.position, Quaternion.identity);
                    ropePart.joint.transform.position = ropePartPosition;
                }

                //draw lock/unlock button with sprites
                Handles.BeginGUI();


                if (ropePart.isAnchored) {
                    if (GUI.Button(
                            new Rect(HandleUtility.WorldToGUIPoint(ropePart.transform.position) + new Vector2(-20, 20),
                                new Vector2(60, 20)), "Unlock")) {
                        ropePart.isAnchored = false;
                        DestroyImmediate(ropePart.joint.gameObject);
                    }
                } else {
                    if (GUI.Button(
                            new Rect(HandleUtility.WorldToGUIPoint(ropePart.transform.position) + new Vector2(-20, 20),
                                new Vector2(40, 20)), "Lock")) {
                        //create rope anchor on rope position
                        var anchor = new GameObject(ropePart.gameObject.name + " Anchor");
                        anchor.transform.parent = ropeTarget.transform;
                        anchor.transform.position = ropePart.transform.position;

                        //create joint
                        var joint = anchor.AddComponent<HingeJoint2D>();
                        joint.autoConfigureConnectedAnchor = false;
                        joint.connectedBody = ropePart.rigidBody;
                        joint.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
                        joint.anchor = Vector2.zero;
                        joint.connectedAnchor = Vector2.zero;


                        ropePart.joint = joint;
                        ropePart.isAnchored = true;
                    }
                }

                Handles.EndGUI();
            }

            //loop over each rope part and draw a green circle if unlocked, red if locked
            foreach (RopePart part in ropeTarget.RopeParts) {
                if (part.isAnchored) {
                    Handles.color = Color.red;
                } else {
                    Handles.color = Color.green;
                }

                Handles.DrawWireDisc(part.transform.position, Vector3.forward, 0.1f);
            }
        }
    }

    private void Update() {
        if (!simulateRope) return;

        Physics2D.simulationMode = SimulationMode2D.Script;
        Physics2D.Simulate(Mathf.Min(Time.deltaTime, 0.1f));
        Physics2D.simulationMode = SimulationMode2D.FixedUpdate;
    }

    private void OnDisable() {
        ropeTarget = null;
        currentConfig = null;

        editRopeMode = false;
        Tools.current = Tool.Move;
        EditorApplication.update -= Update;
    }

    private void OnEnable() {
        ropeTarget = (RopeController)target;
        currentConfig = ropeTarget.creatorConfiguration;

        EditorApplication.update += Update;

        editRopeMode = false;
        Tools.current = Tool.Move;


        lockedSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Editor/Locked.png");
        unlockedSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Editor/Unlocked.png");
    }
}