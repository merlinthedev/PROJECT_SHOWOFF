using UnityEngine;

public class TreeCheese : MonoBehaviour {
    [SerializeField] private float timeUntilActivation = 3f;

    private Collider2D m_Collider2D;

    private void Start() {
        m_Collider2D = GetComponent<Collider2D>();

        if (m_Collider2D == null) {
            Debug.LogError("Collider2D is null for TreeCheese");
            Destroy(this);
            return;
        }

        m_Collider2D.isTrigger = true;

        Utils.Instance.InvokeDelayed(timeUntilActivation, () => {
            m_Collider2D.isTrigger = false;
        });
    }
}