using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VodyanoyParticleController : MonoBehaviour
{
    [SerializeField] ParticleSystem _particleSystem;

    public void EnableParticles() {
        _particleSystem.Play();

        Utils.Instance.InvokeDelayed(1.8f, () => {
            Debug.Log("HGEKOOFDJGDG");
            GlobalSceneManager.GetInstance().LoadLevelFromString("Outro Dialogue");
        });
    }

    public void DisableParticles() {
        _particleSystem.emissionRate = 0;
    }
}
