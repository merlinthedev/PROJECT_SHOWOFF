using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Unity.VisualScripting;
using UnityEngine.U2D;
using System;

[CustomEditor(typeof(RopeController))]
public class RopeControllerEditor : Editor {
    bool editRopeMode = false;
    bool showRopeCreator = false;

    //rope creator
    float ropeLength = 1f;
    int ropePartCount = 2;
    float ropeDamping = 0f;
    float ropeStiffness = 0f;
    bool startEnabled = true;
    RopeSpriteType ropeSpriteType = RopeSpriteType.None;
    Sprite ropeSprite = null;


    HingeJoint2D anchor;
    Rigidbody2D selectedRopePart;
    bool draggingRopePart = false;

    public enum RopeSpriteType {
        None,
        Segmented,
        Skinned
    }

    public override void OnInspectorGUI() {
        base.OnInspectorGUI();

        RopeController ropeController = (RopeController)target;
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
                ropeLength = EditorGUILayout.FloatField("Rope length", ropeLength);

                GUI.enabled = ropeSpriteType != RopeSpriteType.Skinned;
                ropePartCount = EditorGUILayout.IntField("Rope part count", ropePartCount);
                GUI.enabled = true;

                ropeDamping = EditorGUILayout.FloatField("Rope damping", ropeDamping);
                ropeStiffness = EditorGUILayout.FloatField("Rope stiffness", ropeStiffness);
                startEnabled = EditorGUILayout.Toggle("Start enabled", startEnabled);
                ropeSpriteType = (RopeSpriteType)EditorGUILayout.EnumPopup("Rope sprite type", ropeSpriteType);
                EditorGUI.indentLevel++;
                switch (ropeSpriteType) {
                    case RopeSpriteType.None:
                        break;
                    case RopeSpriteType.Segmented:
                        ropeSprite = (Sprite)EditorGUILayout.ObjectField("Rope sprite", ropeSprite, typeof(Sprite), false);
                        break;
                    case RopeSpriteType.Skinned:
                        ropeSprite = (Sprite)EditorGUILayout.ObjectField("Rope sprite", ropeSprite, typeof(Sprite), false);
                        if (ropeSprite != null)
                            ropePartCount = ropeSprite.GetBones().Length;
                        break;
                }
                EditorGUI.indentLevel--;

                if (GUILayout.Button("Create rope")) {
                    //ropeController.CreateRope(ropeLength, ropePartCount, ropeDamping, ropeStiffness, startEnabled, ropeSpriteType, ropeSprite);
                }
                EditorGUI.indentLevel--;
            }
        }
    }

    //on scene gizmo
    private void OnSceneGUI() {
        RopeController ropeController = (RopeController)target;

        if (showRopeCreator) {

        }

        if (editRopeMode) {
            Vector3 mousePosition = Event.current.mousePosition;
            Ray ray = HandleUtility.GUIPointToWorldRay(mousePosition);
            mousePosition = ray.GetPoint(0);
            mousePosition.z = 0;

            //find the rope part closest to the mouse
            var ropePart = ropeController.getClosestRopePart(mousePosition);
            if (ropePart != null) {
                //begin change check
                EditorGUI.BeginChangeCheck();
                Vector3 ropePartPosition = Handles.PositionHandle(ropePart.transform.position, Quaternion.identity);

                draggingRopePart = false;
                selectedRopePart = null;

                if (EditorGUI.EndChangeCheck()) {
                    draggingRopePart = true;
                    selectedRopePart = ropePart.rigidBody;
                    //find the index of our edited rope part
                    int ropePartIndex = ropeController.GetPartIndex(ropePart.gameObject);
                    for (int i = 0; i < ropeController.RopeParts.Count; i++) {
                        var part = ropeController.RopeParts[i];
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

        anchor.connectedBody = selectedRopePart;
        Physics2D.simulationMode = SimulationMode2D.Script;
        Physics2D.Simulate(Time.fixedDeltaTime);
        Physics2D.simulationMode = SimulationMode2D.FixedUpdate;
        anchor.connectedBody = null;
    }

    private void OnDisable() {
        DestroyImmediate(anchor.gameObject);
        editRopeMode = false;
        Tools.current = Tool.Move;
        EditorApplication.update -= Update;
    }

    private void OnEnable() {
        //create empty gameObject with hingeJoint
        GameObject grabAnchor = new GameObject("TempAnchor");
        anchor = grabAnchor.AddComponent<HingeJoint2D>();
        anchor.autoConfigureConnectedAnchor = false;
        EditorApplication.update += Update;

        editRopeMode = false;
        Tools.current = Tool.Move;
    }
}
