using EventBus;
using UnityEngine;

public class SceneLoader : MonoBehaviour {
    [SerializeField] private string sceneName;

    private void OnEnable() {
        EventBus<TobaccoPickupEvent>.Subscribe(onTobaccoPickup);
    }

    private void OnDisable() {
        EventBus<TobaccoPickupEvent>.Unsubscribe(onTobaccoPickup);

    }

    private void onTobaccoPickup(TobaccoPickupEvent e) {
        this.gameObject.GetComponent<Collider2D>().enabled = true;
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (!other.gameObject.CompareTag("Player")) {
            return;
        }

        Debug.Log("new scene triggerred");

        EventBus<NewSceneTriggeredEvent>.Raise(new NewSceneTriggeredEvent());
        Utils.Instance.InvokeDelayed(GlobalSceneManager.GetInstance().GetFadeTime(), () => GlobalSceneManager.GetInstance().LoadLevelFromString(sceneName));
    }
}