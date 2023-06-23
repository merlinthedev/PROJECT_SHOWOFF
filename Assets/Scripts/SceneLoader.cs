using EventBus;
using UnityEngine;

public class SceneLoader : MonoBehaviour {
    [SerializeField] private string sceneName;

    private void OnTriggerEnter2D(Collider2D other) {
        if (!other.gameObject.CompareTag("Player")) {
            return;
        }

        EventBus<NewSceneTriggeredEvent>.Raise(new NewSceneTriggeredEvent());
        Utils.Instance.InvokeDelayed(GlobalSceneManager.GetInstance().GetFadeTime(), () => GlobalSceneManager.GetInstance().LoadLevelFromString(sceneName));
    }
}