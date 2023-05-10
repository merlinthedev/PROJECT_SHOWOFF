using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ParallaxScroller : MonoBehaviour
{
    [SerializeField] private Vector2 scrollSpeed = Vector2.one;
    [SerializeField] private Vector2 offset = Vector2.zero;

    [SerializeField][Range(-9, 10)] private int depth = 0;

    public bool preview = false;
    private bool pPreview = false;

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

        Vector3 newPosition = new Vector3(
            (relativePosition.x * scrollSpeed.x) + offset.x,
            (relativePosition.y * scrollSpeed.y) + offset.y,
            depth
        );

        transform.position = newPosition;
    }

    public void ResetPosition()
    {
        transform.position = new Vector3(0, 0, depth);

    }

    private void OnDrawGizmos()
    {
        //if not in play mode
        if (Application.isPlaying) return;

        //if previewing, update parallax scroll to scene view transform
        if (preview)
        {
            //get scene view transform
            Transform sceneViewTransform = SceneView.lastActiveSceneView.camera.transform;
            UpdateParallaxScroll(sceneViewTransform);
        }
        else if (pPreview) //reset if we turned it off
            ResetPosition();

        pPreview = preview;
    }
}