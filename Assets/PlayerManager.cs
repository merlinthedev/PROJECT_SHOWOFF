using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnPlayerJoined(PlayerInput player)
    {
        Debug.Log("Player Joined: " + player.currentControlScheme);
    }

    public void OnPlayerLeft(PlayerInput player)
    {
        Debug.Log("Player Left: " + player.currentControlScheme);
    }
}
