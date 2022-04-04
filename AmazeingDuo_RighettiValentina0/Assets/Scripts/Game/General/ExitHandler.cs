using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Handles the collision with the arrow sprite 
// The arrow represents the maze exit
public class ExitHandler : MonoBehaviour
{
    private GeneralSettings exitHandler;

    void Start()
    {
        exitHandler = GameObject.FindObjectOfType(typeof(GeneralSettings)) as GeneralSettings;
    }
    
    // Calls the method of GeneralSettings script in order to handle the exit
    void OnTriggerEnter2D(Collider2D collider)
    {
        exitHandler.ExitHandling(collider);
    }
}