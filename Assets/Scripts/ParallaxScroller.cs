using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxScroller : MonoBehaviour
{
    [SerializeField] private Vector2 scrollSpeed = Vector2.one;
    [SerializeField] private Vector2 offset = Vector2.zero;

    [SerializeField][Range(-9, 10)] private int depth = 0;

    Transform cameraTransform;

    // Start is called before the first frame update
    void Start()
    {
        cameraTransform = Camera.main.transform;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateParallaxScroll(cameraTransform);
    }

    public void UpdateParallaxScroll(Transform relativeObject)
    {
        Vector2 relativePosition = relativeObject.position;

        Vector2 newPosition = new Vector2(
            (relativePosition.x * scrollSpeed.x) + offset.x,
            (relativePosition.y * scrollSpeed.y) + offset.y
        );

        transform.position = newPosition;
    }
}