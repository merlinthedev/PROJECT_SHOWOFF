using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerEventHandler : MonoBehaviour
{
    [SerializeField] private LayerMask _objectLayer;
    private GameObject nearObject;
    [SerializeField] private Transform objectGrabPointTranform;
    int empty = 0;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (empty >= 4)
        {
            nearObject = null;
            empty = 0;
        }
        //up, down, left, right directions
        Vector2[] raycastDirection = { Vector2.up, Vector2.up, Vector2.left, Vector2.right};
        for (int i = 0; i < 4; i++)
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, raycastDirection[i], 1f, _objectLayer);
            if (hit.collider != null)
            {
                nearObject = hit.collider.gameObject;
            }
            else
            {
                empty++;
            }
        }
        Debug.Log(empty);


    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift) && nearObject != null)
        {
            Debug.Log(nearObject.name);
            if (nearObject.TryGetComponent(out ObjectGrabbable objectGrabbable))
            {
                objectGrabbable.Grab();

            }
        }
        if (Input.GetKeyUp(KeyCode.LeftShift) && nearObject != null || this.GetComponent<PlayerController>().isGrounded == false && nearObject != null)
        {
            if (nearObject.TryGetComponent(out ObjectGrabbable objectGrabbable))
            {
                objectGrabbable.Drop();
            }
        }
    }

    public void Death()
    {
        gameObject.transform.position = GameObject.FindGameObjectsWithTag("Respawn")[0].transform.position;
    }
}
