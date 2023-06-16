using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AnimationEvents : MonoBehaviour
{
    [SerializeField] private UnityEvent onPickup;

    public void Pickup() {
        onPickup?.Invoke();
    }
}
        
