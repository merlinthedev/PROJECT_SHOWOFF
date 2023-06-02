using System.Collections;
using UnityEngine;

public class Stone : AProjectile, IPickup {
    private Transform spawnerTransform;
    private Camera mainCameraReference;

    private IEnumerator handleViewportPositionCoroutine;
    private IEnumerator handleGroundCollisionCoroutine;

    private Collider2D m_Collider2D;
    private Rigidbody2D m_Rigidbody2D;

    private void Start() {
        this.mainCameraReference = Camera.main;
        if (this.mainCameraReference == null) {
            Debug.LogError("Camera.main is null", this);
            Destroy(this);
        }
    }

    public void OnPickup(Player player) {
        this.transform.SetParent(player.transform);
        this.transform.position = player.GetPlayerProjectileController().GetHoldingTransform().position;
    }

    public void OnDrop() {
        throw new System.NotImplementedException();
    }

    public void OnSpawn() {
        this.handleGroundCollisionCoroutine = checkForGroundCollision();
        this.handleViewportPositionCoroutine = handleViewportPosition();

        this.m_Collider2D = this.gameObject.GetComponent<Collider2D>();
        this.m_Collider2D.isTrigger = false;
        this.m_Rigidbody2D = this.gameObject.GetComponent<Rigidbody2D>();
        if (this.m_Rigidbody2D == null) {
            Debug.Log("Rigidbody was null", this);
            return;
        }

        this.m_Rigidbody2D.bodyType = RigidbodyType2D.Dynamic;

        StartCoroutine(this.handleGroundCollisionCoroutine);
    }

    private IEnumerator checkForGroundCollision() {
        while (true) {
            Debug.Log("Checking ground collision.");
            if (this.m_Collider2D.IsTouchingLayers(LayerMask.GetMask("Ground"))) {
                Debug.Log("Stone collided with ground.");
                preparePlayerPickup();
            }

            yield return new WaitForSeconds(1f);
        }
    }

    public void OnThrow(Player player) {
        this.transform.SetParent(null);

        if (this.m_Collider2D == null) {
            Debug.LogError("Collider was null", this);
            return;
        }

        // Exclude player layer from the collider
        var x = LayerMask.NameToLayer("Player");
        this.m_Collider2D.excludeLayers = 1 << x;
        this.m_Collider2D.isTrigger = false;


        if (this.m_Rigidbody2D == null) {
            Debug.LogError("Rigidbody was null", this);
            return;
        }

        this.m_Rigidbody2D.bodyType = RigidbodyType2D.Dynamic;

        player.GetPlayerProjectileController().ResetProjectile();


        StartCoroutine(this.handleViewportPositionCoroutine);
    }

    private IEnumerator handleViewportPosition() {
        while (true) {
            Vector3 stoneViewportPosition = this.mainCameraReference.WorldToViewportPoint(this.transform.position);
            Debug.Log("Checking viewport position.");
            if (stoneViewportPosition.x < 0 || stoneViewportPosition.x > 1 || stoneViewportPosition.y < 0 ||
                stoneViewportPosition.y > 1) {
                this.handleStoneDestruction();
            }

            yield return new WaitForSeconds(1f);
        }
    }

    private void handleStoneDestruction() {
        this.StopCoroutine(this.handleViewportPositionCoroutine);
        StoneManager stoneManager = this.spawnerTransform.GetComponent<StoneManager>();
        stoneManager.ResetCurrentStone();
        stoneManager.SpawnStone();
        Destroy(this.gameObject);
    }

    private void preparePlayerPickup() {
        StopCoroutine(this.handleGroundCollisionCoroutine);

        this.m_Rigidbody2D.bodyType = RigidbodyType2D.Kinematic;
        this.m_Rigidbody2D.velocity = Vector2.zero;


        this.m_Collider2D.excludeLayers = 0;
        this.m_Collider2D.isTrigger = true;
    }

    public void SetSpawnerTransform(Transform t) {
        this.spawnerTransform = t;
    }
}