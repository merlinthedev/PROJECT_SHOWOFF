using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//fmod
using FMODUnity;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;

public class FootstepSounds : MonoBehaviour {
    [SerializeField] LayerMask soundCheckLayer;

    [SerializeField]
    EventReference climbSound;
    [SerializeField]
    EventReference ropeSound;
    [SerializeField]
    EventReference jumpSound;
    [SerializeField]
    EventReference ledgeClimbPart1;
    [SerializeField]
    EventReference ledgeClimbPart2;
    //fmod sound reference
    [SerializeField]
    List<FootstepConfig> footstepConfigs = new List<FootstepConfig>();

    [System.Serializable]
    public class FootstepConfig {
        public string tag;
        public EventReference sound;
        public EventReference landSound;
        
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
    public void OnClimb() {
        RuntimeManager.PlayOneShot(climbSound, transform.position);
        
    }
    public void OnRope() {
        RuntimeManager.PlayOneShot(ropeSound, transform.position);
    }
    public void OnJump() {
        RuntimeManager.PlayOneShot(jumpSound, transform.position);
        
    }

    ///Ledge Climb
    ///I only know cheese
    public void OnLedgeClimbPart1() {
          RuntimeManager.PlayOneShot(ledgeClimbPart1, transform.position);
        UnityEngine.Debug.Log("We climb part 1");
    }
    public void OnLedgeClimbPart2() {
          RuntimeManager.PlayOneShot(ledgeClimbPart2, transform.position);
        UnityEngine.Debug.Log("We climb part 2");
    }
}