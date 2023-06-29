using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseMenuManager : MonoBehaviour
{
    [SerializeField] private InputAction pauseAction;
    [SerializeField] private GameObject menuRoot;

    private void Start() {
        pauseAction = new InputAction("Pause", binding: "<Keyboard>/escape");

        //bind action to method
        pauseAction.started += OnPauseButtonPressed;
        menuRoot.SetActive(false);

        //activate action
        pauseAction.Enable();
    }

    public void Pause() {
        //set timescale to 0
        Time.timeScale = 0f;
        menuRoot.SetActive(true);
    }

    public void Resume() {
        Time.timeScale = 1f;
        menuRoot.SetActive(false);
    }

    public void QuitToMain() {
        SceneManager.LoadScene("MainMenu");
    }

    void OnPauseButtonPressed(InputAction.CallbackContext context) {
        if (context.started) {
            if (Time.timeScale == 0f) {
                Resume();
            } else {
                Pause();
            }
        }
    }
}
