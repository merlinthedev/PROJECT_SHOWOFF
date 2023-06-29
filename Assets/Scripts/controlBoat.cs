using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class controlBoat : MonoBehaviour {
    [SerializeField] private GameObject boat;
    [SerializeField] private GameObject riseTarget;
    private Transform tr;
    public bool risen = false;

    public float rightSpeed;
    public float upSpeed;
    public float downSpeed;
    public float slightDownSpeed;

    public enum State { Rising, Up, Down, Slightdown, Idle }
    public State currentState;
    //make states

    private void FixedUpdate() {
        if (risen) {
            MoveBoat();
        }
        switch (currentState) {
            case State.Rising:
                GoUp();
                break;
            case State.Up:
                GoUp();
                break;
            case State.Down:
                GoDown();
                break;
            case State.Slightdown:
                GoSlightDown();
                break;
            case State.Idle:
                break;

        }
    }
    private void Start() {
        tr = boat.GetComponent<Transform>();
    }

    public void SwitchRising() {
        risen = true;
    }
    public void MoveBoat() {
        tr.position += new Vector3(rightSpeed, 0, 0);
    }

    public void GoUp() {
        Debug.Log("Going up");
        tr.position += new Vector3(0, upSpeed, 0);
    }

    public void GoDown() {
        tr.position -= new Vector3(0, downSpeed, 0);
    }

    public void GoSlightDown() {
        tr.position -= new Vector3(0, slightDownSpeed, 0);
    }

    public void SetState(State state) {
        currentState = state;
    }
}
