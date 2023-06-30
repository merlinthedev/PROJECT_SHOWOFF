using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CreditsManager : MonoBehaviour
{
    [SerializeField] RectTransform creditsRoot;
    [SerializeField] float creditsStart = -150f;
    [SerializeField] float creditsEnd = 150f;
    [SerializeField] float creditsSpeed = 60f;
    [SerializeField] Image fade;

    [Header("Skip")]
    [SerializeField] Image skipHint;
    [SerializeField] RectMask2D skipProgress;

    [SerializeField] InputAction showSkipButton;
    float showSkipTimer = 0f;
    [SerializeField] float skipFadeoutTime = 3f;
    float lastShowButtonPressTime = -100f;

    [SerializeField] InputAction skipButton;
    [SerializeField] float skipHoldTime = 3f;
    float skipTimer = 0f;
    bool isSkipButtonPressed = false;
    
    void Start() {
        skipButton.Enable();
        skipButton.started += skipButtonPressed;
        skipButton.canceled += skipButtonReleased;
        showSkipButton.Enable();
        showSkipButton.started += showSkipButtonPressed;

        StartCoroutine(InvokeDelayedCoroutine(ShowCredits, 1f));
    }

    private void OnDestroy() {
        skipButton.Disable();
        skipButton.started -= skipButtonPressed;
        skipButton.canceled -= skipButtonReleased;

        showSkipButton.Disable();
        showSkipButton.started -= showSkipButtonPressed;
    }

    private void Update() {
        if(isSkipButtonPressed) {
            skipTimer += Time.deltaTime;
        } else {
            skipTimer -= Time.deltaTime;
        }

        if(skipTimer > 0) {
            showSkipTimer += Time.deltaTime;
        } else {
            if (lastShowButtonPressTime - Time.time < skipFadeoutTime) {
                showSkipTimer += Time.deltaTime;
            } else {
                showSkipTimer -= Time.deltaTime;
            }
        }
        
        skipTimer = Mathf.Clamp(skipTimer, 0, skipHoldTime);
        showSkipTimer = Mathf.Clamp(showSkipTimer, 0, skipFadeoutTime);

        UpdateSkipVisuals();

        if(skipTimer == skipHoldTime) {
            HideCredits();
        }
    }

    void UpdateSkipVisuals() {
        //update skipProgress mask based on timer
        skipProgress.padding = new Vector4(0, 0, 0, skipProgress.rectTransform.rect.height - (skipTimer / skipHoldTime * skipProgress.rectTransform.rect.height));

        //update skipHint alpha based on timer
        skipHint.color = new Color(.4f, .4f, .4f, showSkipTimer / skipFadeoutTime);
        
    }



    void ShowCredits() {
        //scroll the credits
        LeanTween.value(creditsStart, creditsEnd, Mathf.Abs(creditsEnd - creditsStart) / creditsSpeed).setOnUpdate((float val) => {
            creditsRoot.anchoredPosition = new Vector2(0, val);
        }).setOnComplete(() => {
            StartCoroutine(InvokeDelayedCoroutine(HideCredits, 1f));
        });
    }

    void HideCredits() {
        LeanTween.value(0f, 1f, 1.6f).setOnUpdate((float val) => {
            fade.color = new Color(0f, 0f, 0f, val);
        }).setOnComplete(() => {
            StartCoroutine(InvokeDelayedCoroutine(OnEndCredits, 1f));
        });
    }

    void OnEndCredits() {
        LeanTween.cancelAll();
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

    IEnumerator InvokeDelayedCoroutine(System.Action action, float delay) {
        yield return new WaitForSeconds(delay);
        action?.Invoke();
    }

    void skipButtonPressed(InputAction.CallbackContext e) {
        isSkipButtonPressed = true;
        lastShowButtonPressTime = Time.time;
    }

    void skipButtonReleased(InputAction.CallbackContext e) {
        isSkipButtonPressed = false;
    }

    void showSkipButtonPressed(InputAction.CallbackContext e) {
        if (skipTimer > 0) return;
        lastShowButtonPressTime = Time.time;
    }

    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.red;
        //draw line from where credits start to where they end relative to credits center
        Gizmos.DrawLine(new Vector3(0, creditsStart, 0), new Vector3(0, creditsEnd, 0));
    }
}
