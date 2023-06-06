using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trigger : MonoBehaviour
{
    public GameObject[] objectsToActivate;

    private void OnTriggerEnter2D(Collider2D collision) {
        Activate();
    }
    public void Activate() {
        foreach (GameObject obj in objectsToActivate) {
            obj.SetActive(true);
        }
    }
}
