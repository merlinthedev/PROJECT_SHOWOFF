using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestMove : MonoBehaviour
{
    public GameObject[] targets;
    // Start is called before the first frame update
    void Start()
    {
        Vector3[] newTargets = new Vector3[targets.Length];
        for (int i = 0; i < targets.Length; i++) {
            newTargets[i] = targets[i].transform.position;
        }

        LTSpline ltSpline = new LTSpline(newTargets);

        LeanTween.moveSpline(gameObject, ltSpline, 4.0f).setEase(LeanTweenType.easeInOutQuad); // animate 
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
