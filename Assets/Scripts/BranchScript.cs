using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BranchScript : MonoBehaviour
{
    [SerializeField] private bool isParent = false;
    public bool gotBonked = false;
    public bool fade = false;

    private void FixedUpdate() {

        // If this is a parent branch, check if any of the children have been bonked
        if(isParent)
        {
            //try to get a rigidbody2d component
            if(TryGetComponent<Rigidbody2D>(out Rigidbody2D rb)) rb.bodyType = RigidbodyType2D.Kinematic;
            for (int i = 0; i < transform.childCount; i++) 
            {
                if (transform.GetChild(i).GetComponent<BranchScript>().gotBonked) Destroy();
            }
        }
        //if fade is true, fade out the branch and then destroy it
        if(fade) 
        {
            Color tmp = GetComponent<SpriteRenderer>().color;
            if(tmp.a > 0) tmp.a -= 0.009f;
            else if (tmp.a < 0) tmp.a = 0;
            GetComponent<SpriteRenderer>().color = tmp;
            //destroy the branch once it is fully faded out
            if(tmp.a == 0) Destroy(gameObject);
            Debug.Log(tmp.a);
        }
    }
    //destroy all the fixed joints and fade out each individual the branch
    public void Destroy() {
        for (int i = 0; i < transform.childCount; i++) 
        {
            transform.GetChild(i).GetComponent<FixedJoint2D>().enabled = false;
            transform.GetChild(i).GetComponent<BranchScript>().fade = true;
        }

    }
    //if the branch collides with a stone, set gotBonked to true
    private void OnCollisionEnter2D(Collision2D collision) {
        if (collision.gameObject.name == ("stone(Clone)")) gotBonked = true;
    }
}
