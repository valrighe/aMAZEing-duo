using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Handles the Spikes behaviour
// The player that collides with them returns to the starting position
public class SpikesHandler : MonoBehaviour
{
    private GameObject blindPlayer;
    private GameObject deafPlayer;

    private GameSetup getPlayerStartPosition;

    // Initialization of the getPlayerStartPosition object
    void Start()
    {
        getPlayerStartPosition = GameObject.FindObjectOfType(typeof(GameSetup)) as GameSetup;
    }

    // Finds the player's Game Objects in scene
    private void GetPlayers()
    {
        blindPlayer = GameObject.Find("BlindPlayer(Clone)");
        deafPlayer = GameObject.Find("DeafPlayer(Clone)");
    }

    // Checks the player that collided with a Spike and calls the right function
    // to take it to the starting position
    void OnCollisionEnter2D(Collision2D collision)
    {
        // At this time both the players should be in game
        GetPlayers();
        
        if (collision.gameObject.name == blindPlayer.name)
        {
            blindPlayer.transform.position = getPlayerStartPosition.GetBlindPlayerSpawningPositions();
        }
        else if (collision.gameObject.name == deafPlayer.name)
        {
            deafPlayer.transform.position = getPlayerStartPosition.GetDeafPlayerSpawningPosition();
        }
    }
}