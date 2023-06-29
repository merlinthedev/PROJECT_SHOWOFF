using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;


public class VodyanoySounds : MonoBehaviour
{

    [SerializeField] EventReference frogWalk;
    [SerializeField] EventReference frogCroack;
    [SerializeField] EventReference frogRise;
    private void FrogWalk() {
          RuntimeManager.PlayOneShot(frogWalk, transform.position);
    }
    private void FrogCroak() {
        RuntimeManager.PlayOneShot(frogCroack, transform.position);
    }
    private void FrogRise() {
        RuntimeManager.PlayOneShot(frogRise, transform.position);
    }
}
