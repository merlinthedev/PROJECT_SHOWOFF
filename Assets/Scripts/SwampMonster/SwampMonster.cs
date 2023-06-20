using UnityEngine;

public class SwampMonster : MonoBehaviour {
    protected Animator monsterAnimator;

    private void Start() {
        monsterAnimator = GetComponent<Animator>();
        if (monsterAnimator == null) {
            Debug.LogError("SwampMonster has no Animator component", this);
            Destroy(gameObject);
        }
        
        // get the initial animation and start it at a random frame
        var animatorStateInfo = monsterAnimator.GetCurrentAnimatorStateInfo(0);
        monsterAnimator.Play(animatorStateInfo.fullPathHash, -1, Random.Range(0f, 1f));
        
    }
    
    
}