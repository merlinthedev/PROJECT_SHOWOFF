using UnityEngine;
using UnityEngine.Serialization;

public class Player : MonoBehaviour {
    [SerializeField] private PlayerMovementController playerMovementController;
    [SerializeField] private PlayerAnimatorController playerAnimatorController;
    [SerializeField] private PlayerAudioController playerAudioController;
    [SerializeField] private PlayerEventHandler playerEventHandler;
    [SerializeField] private PlayerProjectileController playerProjectileController;

    // Getters
    public PlayerMovementController GetPlayerController() {
        return this.playerMovementController;
    }

    public PlayerAnimatorController GetPlayerAnimatorController() {
        return this.playerAnimatorController;
    }

    public PlayerAudioController GetPlayerAudioController() {
        return this.playerAudioController;
    }

    public PlayerEventHandler GetPlayerEventHandler() {
        return this.playerEventHandler;
    }

    public PlayerProjectileController GetPlayerProjectileController() {
        return this.playerProjectileController;
    }
}