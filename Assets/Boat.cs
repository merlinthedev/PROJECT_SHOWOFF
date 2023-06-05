using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boat : MonoBehaviour {
    public GameObject reet;

    private void FixedUpdate() {
        Debug.DrawRay(reet.transform.position, Vector3.down * 2f, Color.red);

        RaycastHit2D hit;
        hit = Physics2D.Raycast(reet.transform.position, Vector2.down, 2f);
        if (hit.collider == null) return;
        if (hit.collider.gameObject.tag == "Water") {
            Debug.Log("submerge moment");
        }
        //draw the ray for debug
    }


    // Start is called before the first frame update
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }
}
