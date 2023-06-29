using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class MusicBox : MonoBehaviour
{
    //Q: if we want continuously playing audio with fmod, how should I handle that?
    //A: you can use the eventInstance.start() function to start the event, and then eventInstance.stop() to stop it.
    //Q: how do I get the eventInstance?
    //A: you can use RuntimeManager.CreateInstance(eventReference) to get an eventInstance
    
    [SerializeField] EventReference musicBoxSound;
    EventInstance audio;

    [SerializeField] bool fadeIn;
    [SerializeField] float fadeInDuration;

    [SerializeField] bool prematureEnd;
    [SerializeField] bool fadeOut;
    [SerializeField] float fadeOutDuration;
    [SerializeField] bool triggerOnce;
    bool hasTriggered;

    private void OnTriggerEnter2D(Collider2D collision) {
        if (triggerOnce && hasTriggered) return;
        if (collision.gameObject.tag == "Player") {
            hasTriggered = true;
            audio = RuntimeManager.CreateInstance(musicBoxSound);
            audio.start();
            if(fadeIn) {
                LeanTween.value(0f, 1f, fadeInDuration).setOnUpdate((float val) => {
                    audio.setVolume(val);
                });
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision) {
        if (collision.gameObject.tag == "Player") {
            if (!prematureEnd) return;
            if (fadeOut) {
                LeanTween.value(1f, 0f, fadeOutDuration).setOnUpdate((float val) => {
                    audio.setVolume(val);
                }).setOnComplete(() => { audio.stop(FMOD.Studio.STOP_MODE.IMMEDIATE); });
            } else {
                audio.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            }
        }
    }
}
