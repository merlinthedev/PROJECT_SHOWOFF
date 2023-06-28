using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

public class AudioOneShot : MonoBehaviour
{
    [SerializeField] private EventReference audio;
    public void PlaySound() {
        FMODUnity.RuntimeManager.PlayOneShot(audio, transform.position);
    }
}