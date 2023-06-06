using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class Vodyanoy : MonoBehaviour {

    [Header ("movespeed")]
    public float riseDuration;
    public float moveDuration;

    [Header ("targets")]
    public GameObject riseTarget;
    public GameObject[] targets;

    public FixedJoint2D fj;
    public GameObject boat;
    public PlayerController player;

    //When the object is enabled, it will move to the riseTarget
    private void OnEnable() {
        Move();
    }

    //Check if the object has reached the riseTarget
    private void FixedUpdate() {
        CheckPos();
    }

    //Move the object to the riseTarget
    private void Move() {
        LeanTween.move(gameObject, riseTarget.transform.position, riseDuration).setEase(LeanTweenType.easeInOutSine);
    }

    //Check if the object has reached the riseTarget. If it has, initiate SplineMove()
    private void CheckPos() {
        if(transform.position == riseTarget.transform.position) {
            SplineMove();
        }
    }

    //Move the object along the spline.
    private void SplineMove() {
        //Define spline
        Vector3[] newTargets = new Vector3[targets.Length];
        for (int i = 0; i < targets.Length; i++) {
            newTargets[i] = targets[i].transform.position;
        }
        LTSpline ltSpline = new LTSpline(newTargets);

        //Move along spline
        var x = LeanTween.moveSpline(gameObject, ltSpline, moveDuration).setEase(LeanTweenType.easeInOutQuad);
        x.setOnComplete(() => {
            EnablePlayerJump();
        });
    }
    
    private void EnablePlayerJump() {
        Debug.Log("EnablePlayerJump");
        // player.gameObject.GetComponent<PlayerMovementController>().canJump = true;
        // player.gameObject.GetComponent<PlayerMovementController>().travelling = false;

        this.player.setCanJump(true);
        this.player.setTraveling(false);
    }
    public void Connect() {
        fj.connectedBody = boat.GetComponent<Rigidbody2D>();
    }
}