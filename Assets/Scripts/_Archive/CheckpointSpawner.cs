using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckpointSpawner : MonoBehaviour
{
    [SerializeField] private LayerMask _groundLayer;
    Vector2 checkpointPos;
    [SerializeField] GameObject checkpointPrefab;
    bool used = false;


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //Cast Raycast down
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, float.MaxValue, _groundLayer);
        if (hit.collider != null)
        {
            checkpointPos.x = hit.point.x;
            checkpointPos.y = hit.point.y;
        }

        //Delete old checkpoints
        GameObject myObject = GameObject.FindGameObjectsWithTag("Respawn")[0];
        Destroy(myObject);
        GameObject[] usedSpawners = GameObject.FindGameObjectsWithTag("CheckpointSpawner");
        for (int i = 0; i < usedSpawners.Length; i++)
        {
            if(usedSpawners[i].GetComponent<CheckpointSpawner>().used)
                Destroy(usedSpawners[i]);
        }

        //Instantiate new spawnpoint
        Vector3 myVector = new Vector3(checkpointPos.x, checkpointPos.y, 0);
        Instantiate(checkpointPrefab, myVector, Quaternion.identity);
        used = true;
    }
}
