using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//fmod
using FMODUnity;

public class FootstepSounds : MonoBehaviour {
    [SerializeField]
    LayerMask soundCheckLayer;

    //fmod sound reference
    [SerializeField]
    List<FootstepConfig> footstepConfigs = new List<FootstepConfig>();

    [System.Serializable]
    public class FootstepConfig {
        public string tag;
        public EventReference sound;
    }

    public void OnFootstep() {
        //circle cast to check trigger colliders we are in
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, 0.1f, soundCheckLayer);

        //if we are in a collider
        if (colliders.Length > 0) {
            //get the tag of the collider
            string tag = colliders[0].gameObject.tag;

            //find the sound config for that tag
            FootstepConfig config = footstepConfigs.Find(x => x.tag == tag);

            //if we found a config
            if (config != null) {
                //play the sound

                RuntimeManager.PlayOneShot(config.sound, transform.position);
            }
        }
    }
}