using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class LogCheese : MonoBehaviour {
    [SerializeField] private GameObject logObject;
    [SerializeField] private InputAction logCheeseAction;
    [SerializeField] private Vector3 initialLogPosition;
    [SerializeField] private Quaternion initialLogRotation;


    private void Start() {
        logCheeseAction.Enable();
        logCheeseAction.performed += ctx => logCheese();
        
        initialLogPosition = logObject.transform.position;
        initialLogRotation = logObject.transform.rotation;
    }
    

    private void logCheese() {
        logObject.transform.position = initialLogPosition;
        logObject.transform.rotation = initialLogRotation;
    }


}