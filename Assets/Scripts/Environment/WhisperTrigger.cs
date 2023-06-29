using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

public class WhisperTrigger : MonoBehaviour
{
    [SerializeField] private EventReference whisperSound;
    [SerializeField] private float minTriggerDelay;
    [SerializeField] private float maxTriggerDelay;
    private float nextTriggerTime;
    [SerializeField] private Collider2D triggerCollider;
    private bool playerInTrigger = false;

    // Start is called before the first frame update
    void Start()
    {
        triggerCollider.isTrigger = true;
        Utils.Instance.InvokeDelayed(Random.Range(minTriggerDelay, maxTriggerDelay), TriggerWhisper);
    }

    void TriggerWhisper() {
        if (playerInTrigger) {
            RuntimeManager.PlayOneShot(whisperSound, transform.position);
        }
        nextTriggerTime = Time.time + Random.Range(minTriggerDelay, maxTriggerDelay);
        Utils.Instance.InvokeDelayed(nextTriggerTime - Time.time, TriggerWhisper);
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.gameObject.tag == "Player") {
            playerInTrigger = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision) {
        if (collision.gameObject.tag == "Player") {
            playerInTrigger = false;
        }
    }
}
