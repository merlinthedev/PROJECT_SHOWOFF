using UnityEngine;

public class Utils : MonoBehaviour {
    public static Utils Instance { get; private set; } = null;
    [SerializeField] private GameObject mainCamRef;

    private void Awake() {
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(this);
        }

        mainCamRef.SetActive(false);
        InvokeDelayed(0.2f, () => {
            mainCamRef.SetActive(true);
        });
    }

    // Refactor the code below to also take parameters for the System.Action

    public void InvokeDelayed(float delay, System.Action action) {
        StartCoroutine(InvokeDelayedCoroutine(delay, action));
    }

    private System.Collections.IEnumerator InvokeDelayedCoroutine(float delay, System.Action action) {
        yield return new WaitForSeconds(delay);
        action.Invoke();
    }

    public static bool IsInLayerMask(int layer, LayerMask layerMask) {
        return layerMask == (layerMask | (1 << layer));
    }
}