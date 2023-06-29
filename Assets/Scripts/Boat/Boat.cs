using EventBus;
using System;
using UnityEngine;

public class Boat : MonoBehaviour {
    [Header("Boat Components")] public GameObject reet;

    public Rigidbody2D rb;
    [SerializeField] private BoatHitbox bh;
    public BuoyancyEffector2D water;

    [Header("Boat Properties")] public float boatSpeed = 1;
    public bool playerInBoat = false;

    private void OnEnable() {
        EventBus<BoatDestinationReachedEvent>.Subscribe(onDestinationReached);
    }

    private void OnDisable() {
        EventBus<BoatDestinationReachedEvent>.Unsubscribe(onDestinationReached);
    }

    private void FixedUpdate() {
        if (playerInBoat) {
            MoveBoat(boatSpeed);
        }

        playerInBoat = bh.playerInBoat;
    }

    private void ReetCast() {
        Debug.DrawRay(reet.transform.position, Vector3.down * 0.5f, Color.red);
        RaycastHit2D hit;
        hit = Physics2D.Raycast(reet.transform.position, Vector2.down, .5f);
        if (hit.collider.gameObject.CompareTag("Water")) {
            Debug.Log("me heavy now");
            MoveBoat(1);
        }
    }

    private void MoveBoat(float speed) {
        rb.velocity = new Vector2(speed, 0);
    }

    private void onDestinationReached(BoatDestinationReachedEvent e) {
        boatSpeed = 0f;
    }

    private bool activated = false;

    private void OnCollisionEnter2D(Collision2D collision) {
        if (collision.gameObject.CompareTag("Trigger")) {
            if (activated) return;
            activated = true;
            boatSpeed = 0;
            Utils.Instance.InvokeDelayed(2f, () => boatSpeed = 1);
        }

        if (collision.gameObject.CompareTag("Vodyanoy")) {
            //collision.gameObject.GetComponent<Vodyanoy>().Connect();
            rb.freezeRotation = true;
        }
    }

    public BoatHitbox GetBoatHitbox() {
        return this.bh;
    }
}