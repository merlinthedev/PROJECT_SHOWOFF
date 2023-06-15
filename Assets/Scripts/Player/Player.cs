using UnityEngine;

public class Player : MonoBehaviour, IReactor {
    [SerializeField] private BetterPlayerMovement playerMovementController;
    [SerializeField] private PlayerAnimatorController playerAnimatorController;
    [SerializeField] private PlayerAudioController playerAudioController;
    [SerializeField] private PlayerEventHandler playerEventHandler;
    [SerializeField] private PlayerProjectileController playerProjectileController;

    // Getters
    public BetterPlayerMovement GetPlayerController() {
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