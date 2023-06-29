using EventBus;
using System.Collections.Generic;
using UnityEngine;

public class Boat : MonoBehaviour {
    [Header("Boat Components")] public GameObject reet;

    public Rigidbody2D rb;
    [SerializeField] private BoatHitbox bh;
    public BuoyancyEffector2D water;

    [Header("Boat Properties")] public float boatSpeed = 1;
    public bool playerInBoat = false;
    private bool boatInWater = false;

    [SerializeField] private List<Transform> checkpoints = new();

    private bool shouldMove = false;
    [SerializeField] private float frozenTime = 10f;
    private float rawX;

    private void OnEnable() {
        EventBus<BoatDestinationReachedEvent>.Subscribe(onDestinationReached);
        EventBus<PlayerBoatEnter>.Subscribe(onPlayerBoatEnter);

    }

    private void OnDisable() {
        EventBus<BoatDestinationReachedEvent>.Unsubscribe(onDestinationReached);
        EventBus<PlayerBoatEnter>.Unsubscribe(onPlayerBoatEnter);

    }

    private void FixedUpdate() {
        playerInBoat = bh.playerInBoat;

        if (shouldMove) {
            MoveBoat(rawX);
        }
    }


    private void MoveBoat(float xMove) {
        float desiredHorizontalSpeed = xMove * boatSpeed;
        float velocityGap = desiredHorizontalSpeed - rb.velocity.x;

        float acceleration = 70;
        float accelerationThisFrame = acceleration * Time.fixedDeltaTime;
        float accelerationSign = Mathf.Sign(velocityGap);
        float accelerationMagnitude = Mathf.Min(Mathf.Abs(velocityGap), accelerationThisFrame);

        Vector2 accelerationVector = new Vector2(accelerationMagnitude * accelerationSign, 0);

        rb.AddForce(accelerationVector, ForceMode2D.Impulse);

    }

    private void onDestinationReached(BoatDestinationReachedEvent e) {
        shouldMove = false;

        Debug.Log("bdre received");

        if (e.last) return;

        Debug.Log("updating raw x");
        rawX = -(transform.position.x - checkpoints[e.index + 1].position.x);
        Utils.Instance.InvokeDelayed(frozenTime, () => {
            shouldMove = true;
            Debug.Log("shouldMove = true");
        });
    }

    private void onPlayerBoatEnter(PlayerBoatEnter e) {
        Debug.Log("player entered boat");
        rawX = -(transform.position.x - checkpoints[0].position.x);
        rawX = Mathf.Clamp(rawX, -1, 1);

        if (boatInWater) {
            shouldMove = true;
            bh.GetPlayer().GetPlayerController().noJumpAllowed = true;
        }

    }


    private void OnCollisionEnter2D(Collision2D collision) {
        if (collision.gameObject.CompareTag("Vodyanoy")) {
            //collision.gameObject.GetComponent<Vodyanoy>().Connect();
            rb.freezeRotation = true;
        }
    }

    public void BoatInWater() {
        boatInWater = true;
    }

    public BoatHitbox GetBoatHitbox() {
        return this.bh;
    }
}