using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BranchScript : MonoBehaviour
{
    //check for collision with player rock
    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.gameObject.name == "stone") {
            //destroy branch
            Debug.Log("bonk");
        }
    }
}
