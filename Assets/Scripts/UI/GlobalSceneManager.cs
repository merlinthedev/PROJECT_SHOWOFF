using EventBus;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static UnityEngine.SceneManagement.SceneManager;

public class GlobalSceneManager : MonoBehaviour {
    [SerializeField] private InteractableAutoSelect autoSelect;
    [SerializeField] private GameObject mainMenuObject;
    [SerializeField] private GameObject settingsMenuObject;
    [SerializeField] private GameObject loadingScreenObject;
    [SerializeField] private Image fadeImage;
    private float fadeTime = 2.5f;

    [SerializeField] private List<string> levelStrings = new();

    private static GlobalSceneManager instance;

    private void Awake() {
        if (instance == null) {
            instance = this;
            DontDestroyOnLoad(this);
        } else {
            Destroy(gameObject);
        }

        fadeImage.gameObject.SetActive(false);
    }

    private void OnEnable() {
        EventBus<NewSceneTriggeredEvent>.Subscribe(fadeOut);
    }

    private void OnDisable() {
        EventBus<NewSceneTriggeredEvent>.Unsubscribe(fadeOut);
    }

    public void StartGame() {
        loadingScreenObject.SetActive(true);
        mainMenuObject.gameObject.transform.parent.gameObject.SetActive(false);

        LoadSceneAsync(levelStrings[0]);

        sceneLoaded += OnSceneLoaded;
    }

    public void SettingsNav() {
        settingsMenuObject.SetActive(true);
        mainMenuObject.SetActive(false);

        try {
            autoSelect.OnPerformed();
        } catch (System.Exception e) {
            Debug.Log("No auto select found");
            Debug.Log(e.Message);
        }
    }

    public void MainNav() {
        mainMenuObject.SetActive(true);
        settingsMenuObject.SetActive(false);
        autoSelect.OnPerformed();
    }

    private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1) {
        loadingScreenObject.SetActive(false);
        fadeIn();
    }

    public void LoadLevelFromString(string s) {
        if (s == "MainMenu") {
            LoadSceneAsync(s);
            fadeImage.gameObject.SetActive(false);
            mainMenuObject.gameObject.transform.parent.gameObject.SetActive(true);
            return;
        }

        if (levelStrings.Contains(s)) {
            LoadSceneAsync(s);
        }
    }

    public void LoadLevelFromIndex(int i) {
        string level = levelStrings[i];

        if (level != null) {
            LoadLevelFromString(level);
        }
    }

    private void fadeIn() {
        fadeImage.gameObject.SetActive(true);
        fadeImage.CrossFadeAlpha(0, fadeTime, false);
    }

    private void fadeOut(NewSceneTriggeredEvent e) {
        fadeImage.CrossFadeAlpha(1, fadeTime, false);
    }

    public static GlobalSceneManager GetInstance() {
        return instance;
    }

    public float GetFadeTime() {
        return fadeTime;
    }
}