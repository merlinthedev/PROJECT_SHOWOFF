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
        if (currentStone != null) {
            Debug.LogError("Cannot spawn stone when another stone still exists.");
            return;
        }

        currentStone = Instantiate(stonePrefab, stoneSpawnPoint.position,
            stonePrefab.transform.rotation);

        var stoneScript = currentStone.GetComponent<Stone>();
        if (stoneScript == null) {
            Debug.LogError("Stone prefab has no Stone script attached.");
            return;
        }

        stoneScript.SetSpawnerTransform(transform);
        stoneScript.OnSpawn();
    }

    public void ResetCurrentStone() {
        currentStone = null;
    }
}