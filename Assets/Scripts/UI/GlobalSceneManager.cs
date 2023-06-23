using EventBus;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static UnityEngine.SceneManagement.SceneManager;

public class GlobalSceneManager : MonoBehaviour {
    [SerializeField] private GameObject mainMenuObject;
    [SerializeField] private GameObject loadingScreenObject;
    [SerializeField] private Image fadeImage;
    private float fadeTime = 1f;

    [SerializeField] private List<string> levelStrings = new();

    private static GlobalSceneManager instance;

    private void Awake() {
        if (instance == null) {
            instance = this;
            DontDestroyOnLoad(this);
        } else {
            Destroy(gameObject);
        }
    }

    private void OnEnable() {
        EventBus<NewSceneTriggeredEvent>.Subscribe(fadeOut);
    }

    private void OnDisable() {
        EventBus<NewSceneTriggeredEvent>.Unsubscribe(fadeOut);
    }

    public void StartGame() {
        loadingScreenObject.SetActive(true);
        mainMenuObject.SetActive(false);

        LoadSceneAsync(levelStrings[0]);

        sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1) {
        loadingScreenObject.SetActive(false);
        fadeIn();
    }

    public void LoadLevelFromString(string s) {
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