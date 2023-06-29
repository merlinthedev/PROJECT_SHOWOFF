using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DialogueManager : MonoBehaviour {
    [SerializeField] string[] dialogue;
    [SerializeField] float delay;
    [SerializeField] TMP_Text[] text;
    private int textPos;
    private bool fadeIn;

    private void Start() {
        StartCoroutine(InvokeDelayedCoroutine(RunDialogue));
        textPos = 0;
        for(int i  = 0; i < text.Length; i++) {
            text[i].color = new Color(text[i].color.r, text[i].color.g, text[i].color.b, 0);
        }
    }

    private void FixedUpdate() {
        //fade in the text
        if (fadeIn) {
            text[textPos].color = new Color(text[textPos].color.r, text[textPos].color.g, text[textPos].color.b, text[textPos].color.a + 0.01f);
        }

        //if there is a next entry in the array, do this after the text is faded in
        if (text[textPos].color.a >= 1f && textPos < text.Length - 1) { 
            fadeIn = false;
            textPos++;
            StartCoroutine(InvokeDelayedCoroutine(RunDialogue));
        }
    }

    public void RunDialogue() {
        text[textPos].text = dialogue[textPos];
        fadeIn = true;
    }

    private System.Collections.IEnumerator InvokeDelayedCoroutine(System.Action action) {
        Debug.Log("delaying");
        yield return new WaitForSeconds(delay);
        action.Invoke();
    }
}