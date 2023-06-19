using Unity.VisualScripting;
using UnityEngine;

public class ExternalPlayerController : MonoBehaviour {
    [SerializeField] private Player player;
    [SerializeField] private GameObject destination;

    [SerializeField] private bool shouldIntro = false;


    private void Start() {
        Utils.Instance.InvokeDelayed(0.5f, () => Move(destination.gameObject.transform.position));
    }

    private void Move(Vector3 destination) {
        if (!shouldIntro) return;
        player.GetPlayerController().externalLocomotion(destination);
    }
}