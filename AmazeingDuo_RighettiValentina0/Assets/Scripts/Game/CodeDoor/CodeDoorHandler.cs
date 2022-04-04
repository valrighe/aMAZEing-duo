using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Handles the collision with Code Door (the blue one)
public class CodeDoorHandler : MonoBehaviour
{
    public CodeDoorController doorController;
    
    void Start()
    {
        // Gets the door collided with the player
        doorController = GameObject.FindObjectOfType(typeof(CodeDoorController)) as CodeDoorController;
    }
    
    // Calls the CollisionHandler method of CodeDoorController class
    void OnCollisionEnter2D(Collision2D collision)
    {
        doorController.CollisionHandler(collision);
    }
}