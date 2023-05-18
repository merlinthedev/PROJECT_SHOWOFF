using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Unity.VisualScripting;

[CustomEditor(typeof(RopeController))]
public class RopeControllerEditor : Editor
{
    bool showRopeCreator = false;
    
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();

        RopeController ropeController = (RopeController)target;

        GUILayout.BeginHorizontal();
        if(GUILayout.Button("Find Rope")) {
            ropeController.FindRope();
        }

        if(GUILayout.Button("Setup Rope")) {
            ropeController.SetupRope();
        }

        if(GUILayout.Button("Clear Rope")) {
            ropeController.ClearRope();
        }
        GUILayout.EndHorizontal();

        showRopeCreator = EditorGUILayout.Foldout(showRopeCreator, "Rope creator");
        if(showRopeCreator) {
            //indent
            EditorGUI.indentLevel++;
            if (ropeController.RopeRoot == null) {
                EditorGUILayout.HelpBox("No rope root set", MessageType.Error);
            } else {
                EditorGUILayout.HelpBox("Still have to do this part \\o/", MessageType.Info);
            }
            EditorGUI.indentLevel--;
        }
    }
}
