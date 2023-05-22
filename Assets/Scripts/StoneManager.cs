using System;
using UnityEngine;

public class StoneManager : MonoBehaviour {
    [SerializeField] private GameObject stonePrefab;
    [SerializeField] private Transform stoneSpawnPoint;

    [SerializeField] private GameObject currentStone = null;

    private void Start() {
        SpawnStone();
    }

    public void SpawnStone() {
        if (this.currentStone != null) {
            Debug.LogError("Cannot spawn stone when another stone still exists.");
            return;
        }

        this.currentStone = Instantiate(this.stonePrefab, this.stoneSpawnPoint.position, Quaternion.identity);

        var stoneScript = this.currentStone.GetComponent<Stone>();
        if (stoneScript == null) {
            Debug.LogError("Stone prefab has no Stone script attached.");
            return;
        }
        
        stoneScript.SetSpawnerTransform(this.transform);
        stoneScript.OnSpawn();
        
    }

    public void ResetCurrentStone() {
        this.currentStone = null;
    }
}