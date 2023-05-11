using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderCameraTrigger : MonoBehaviour
{
    [SerializeField] private Cinemachine.CinemachineVirtualCameraBase camera;
    [SerializeField] private int cameraPriority = 5;

    // Start is called before the first frame update
    void Start()
    {
        if (camera == null)
            camera = GetComponent<Cinemachine.CinemachineVirtualCameraBase>();
        
        camera.Priority = -1;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        //Debug.Log("Object entered with tag: " + other.gameObject.tag);
        if (other.gameObject.tag == "Player")
        {
            camera.Priority = cameraPriority;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.tag == "Player")
        {
            camera.Priority = -1;
        }
    }
}
