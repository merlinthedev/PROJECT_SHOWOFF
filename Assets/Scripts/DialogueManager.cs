using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using FMODUnity;

public class DialogueManager : MonoBehaviour {
    [SerializeField] DialogueEntry[] entries;
    private int textPos;
    private bool fadeIn;

    [System.Serializable]
    class DialogueEntry {
        public string dialogue;
        public float endDelay;
        public TMP_Text text;
        public EventReference sound;
    }

    private void Start() {
        textPos = 0;
        RunDialogue();
        for(int i  = 0; i < entries.Length; i++) {
            var textCol = entries[i].text.color;
            entries[i].text.color = new Color(textCol.r, textCol.g, textCol.b, 0);
        }
    }

    private void FixedUpdate() {
        var currTextCol = entries[textPos].text.color;
        //fade in the text
        if (fadeIn) {
            currTextCol = new Color(currTextCol.r, currTextCol.g, currTextCol.b, currTextCol.a + 0.01f);
            entries[textPos].text.color = currTextCol;
        }

        //if there is a next entry in the array, do this after the text is faded in
        if (currTextCol.a >= 1f && textPos < entries.Length - 1) { 
            fadeIn = false;
            textPos++;
            StartCoroutine(InvokeDelayedCoroutine(RunDialogue));
        }
    }

    public void RunDialogue() {
        entries[textPos].text.text = entries[textPos].dialogue;
        fadeIn = true;

        if (!entries[textPos].sound.IsNull)
            RuntimeManager.PlayOneShot(entries[textPos].sound, transform.position);

    }

    public void NextScene() {
        GlobalSceneManager.GetInstance().LoadLevelFromString("TheRealExampleArtScene");
    }

    private IEnumerator InvokeDelayedCoroutine(System.Action action) {

        Debug.Log("delaying");
        yield return new WaitForSeconds(entries[textPos - 1].endDelay);
        if(textPos == entries.Length) NextScene();

        action.Invoke();
    }
}