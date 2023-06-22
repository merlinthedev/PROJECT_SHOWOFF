using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderCameraTrigger : MonoBehaviour
{
    [SerializeField] private Cinemachine.CinemachineVirtualCamera vcam;
    [SerializeField] private Cinemachine.CinemachineMixingCamera mixCam;

    [SerializeField] bool playerInRange = false;
    [SerializeField] float blendStart = 5f;
    [SerializeField] float blendEnd = 1f;
    
    private GameObject player;

    [SerializeField] AnimationCurve blendCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));


    private void Update() {
        if (playerInRange) {
            float distance = Vector2.Distance(transform.position, player.transform.position);
            float blendWeight = blendCurve.Evaluate(Mathf.InverseLerp(blendStart, blendEnd, distance));
            mixCam.SetWeight(vcam, blendWeight);
        } else {
            mixCam.SetWeight(vcam, 0);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "Player")
        {
            playerInRange = true;
            player = other.gameObject;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        Debug.Log("Trigger entered with tag: " + other.gameObject.tag);
        if (other.gameObject.tag == "Player")
        {
            playerInRange = false;
            player = null;
        }
    }

    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, blendStart);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, blendEnd);
    }
}
