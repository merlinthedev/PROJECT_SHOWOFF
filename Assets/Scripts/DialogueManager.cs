using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using FMODUnity;
using UnityEngine.Events;

public class DialogueManager : MonoBehaviour {
    [SerializeField] DialogueEntry[] entries;
    private int entryIndex;

    [System.Serializable]
    class DialogueEntry {
        public string dialogue;
        public float endDelay;
        public TMP_Text text;
        public EventReference sound;
        public UnityEvent onEnd;
    }

    private void Start() {
        //enduser we start at beginning and texts are invisible
        entryIndex = 0;
        for(int i  = 0; i < entries.Length; i++) {
            var textCol = entries[i].text.color;
            entries[i].text.color = new Color(textCol.r, textCol.g, textCol.b, 0);
        }
        
        RunDialogue();
    }

    public void RunDialogue() {
        var currentEntry = entries[entryIndex];
        if(currentEntry == null) {
            Debug.LogError("No entry for index " + entryIndex);
            return;
        }
        
        //ensure text is the one specified
        currentEntry.text.text = currentEntry.dialogue;

        //play the sound if specified
        if (!currentEntry.sound.IsNull)
            RuntimeManager.PlayOneShot(currentEntry.sound, transform.position);

        //tween the color of the text to fade in over 1.6 seconds
        LeanTween.value(currentEntry.text.gameObject, 0f, 1f, 1.6f).setOnUpdate((float val) => {
            var textCol = currentEntry.text.color;
            currentEntry.text.color = new Color(textCol.r, textCol.g, textCol.b, val);
        }).setOnComplete(() => {
            //when tween is done, wait for the end delay and then invoke the end dialogue function
            StartCoroutine(InvokeDelayedCoroutine(OnEndDialogue, currentEntry.endDelay));
        });
    }


    void OnEndDialogue() {
        entries[entryIndex].onEnd?.Invoke();
        if (entryIndex < entries.Length - 1) {
            entryIndex++;
            RunDialogue();
        }
    }

    
    public void NextScene(string sceneName) {
        //fadeout all text over 1.6 seconds
        for (int i = 0; i < entries.Length; i++) {
            var textCol = entries[i].text.color;
            LeanTween.value(entries[i].text.gameObject, textCol.a, 0f, 1.6f).setOnUpdate((float val) => {
                entries[i].text.color = new Color(textCol.r, textCol.g, textCol.b, val);
            });
        }
        //after the fadeout is done, load the next scene
        StartCoroutine(InvokeDelayedCoroutine(() => {
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
        }, 2f));

        //GlobalSceneManager.GetInstance().LoadLevelFromString(sceneName);
    }

    private IEnumerator InvokeDelayedCoroutine(System.Action action, float delay) {
        Debug.Log("delaying");
        yield return new WaitForSeconds(delay);
        action.Invoke();
    }
}