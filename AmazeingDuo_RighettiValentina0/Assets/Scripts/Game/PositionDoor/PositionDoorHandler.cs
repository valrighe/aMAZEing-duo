using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Handles the Position Door (the yellow one) behaviour after collision
public class PositionDoorHandler : MonoBehaviour
{
    private PositionDoorController doorController;
    
    // Initialization of doorController
    void Start()
    {
        doorController = GameObject.FindObjectOfType(typeof(PositionDoorController)) as PositionDoorController;
    }
    
    // Calls the CollisionHandler function of PositionDoorController script which handles the collision
    void OnCollisionEnter2D(Collision2D collision)
    {
        doorController.CollisionHandler(collision);
    }
}