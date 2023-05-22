using UnityEngine;
using UnityEngine.Jobs;

public class Stone : AProjectile, IPickup {

    private Transform spawnerTransform;

    public void OnPickup(Player player) {
        this.transform.SetParent(player.transform);
        this.transform.position = player.GetHoldingTransform().position;
    }

    public void OnDrop() {
        throw new System.NotImplementedException();
    }

    public void OnSpawn() {
        var newCollider2D = this.gameObject.GetComponent<Collider2D>();
        newCollider2D.isTrigger = false;
        var _rigidbody2D = this.gameObject.GetComponent<Rigidbody2D>();
        if (_rigidbody2D == null) {
            Debug.Log("Rigidbody was null", this);
            return;
        }

        _rigidbody2D.bodyType = RigidbodyType2D.Dynamic;

        Utils.Instance.InvokeDelayed(2f, () => {
            _rigidbody2D.bodyType = RigidbodyType2D.Kinematic;
            newCollider2D.isTrigger = true;
            _rigidbody2D.velocity = Vector2.zero;
        });
    }

    public void OnThrow(Player player) {
        this.transform.SetParent(null);
        var newCollider2D = this.gameObject.GetComponent<Collider2D>();

        // Exclude player layer from the collider
        var x = LayerMask.NameToLayer("Player");
        newCollider2D.excludeLayers = 1 << x;
        newCollider2D.isTrigger = false;

        var newRigidbody2D = this.gameObject.GetComponent<Rigidbody2D>();

        if (newRigidbody2D == null) {
            Debug.Log("Rigidbody was null", this);
            return;
        }

        newRigidbody2D.bodyType = RigidbodyType2D.Dynamic;

        player.ResetProjectile();

        Utils.Instance.InvokeDelayed(2.5f, () => {
            // reset exlude layers
            if (newCollider2D == null) return;
            if (newRigidbody2D == null) return;
            newCollider2D.excludeLayers = 0;
            newCollider2D.isTrigger = true;
            newRigidbody2D.bodyType = RigidbodyType2D.Kinematic;
            newRigidbody2D.velocity = Vector2.zero;

            checkViewportPosition();
        });

    }

    private void checkViewportPosition() {
        var cam = Camera.main;
        if (cam == null) return;
        Debug.Log("Checking viewport position with camera " + cam.name);

        var viewportPos = cam.WorldToViewportPoint(this.transform.position);
        Debug.Log("Viewport position: " + viewportPos);

        if (viewportPos.x < 0 || viewportPos.x > 1 || viewportPos.y < 0 || viewportPos.y > 1) {
            Debug.Log("Stone is out of bounds");
            Destroy(this.gameObject);
            var stoneManager = this.spawnerTransform.GetComponent<StoneManager>();
            stoneManager.ResetCurrentStone();
            stoneManager.SpawnStone();
            return;
        }

        Utils.Instance.InvokeDelayed(2.5f, checkViewportPosition);

    }

    public void SetSpawnerTransform(Transform transform) {
        this.spawnerTransform = transform;
    }

    public void ToString() {
        Debug.Log("Picked up a stone");
    }
}