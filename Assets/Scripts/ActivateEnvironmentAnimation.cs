using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivateEnvironmentAnimation : MonoBehaviour
{
    [SerializeField] private GameObject parent;
    [SerializeField] private GameObject travelTo;
    [SerializeField] private float travelTime = 1f;
    public void Activate() {
        Debug.Log("gonna do stuff");
        parent.GetComponent<Animator>().SetTrigger("Activate");
        LeanTween.move(parent, travelTo.transform.position, travelTime).setEaseOutBounce();
    }
}
