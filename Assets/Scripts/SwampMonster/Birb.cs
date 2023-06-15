using UnityEngine;

public class Birb : SwampMonster {
    [SerializeField] private Collider2D collisionTriggerCollider;
    [SerializeField] private float flyDistance = 50f;
    [SerializeField] private float flyTime = 10f;
    private float flyAngle;

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.GetComponent<IReactor>() == null) {
            return;
        }

        flyAngle = Random.Range(45f, 135f);

        monsterAnimator.SetTrigger("GoFly");

        Debug.Log("Birb is flying away + " + flyAngle);

        // move to the target
        moveTowardsTarget();
    }

    private void moveTowardsTarget() {
        // Tween to the target
        var target = transform.position + new Vector3(flyDistance, flyAngle, 0);
        LeanTween.move(gameObject, target, flyTime).setEase(LeanTweenType.easeInOutQuad);

        Utils.Instance.InvokeDelayed(flyTime, () => {
            Destroy(gameObject);
        });
    }

    public void OnDrawGizmosSelected() {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position, collisionTriggerCollider.bounds.size * 1.01f);
    }
}