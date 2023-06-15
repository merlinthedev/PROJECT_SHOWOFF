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
        public EventReference landSound;
    }
    public void Update() {
    
        //find an object with the tag Player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        // run Onland only once whenever the player becomes grounded and their y velocity becomes 0
        if (player.GetComponent<Rigidbody2D>().velocity.y == 0 && player.GetComponent<BetterPlayerMovement>().IsGrounded == true) {
            OnLand();
            Debug.Log("landed");
        }
        
        
        



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

    public void OnLand() {
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

                RuntimeManager.PlayOneShot(config.landSound, transform.position);
            }
        }
    }
}