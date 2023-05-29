using UnityEngine;

public class PlayerAudioController : MonoBehaviour {

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;

    [SerializeField] private AudioClip jumpSound;
    [SerializeField] private AudioClip walkSound;
    private bool walkSoundPlaying = false;

    public void PlayWalkingSound(bool isMoving) {
        if (!walkSoundPlaying && isMoving) {
            audioSource.clip = walkSound;
            audioSource.volume = 0.4f;
            audioSource.Play();
            walkSoundPlaying = true;
        } else if (!isMoving && walkSoundPlaying) {
            audioSource.Stop();
            walkSoundPlaying = false;
        }
    }

    public void PlayJumpSound() {
        audioSource.PlayOneShot(jumpSound);
    }
}