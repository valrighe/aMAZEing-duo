using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using ExitGames.Client.Photon;
using Photon.Realtime;

// Handles the Tiles behaviour
// Tiles play 6 bips every 4 seconds. Players can cross them only if the Tiles
// don't play any sound
// Tiles can be heard only by the Blind Player and only if near the Tile
public class TileHandler : MonoBehaviourPunCallbacks, IOnEventCallback
{
    // Bip sound played by the Tile
    private AudioClip tileBlipClip;
    private AudioSource tileBlipAudio;
    
    // Variables for controls
    private bool keepPlaying;
    private bool getPlayersController;

    // Players
    private GameObject blindPlayer;
    private GameObject deafPlayer;
    private PhotonView deafPlayerPV;

    private GameSetup getPlayerStartPosition;

    // Constant for Raise Event method
    private const int START_BLIP = 26;

    // Initialization of some variables
    void Start()
    {
        tileBlipClip = Resources.Load<AudioClip>("Audio/BlipTileArea") as AudioClip;
        tileBlipAudio = GetComponent<AudioSource>();
        tileBlipAudio.clip = tileBlipClip;

        keepPlaying = true;
        getPlayersController = true;

        getPlayerStartPosition = GameObject.FindObjectOfType(typeof(GameSetup)) as GameSetup;

        StartCoroutine(DelayStart());
    }

    // Finds the player's Game Objects in scene
    private void GetPlayers()
    {
        blindPlayer = GameObject.Find("BlindPlayer(Clone)");
        deafPlayer = GameObject.Find("DeafPlayer(Clone)");
        deafPlayerPV = deafPlayer.GetComponent<PhotonView>();
    }

    // Calls the Coroutine of PlayBlip that plays the Tile's sound
    private void ActivateTiles()
    {
        StartCoroutine(PlayBlip());
    }

    // Waits until both the players are in the maze so the Tiles play
    // the sound in sync for both of them
    private IEnumerator DelayStart()
    {
        while (getPlayersController)
        {
            yield return new WaitForSecondsRealtime(2f);
            GetPlayers();

            if ((blindPlayer != null) && (deafPlayer != null))
            {
                getPlayersController = false;

                // Raises the event that activates the Tiles
                object[] options = new object[] {};
                RaiseEventOptions raiseEventOptions = new RaiseEventOptions{ Receivers = ReceiverGroup.All };
                PhotonNetwork.RaiseEvent(START_BLIP, options, raiseEventOptions, SendOptions.SendReliable);
            }
        }
    }

    // Handles the events raised with RaiseEvent
    public void OnEvent(EventData photonEvent)
    {
        byte eventCode = photonEvent.Code;

        if (eventCode == START_BLIP)
        {
            ActivateTiles();
        }
    }

    // Callback method for OnEvent
    private void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }

    // Callback method for OnEvent
    private void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    // Plays the bip sound and then waits for 4s before playing it again
    private IEnumerator PlayBlip()
    {
        while (keepPlaying)
        {
            if (deafPlayerPV.IsMine)
            {
                tileBlipAudio.mute = true;
            }

            tileBlipAudio.volume = 1.0f;
            tileBlipAudio.Play();

            while (tileBlipAudio.isPlaying)
            {
                yield return null;
            }
            yield return new WaitForSecondsRealtime(4f);
        }
    }
    
    // Handles the collision with the Tile
    void OnTriggerEnter2D(Collider2D collider)
    {
        if (tileBlipAudio.isPlaying)
        {
            if (collider.name == blindPlayer.name)
            {
                blindPlayer.transform.position = getPlayerStartPosition.GetBlindPlayerSpawningPositions();
            }
            else if (collider.name == deafPlayer.name)
            {
                deafPlayer.transform.position = getPlayerStartPosition.GetDeafPlayerSpawningPosition();
            }
        }
    }

    // Checks if a player is on the Tile while it's playing
    // If so the player is brought back to the starting position
    void OnTriggerStay2D(Collider2D collider)
    {
        if (tileBlipAudio.isPlaying)
        {
            if (collider.name == blindPlayer.name)
            {
                blindPlayer.transform.position = getPlayerStartPosition.GetBlindPlayerSpawningPositions();
            }
            else if (collider.name == deafPlayer.name)
            {
                deafPlayer.transform.position = getPlayerStartPosition.GetDeafPlayerSpawningPosition();
            }
        }
    }

    // Called from other scripts to mute the audio
    // Useful when Blind Player has to hear other sounds (ex. codes)
    public void SetMuteBlip()
    {
        if (!deafPlayerPV.IsMine)
        {
            tileBlipAudio.mute = true;
        }
    }

    // Called from other scripts to unmute the audio and hear the Tile again
    public void SetUnmuteBlip()
    {
        if (!deafPlayerPV.IsMine)
        {
            tileBlipAudio.mute = false;
        }
    }
}