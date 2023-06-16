using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour {

    [SerializeField] private GameObject mainMenuObject;
    [SerializeField] private GameObject loadingScreenObject;
    public void StartGame() {
        loadingScreenObject.SetActive(true);
        mainMenuObject.SetActive(false);
        
        SceneManager.LoadScene("Level 1");
    }
}