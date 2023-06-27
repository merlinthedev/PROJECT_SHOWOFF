using UnityEngine;
using UnityEngine.InputSystem;

public class DebugManager : MonoBehaviour {
    [SerializeField] private InputAction resetSceneAction;

    private void Awake() {
        DontDestroyOnLoad(this);
        resetSceneAction.Enable();
        resetSceneAction.performed += ctx => resetScene();
    }

    private void resetScene() {
        // get the current scene
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        // unload and load the scene
        UnityEngine.SceneManagement.SceneManager.LoadScene(scene.name);
    }
}