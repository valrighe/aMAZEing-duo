using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Handles the collision with the yellow position when reached
public class PositionsHandler : MonoBehaviour
{
    private PositionDoorController posHandler;

    // Initialization of posHandler object
    void Start()
    {
        posHandler = GameObject.FindObjectOfType(typeof(PositionDoorController)) as PositionDoorController;
    }
    
    // Disables the collider of the position in order to be collided by just one player
    // and calls the TriggerHandler function of PositionDoorController script that handles the collision
    void OnTriggerEnter2D(Collider2D collider)
    {
        GameObject position = this.gameObject;

        position.GetComponent<Collider2D>().enabled = false;
        posHandler.TriggerHandler(collider);
    }
}