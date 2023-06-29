using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AnimationEvents : MonoBehaviour
{
    [SerializeField] private UnityEvent onPickup;
    [SerializeField] private UnityEvent onPickupEnd;

    public void Pickup() {
        onPickup?.Invoke();
    }

    public void PickupEnd() {
        onPickupEnd?.Invoke();
    }
    
}
        
