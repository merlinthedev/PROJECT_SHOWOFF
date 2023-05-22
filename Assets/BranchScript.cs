using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BranchScript : MonoBehaviour
{
    [SerializeField] private bool isParent = false;
    public bool gotBonked = false;

    private void Update() {

        if(isParent)
        { 
            for (int i = 0; i < transform.childCount; i++) 
            {
                if (transform.GetChild(i).GetComponent<BranchScript>().gotBonked == true)
                    Destroy();
            }
        }
    }
    public void Destroy() {
        for (int i = 0; i < transform.childCount; i++) {
            transform.GetChild(i).GetComponent<FixedJoint2D>().enabled = false;
        }

    }
    private void OnCollisionEnter2D(Collision2D collision) {
        if(collision.gameObject.name == ("stone")) {
            gotBonked=true;
        }
    }
}
