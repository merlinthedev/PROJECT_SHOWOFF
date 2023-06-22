using Unity.VisualScripting;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.PlayerLoop;
using UnityEngine.Events;
using FMODUnity;

public class BranchScript : MonoBehaviour {

    [SerializeField] EventReference bonkSound;
    [SerializeField] private bool isParent = false;
    public bool gotBonked = false;
    public bool fade = false;
    [SerializeField] private GameObject enableThis;
    [SerializeField] private RopePart ropePart;
    [SerializeField] private Joint2D joint;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private float fadeTime = 2f;
    private Rigidbody2D rb;
    private BranchScript root;
    [SerializeField] private UnityEvent OnBonk;

    private List<BranchScript> childBranches = new();

    private void Start() {
        rb = GetComponent<Rigidbody2D>();
        if (rb != null && isParent) {
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        if (isParent) {
            root = this;
            childBranches = GetComponentsInChildren<BranchScript>().ToList();
        } else {
            root = transform.parent.GetComponent<BranchScript>();
            if (root == null) {
                Debug.LogError("Root is null in branch.", this);
            }
        }
    }

    public void Bonk() {
        if (enableThis != null) {
            enableThis.gameObject.SetActive(true);
        }

        if (ropePart != null) {
            ropePart.Unlock();
        }

        this.Destroy();
    }


    public void Fade() {
        if (joint != null) Destroy(joint);
        if (rb != null) rb.bodyType = RigidbodyType2D.Dynamic;
        //turn off our collider
        if (GetComponent<Collider2D>() != null) {
            GetComponent<Collider2D>().enabled = false;
        }
        LeanTween.value(gameObject, setSpriteAlpha, 1f, 0f, fadeTime).setOnComplete(() => {
            Destroy(gameObject);
        });
    }

    private void setSpriteAlpha(float value) {
        if (spriteRenderer != null) {
            spriteRenderer.color =
                new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, value);
        }
    }

    //destroy all the fixed joints and fade out each individual the branch
    public void Destroy() {
        if (isParent) {
            RuntimeManager.PlayOneShot(bonkSound, transform.position);
            OnBonk?.Invoke();
            childBranches.ForEach(b => {
                b.Fade();
            });
        }
    }

    //if the branch collides with a stone, set gotBonked to true
    private void OnCollisionEnter2D(Collision2D collision) {
        if (collision.gameObject.CompareTag("Pickup")) {
            gotBonked = true;
            root.Bonk();
            Destroy(collision.gameObject);
            //play bonk sound
            Debug.Log("bonk sound played?");

        }
    }
}