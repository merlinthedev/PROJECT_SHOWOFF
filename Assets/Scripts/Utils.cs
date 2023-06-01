using UnityEngine;

public class Utils : MonoBehaviour {
    public static Utils Instance { get; private set; } = null;

    private void Awake() {
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(this);
        }
    }
    
    // Refactor the code below to also take parameters for the System.Action
    
    public void InvokeDelayed(float delay, System.Action action) {
        StartCoroutine(InvokeDelayedCoroutine(delay, action));
    }

    private System.Collections.IEnumerator InvokeDelayedCoroutine(float delay, System.Action action) {
        yield return new WaitForSeconds(delay);
        action.Invoke();
    }
}