using UnityEngine;
using UnityEditor;
using UnityEngine.U2D;
using Unity.VisualScripting;

[CustomEditor(typeof(RopeController))]
public class RopeControllerEditor : Editor {
    bool editRopeMode = false;
    bool showRopeCreator = false;

    RopeController ropeTarget;
    
    RopeController.CreatorConfiguration currentConfig;
    
    //rope editor
    HingeJoint2D anchor;
    RopePart selectedRopePart;
    bool draggingRopePart = false;
    
    

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
            }

            if (GUILayout.Button("Clear Rope")) {
                ropeController.ClearRope();
            }

        } else {

            showRopeCreator = EditorGUILayout.Foldout(showRopeCreator, "Rope creator") && ropeController.RopeParts.Count == 0;
            if (showRopeCreator) {
                //indent
                EditorGUI.indentLevel++;

                currentConfig.ropeLength = EditorGUILayout.FloatField("Rope length", currentConfig.ropeLength);

                GUI.enabled = currentConfig.ropeSpriteType != RopeController.RopeSpriteType.Skinned;
                currentConfig.ropePartCount = EditorGUILayout.IntField("Rope part count", currentConfig.ropePartCount);
                GUI.enabled = true;

                currentConfig.initialRopeCurveAngle = EditorGUILayout.FloatField("Initial Rope Angle", currentConfig.initialRopeCurveAngle);

                currentConfig.ropeSpriteType = (RopeController.RopeSpriteType)EditorGUILayout.EnumPopup("Rope sprite type", currentConfig.ropeSpriteType);
                EditorGUI.indentLevel++;
                switch (currentConfig.ropeSpriteType) {
                    case RopeController.RopeSpriteType.None:
                        break;
                    case RopeController.RopeSpriteType.Segmented:
                        currentConfig.ropeSprite = (Sprite)EditorGUILayout.ObjectField("Rope sprite", currentConfig.ropeSprite, typeof(Sprite), false);
                        currentConfig.SpriteSize = EditorGUILayout.Vector2Field("Sprite scale", currentConfig.SpriteSize);
                        currentConfig.SpriteOffset = EditorGUILayout.Vector2Field("Sprite offset", currentConfig.SpriteOffset);
                        break;
                    case RopeController.RopeSpriteType.Skinned:
                        currentConfig.ropeSprite = (Sprite)EditorGUILayout.ObjectField("Rope sprite", currentConfig.ropeSprite, typeof(Sprite), false);
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
        if(EditorGUI.EndChangeCheck()) {
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
            ropePart.transform.rotation = Quaternion.Euler(0, 0, partAngle);

            var cc = ropePart.AddComponent<CircleCollider2D>();
            ropePart.joint = ropePart.AddComponent<HingeJoint2D>();
            ropePart.rigidBody = ropePart.GetComponent<Rigidbody2D>();
            ropePart.Root = ropeTarget;
            if (lastPart != null) {
                lastPart.joint.connectedBody = ropePart.rigidBody;
            }

            Vector2 add = (Quaternion.Euler(0, 0, partAngle) * Vector2.down) * partLength;
            partPosition += add;
            partAngle += currentConfig.initialRopeCurveAngle;
            previousPart = ropePart.gameObject;
            lastPart = ropePart;
        }
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
            if(currentConfig.ropeSprite != null) {
                var spriteBounds = currentConfig.ropeSprite.bounds;
                localBoxCenter = (Vector2)spriteBounds.center + currentConfig.SpriteOffset;
                localBoxSize = (Vector2)spriteBounds.size * currentConfig.SpriteSize;
            }
            for(int i = 0; i < currentConfig.ropePartCount; i++) {
                Handles.color = Color.white;
                //first draw part gizmo, then update variables
                Handles.DrawWireDisc(partPosition, Vector3.forward, partLength / 3f);
                
                Handles.color = Color.green;
                if(currentConfig.ropeSpriteType == RopeController.RopeSpriteType.Segmented && currentConfig.ropeSprite != null) {
                    //draw bounding box of potential sprite
                    Quaternion rotationOffset = Quaternion.Euler(0, 0, partAngle);
                    Vector2 center = partPosition + (Vector2)(rotationOffset * localBoxCenter);
                    Vector3 topRight = center + (Vector2)(rotationOffset * (localBoxSize * new Vector2(1, 1)));
                    Vector3 botRight = center + (Vector2)(rotationOffset * (localBoxSize * new Vector2(1, -1)));
                    Vector3 botLeft = center + (Vector2)(rotationOffset * (localBoxSize * new Vector2(-1, -1)));
                    Vector3 topLeft = center + (Vector2)(rotationOffset * (localBoxSize * new Vector2(-1, 1)));

                    Handles.DrawLines(new Vector3[] { topRight, topLeft, topLeft, botLeft, botLeft, botRight, botRight, topRight});
                    
                }
                
                Handles.color = Color.white;
                Vector2 prevPosition = partPosition;
                Vector2 add = (Quaternion.Euler(0, 0, partAngle) * Vector2.down) * partLength;
                partPosition += add;
                partAngle += currentConfig.initialRopeCurveAngle;

                Handles.DrawLine(prevPosition, partPosition);

            }
        }

        if (editRopeMode) {
            Vector3 mousePosition = Event.current.mousePosition;
            Ray ray = HandleUtility.GUIPointToWorldRay(mousePosition);
            mousePosition = ray.GetPoint(0);
            mousePosition.z = 0;

            //find the rope part closest to the mouse
            var ropePart = ropeTarget.getClosestRopePart(mousePosition);
            if (ropePart != null) {
                //begin change check
                EditorGUI.BeginChangeCheck();
                Vector3 ropePartPosition = Handles.PositionHandle(ropePart.transform.position, Quaternion.identity);

                draggingRopePart = false;
                selectedRopePart = null;

                if (EditorGUI.EndChangeCheck()) {
                    draggingRopePart = true;
                    selectedRopePart = ropePart;
                    //find the index of our edited rope part
                    int ropePartIndex = ropeTarget.GetPartIndex(ropePart.gameObject);
                    for (int i = 0; i < ropeTarget.RopeParts.Count; i++) {
                        var part = ropeTarget.RopeParts[i];
                        Undo.RecordObject(part.transform, "Move Rope");
                        if (i == ropePartIndex) { }
                        //part.transform.position = ropePartPosition;
                    }
                    anchor.transform.position = mousePosition;

                }
            }
        }
    }

    private void Update() {
        if (!editRopeMode) return;

        anchor.connectedBody = selectedRopePart.rigidBody;
        Physics2D.simulationMode = SimulationMode2D.Script;
        Physics2D.Simulate(Time.fixedDeltaTime);
        Physics2D.simulationMode = SimulationMode2D.FixedUpdate;
        anchor.connectedBody = null;
    }

    private void OnDisable() {
        ropeTarget = null;
        currentConfig = null;

        DestroyImmediate(anchor.gameObject);
        editRopeMode = false;
        Tools.current = Tool.Move;
        EditorApplication.update -= Update;
    }

    private void OnEnable() {
        ropeTarget = (RopeController)target;
        currentConfig = ropeTarget.creatorConfiguration;

        //create empty gameObject with hingeJoint
        GameObject grabAnchor = new GameObject("TempAnchor");
        anchor = grabAnchor.AddComponent<HingeJoint2D>();
        anchor.autoConfigureConnectedAnchor = false;
        EditorApplication.update += Update;

        editRopeMode = false;
        Tools.current = Tool.Move;
    }
}
