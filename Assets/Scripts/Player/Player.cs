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
        return playerMovementController;
    }

    public PlayerAnimatorController GetPlayerAnimatorController() {
        return playerAnimatorController;
    }

    public PlayerAudioController GetPlayerAudioController() {
        return playerAudioController;
    }

    public PlayerEventHandler GetPlayerEventHandler() {
        return playerEventHandler;
    }

    public PlayerProjectileController GetPlayerProjectileController() {
        return playerProjectileController;
    }
}