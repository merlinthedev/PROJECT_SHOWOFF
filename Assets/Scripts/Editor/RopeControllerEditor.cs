using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RopeController))]
public class RopeControllerEditor : Editor
{
    
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
    }
}
