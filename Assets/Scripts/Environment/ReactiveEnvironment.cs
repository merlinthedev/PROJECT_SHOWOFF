using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReactiveEnvironment : MonoBehaviour
{
    [SerializeField] private GameObject queuedEvent;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        //if the collision has player tag
        if (collision.CompareTag("Player") || collision.CompareTag("Pickup")) {
            if (queuedEvent != null) 
            {
                queuedEvent.TryGetComponent(out ActivateEnvironmentAnimation animation);
                if (animation != null) animation.Activate();
            }
        }
    }
}
