using UnityEngine;

public class SwampMonster : MonoBehaviour {
    protected Animator monsterAnimator;

    private void Start() {
        monsterAnimator = GetComponent<Animator>();
        if (monsterAnimator == null) {
            Debug.LogError("SwampMonster has no Animator component", this);
            Destroy(gameObject);
        }
    }
    
    
}