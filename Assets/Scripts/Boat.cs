using UnityEngine;

public class Boat : MonoBehaviour {

    [Header("Boat Components")]
    public GameObject reet;

    public Rigidbody2D rb;
    [SerializeField] private BoatHitbox bh;
    public BuoyancyEffector2D water;

    [Header("Boat Properties")]
    public float boatMass = 1;

    public float boatSpeed = 1;
    public bool playerInBoat = false;

    private void FixedUpdate() {
        if (playerInBoat && rb.mass == boatMass) {
            water.density = 50;
            MoveBoat(boatSpeed);
        }

        playerInBoat = bh.playerInBoat;

        if (rb.mass != boatMass) ReetCast();

    }

    private void ReetCast() {
        Debug.DrawRay(reet.transform.position, Vector3.down * .5f, Color.red);
        RaycastHit2D hit;
        hit = Physics2D.Raycast(reet.transform.position, Vector2.down, .5f);
        if (hit.collider.gameObject.CompareTag("Water")) {
            Debug.Log("me heavy now");
            rb.mass = boatMass;
            MoveBoat(1);
        }
    }

    private void MoveBoat(float speed) {
        rb.velocity = new Vector2(speed, 0);
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        if (collision.gameObject.CompareTag("Trigger")) {
            boatSpeed = 0;
        }
        if (collision.gameObject.CompareTag("Vodyanoy")) {
            //collision.gameObject.GetComponent<Vodyanoy>().Connect();
            rb.mass = 0.1f;
            boatMass = 0.1f;
            rb.freezeRotation = true;
        }
    }
}