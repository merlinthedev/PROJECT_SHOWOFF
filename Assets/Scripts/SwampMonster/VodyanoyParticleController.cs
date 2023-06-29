using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VodyanoyParticleController : MonoBehaviour
{
    [SerializeField] ParticleSystem _particleSystem;

    public void EnableParticles() {
        _particleSystem.Play();
    }

    public void DisableParticles() {
        _particleSystem.emissionRate = 0;
    }
}
