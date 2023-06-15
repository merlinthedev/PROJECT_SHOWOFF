using System.Collections;
using UnityEngine;

public class Stone : AProjectile, IPickup, IReactor {
    private Transform spawnerTransform;
    private Camera mainCameraReference;

    private IEnumerator handleViewportPositionCoroutine;
    private IEnumerator handleGroundCollisionCoroutine;

    private Collider2D m_Collider2D;
    private Rigidbody2D m_Rigidbody2D;

    private void Start() {
        mainCameraReference = Camera.main;
        if (mainCameraReference == null) {
            Debug.LogError("Camera.main is null", this);
            Destroy(this);
        }
    }

    public void OnPickup(Player player) {
        transform.SetParent(player.transform);
        transform.position = player.GetPlayerProjectileController().GetHoldingTransform().position;
    }

    public void OnDrop() {
        throw new System.NotImplementedException();
    }

    public void OnSpawn() {
        handleGroundCollisionCoroutine = checkForGroundCollision();
        handleViewportPositionCoroutine = handleViewportPosition();

        m_Collider2D = gameObject.GetComponent<Collider2D>();
        m_Collider2D.isTrigger = false;
        m_Rigidbody2D = gameObject.GetComponent<Rigidbody2D>();
        if (m_Rigidbody2D == null) {
            Debug.Log("Rigidbody was null", this);
            return;
        }

        m_Rigidbody2D.bodyType = RigidbodyType2D.Dynamic;

        StartCoroutine(handleGroundCollisionCoroutine);
    }

    private IEnumerator checkForGroundCollision() {
        while (true) {
            Debug.Log("Checking ground collision.");
            if (m_Collider2D.IsTouchingLayers(LayerMask.GetMask("Grass"))) {
                Debug.Log("Stone collided with ground.");
                preparePlayerPickup();
            }

            yield return new WaitForSeconds(1f);
        }
    }

    public void OnThrow(Player player) {
        transform.SetParent(null);

        if (m_Collider2D == null) {
            Debug.LogError("Collider was null", this);
            return;
        }

        // Exclude player layer from the collider
        var x = LayerMask.NameToLayer("Player");
        m_Collider2D.excludeLayers = 1 << x;
        m_Collider2D.isTrigger = false;


        if (m_Rigidbody2D == null) {
            Debug.LogError("Rigidbody was null", this);
            return;
        }

        m_Rigidbody2D.bodyType = RigidbodyType2D.Dynamic;

        player.GetPlayerProjectileController().ResetProjectile();


        StartCoroutine(handleViewportPositionCoroutine);
    }

    private IEnumerator handleViewportPosition() {
        while (true) {
            Vector3 stoneViewportPosition = mainCameraReference.WorldToViewportPoint(transform.position);
            Debug.Log("Checking viewport position.");
            if (stoneViewportPosition.x < 0 || stoneViewportPosition.x > 1 || stoneViewportPosition.y < 0 ||
                stoneViewportPosition.y > 1) {
                handleStoneDestruction();
            }

            yield return new WaitForSeconds(1f);
        }
    }

    private void handleStoneDestruction() {
        StopCoroutine(handleViewportPositionCoroutine);
        StoneManager stoneManager = spawnerTransform.GetComponent<StoneManager>();
        stoneManager.ResetCurrentStone();
        stoneManager.SpawnStone();
        Destroy(gameObject);
    }

    private void preparePlayerPickup() {
        StopCoroutine(handleGroundCollisionCoroutine);

        m_Rigidbody2D.bodyType = RigidbodyType2D.Kinematic;
        m_Rigidbody2D.velocity = Vector2.zero;
        // m_Rigidbody2D.rotation = -45f;
        m_Rigidbody2D.angularVelocity = 0f;


        m_Collider2D.excludeLayers = 0;
        m_Collider2D.isTrigger = true;
    }

    public void SetSpawnerTransform(Transform t) {
        spawnerTransform = t;
    }
}