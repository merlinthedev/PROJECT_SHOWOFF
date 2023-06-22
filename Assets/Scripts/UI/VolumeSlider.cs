using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

public class VolumeSlider : MonoBehaviour
{
    [SerializeField] private UnityEngine.UI.Slider slider;
    private FMOD.Studio.Bus bus;
    [SerializeField] private string busName;
    // Start is called before the first frame update
    void Start()
    {
        bus = RuntimeManager.GetBus("bus:/" + busName);
        float busVolume;
        bus.getVolume(out busVolume);
        //check if pref exists
        if (PlayerPrefs.HasKey(busName)) {
            busVolume = PlayerPrefs.GetFloat(busName);
        }
        slider.value = busVolume;
    }

    public void UpdateVolume(float newVolume) {
        bus.setVolume(newVolume);
        //save to unity preferences
        PlayerPrefs.SetFloat(busName, newVolume);
        PlayerPrefs.Save();
    }

    private void OnValidate() {
        if (Application.isPlaying) {
            bus = RuntimeManager.GetBus("bus:/" + busName);
        }
    }
}
