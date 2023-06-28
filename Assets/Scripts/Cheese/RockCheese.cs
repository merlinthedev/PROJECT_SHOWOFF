using UnityEngine;
using UnityEngine.InputSystem;


public class RockCheese : MonoBehaviour {
    [SerializeField] private Stone stonePrefab;
    [SerializeField] private Transform stoneSpawnPoint;
    [SerializeField] private Vector3 stoneSpawnOffset;
    [SerializeField] private InputAction rockCheeseAction;

    private void Start() {
        rockCheeseAction.Enable();
        rockCheeseAction.performed += ctx => rockCheese();
    }

    private void rockCheese() {
        //instantia rock at point
        var stoneScript = Instantiate(stonePrefab, stoneSpawnPoint.position + stoneSpawnOffset,
            stonePrefab.transform.rotation);


        if (stoneScript == null) {
            Debug.LogError("Stone prefab has no Stone script attached.");
            return;
        }

        stoneScript.SetSpawnerTransform(transform);
        stoneScript.OnSpawn();
    }
}