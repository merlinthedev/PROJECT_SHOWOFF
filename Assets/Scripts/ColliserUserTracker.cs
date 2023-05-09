using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliserUserTracker : MonoBehaviour
{
    [SerializeField] private Animator cameraSwitcher;
    [SerializeField] private int colliderIndex = 0;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Object entered with tag: " + other.gameObject.tag);
        if (other.gameObject.tag == "Player")
        {
            cameraSwitcher.SetInteger("CameraIndex", colliderIndex);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.tag == "Player")
        {
            cameraSwitcher.SetInteger("CameraIndex", 0);
        }
    }
}
