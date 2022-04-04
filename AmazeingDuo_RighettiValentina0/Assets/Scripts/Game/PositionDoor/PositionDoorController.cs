using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using Random = System.Random;
using Photon.Pun;
using ExitGames.Client.Photon;
using Photon.Realtime;

// Handles the Position Door (the yellow one) behaviours 
public class PositionDoorController : MonoBehaviourPunCallbacks, IOnEventCallback
{
    // Panels
    [SerializeField]
    private GameObject infoPanel;
    [SerializeField]
    private GameObject codePanel;
    [SerializeField]
    private GameObject wrongCodePanel;
    [SerializeField]
    private GameObject pauseButton;

    // Players
    private GameObject blindPlayer;
    private GameObject deafPlayer;
    private PhotonView deafPV;
    
    // Door
    private GameObject door;

    // Clips for code guessing
    private AudioClip[] clips;
    private const string resourcePath = "Audio/PositionDoor/";
    private int clipNumber;
    private int buttonNumber;
    
    private bool isCollided;

    private GeneralSettings wrongAnswer;
    private int triggerCounter;

    // Positions to reach after the collision with the door
    [SerializeField]
    private GameObject position1;
    [SerializeField]
    private GameObject position2;
    private int temp1;
    private int temp2;
    //Positions of the yellow positions to reach
    private Vector3 temposition1;
    private Vector3 temposition2;

    // Vectors with the position for the yellow positions to reach
    // based on the level
    // Positions are in Easy and Hard levels
    private static Vector3[] easylevelpositions = 
    {
        new Vector3(-462, 120, -1),
        new Vector3(-380, -27, -1),
        new Vector3(-317, -135, -1),
        new Vector3(-462, -282, -1),
        new Vector3(-302, -282, -1),
        new Vector3(-142, -267, -1),
        new Vector3(19, -201, -1),
        new Vector3(32, 54, -1)
    };
    private static Vector3[] hardlevelpositions = 
    {
        new Vector3(-304, -194, -1),
        new Vector3(105, -24, -1),
        new Vector3(-151, -279, -1),
        new Vector3(23, -519, -1),
        new Vector3(416, -272, -1),
        new Vector3(569, -519, -1),
        new Vector3(823, -359, -1),
        new Vector3(816, -112, -1)
    };

    private Random rnd;

    // Sounds heard by the Blind Player when there's a collision
    // or destruction of the door and when a position is reached
    private AudioClip toctocClip;
    private AudioClip posReachedClip;
    private AudioSource toctocAudio;
    private AudioSource posReachedAudio;
    private AudioClip explosionClip;
    private AudioSource explosionSound;

    // Constants for Raise Event function
    private const int PLAYMUSIC_POSITIONDOOR = 8;
    private const int ADD_SECONDS = 9;
    private const int ENABLE_PLAYERS_MOVEMENTS = 10;
    private const int DISABLE_PLAYERS_MOVEMENTS = 11;
    private const int SET_POSITIONS = 16;
    private const int DESTROY_DOOR_REMOVE_POSITIONS = 20;

    // Initialization of the variables
    void Start()
    {
        isCollided = false;

        clips = Resources.LoadAll<AudioClip>(resourcePath);

        buttonNumber = 0;
        clipNumber = 0;
        wrongAnswer = GameObject.FindObjectOfType(typeof(GeneralSettings)) as GeneralSettings;
        triggerCounter = 0;
        
        temp1 = 0;
        temp2 = 0;
        temposition1 = new Vector3(0, 0, -1);
        temposition2 = new Vector3(0, 0, -1);

        toctocClip = Resources.Load<AudioClip>("Audio/Toctoc") as AudioClip;
        toctocAudio = GetComponent<AudioSource>();
        posReachedClip = Resources.Load<AudioClip>("Audio/PositionReachedSound") as AudioClip;
        posReachedAudio = GetComponent<AudioSource>();
        explosionClip = Resources.Load<AudioClip>("Audio/Explosion") as AudioClip;
        explosionSound = GetComponent<AudioSource>();

        // Gets the seed set for both players
        if (PlayerPrefs.HasKey("seed"))
        {
            rnd = new Random(PlayerPrefs.GetInt("seed"));
        }
        else    // Should never be called
        {
            Debug.Log("Something went wrong - START seed is 0");
            rnd = new Random(0);
        }
    }

    // Raises Events for both players 
    // Based on the constant used
    private void SetRaiseEvent(byte constant)
    {
        object[] options = new object[] {};
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions{ Receivers = ReceiverGroup.All };
        PhotonNetwork.RaiseEvent(constant, options, raiseEventOptions, SendOptions.SendReliable);
    }

    // Sets the yellow spots' positions based on the level
    // There are 2 positions for each level, spawn randomly
    private void SetPositions()
    {   
        while (temp1 == temp2)
        {
            temp1 = rnd.Next(0, 8);
            temp2 = rnd.Next(0, 8);
        }

        if (SceneManager.GetActiveScene().name == "EasyLevel")
        {
            temposition1 = easylevelpositions[temp1];
            temposition2 = easylevelpositions[temp2];
        }
        if (SceneManager.GetActiveScene().name == "HardLevel")
        {
            temposition1 = hardlevelpositions[temp1];
            temposition2 = hardlevelpositions[temp2];
        }

        position1.transform.position = temposition1;
        position2.transform.position = temposition2;
        position1.SetActive(true);
        position2.SetActive(true);

        SetTileSoundState(true);
    }

    // Handles the collision with the Position Door
    public void CollisionHandler(Collision2D collision)
    {
        // At this time the players should be in game
        GetPlayers();
        
        SetTileSoundState(false);

        if (!deafPV.IsMine)
        {
            toctocAudio.PlayOneShot(toctocClip, 0.7f);
        }

        if (!isCollided)
        {
            door = collision.otherCollider.gameObject;

            SetRaiseEvent(DISABLE_PLAYERS_MOVEMENTS);
            
            if (deafPV.IsMine)
            {
                infoPanel.SetActive(true);
            }
        }
        
        isCollided = true;
    }

    // Finds the player's Game Objects in scene
    private void GetPlayers()
    {
        if (blindPlayer == null || deafPlayer == null)
        {
            blindPlayer = GameObject.Find("BlindPlayer(Clone)");
            deafPlayer = GameObject.Find("DeafPlayer(Clone)");
            deafPV = deafPlayer.GetComponent<PhotonView>();
        }
    }

    // Called when the Let's Go Button is pressed
    // Activates the yellow positions to reach
    private void LetsGo()
    {
        infoPanel.SetActive(false);
        SetRaiseEvent(ENABLE_PLAYERS_MOVEMENTS);
        SetRaiseEvent(SET_POSITIONS);
    }

    // Handles the trigger of yellow positions after door collision
    public void TriggerHandler(Collider2D collider)
    {
        if (!deafPV.IsMine)
        {
            posReachedAudio.PlayOneShot(posReachedClip, 1.0f);
        }
        
        GameObject player = collider.gameObject;
        Debug.Log("POSITION TRIGGER player name: " + player.name);
        
        // Disables the players' movements in order to avoid involuntary collisions/triggers
        if (player.name == "DeafPlayer(Clone)")
        {
            deafPlayer.GetComponent<DeafPlayer>().enabled = false;
        }
        if (player.name == "BlindPlayer(Clone)")
        {
            blindPlayer.GetComponent<BlindPlayer>().enabled = false;
        }
        triggerCounter++;
        
        // When both players have reached their position a sound is played
        if (triggerCounter == 2)
        {
            RaisePlayEvent();
        }   
    }

    // Reactivates the panel with the 4 answers and plays a new audio
    private void RaisePlayEvent()
    {
        if (deafPV.IsMine)
        {
            codePanel.SetActive(true);
        }

        SetRaiseEvent(PLAYMUSIC_POSITIONDOOR);
    }

    // Handles the events raised with RaiseEvent
    public void OnEvent(EventData photonEvent)
    {
        byte eventCode = photonEvent.Code;

        if (eventCode == PLAYMUSIC_POSITIONDOOR)
        {
            SetTileSoundState(false);

            StartCoroutine(PlayAudio());
        }
        else if (eventCode == ADD_SECONDS)
        {
            wrongAnswer.AddSeconds();
        }
        else if (eventCode == ENABLE_PLAYERS_MOVEMENTS)
        {
            blindPlayer.GetComponent<BlindPlayer>().enabled = true;
            deafPlayer.GetComponent<DeafPlayer>().enabled = true;

            pauseButton.SetActive(true);
        }
        else if (eventCode == DISABLE_PLAYERS_MOVEMENTS)
        {
            blindPlayer.GetComponent<BlindPlayer>().enabled = false;
            deafPlayer.GetComponent<DeafPlayer>().enabled = false;

            pauseButton.SetActive(false);
        }
        else if (eventCode == SET_POSITIONS)
        {
            SetPositions();
        }
        else if (eventCode == DESTROY_DOOR_REMOVE_POSITIONS)
        {
            if (!deafPV.IsMine)
            {
                explosionSound.PlayOneShot(explosionClip);
            }
            
            PhotonNetwork.Destroy(this.door);

            position1.SetActive(false);
            position1.GetComponent<Collider2D>().enabled = true;
            position2.SetActive(false);
            position2.GetComponent<Collider2D>().enabled = true;

            isCollided = false;
            triggerCounter = 0;

            SetTileSoundState(true);
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

    // Sets the state (mute/unmute) of the Tiles in scene
    private void SetTileSoundState(bool state)
    {
        if (!(SceneManager.GetActiveScene().name == "MediumLevel") || (SceneManager.GetActiveScene().name == "HardLevel"))
        {
            return;
        }

        object[] tileHandlers = GameObject.FindObjectsOfType(typeof(TileHandler));
        foreach (TileHandler tileHandler in tileHandlers)
        {
            if (state) 
            {
                tileHandler.SetUnmuteBlip();
            } 
            else 
            {
                tileHandler.SetMuteBlip();
            }
        }
    }

    // Waits for 1.5s and then plays a random clip for the Blind Player
    // Called after both of the players have reached a yellow position
    private IEnumerator PlayAudio()
    {
        AudioSource temp = GetComponent<AudioSource>();
        clipNumber = rnd.Next(0, 4);
        yield return new WaitForSecondsRealtime(1.5f);

        temp.clip = clips[clipNumber];
        if (deafPV.IsMine)
        {
            temp.mute = true;
        }
        else 
        {
            temp.Play();
        }
        Debug.Log("PLAYAUDIO played clip number: " + clipNumber);
    }

    // Handles the Deaf Player choice depending on the sound played
    private void ButtonsHandler()
    {
        string buttonName = EventSystem.current.currentSelectedGameObject.name;
        
        if (buttonName == "CalandoButton")
        {
            buttonNumber = 0;
        }
        else if (buttonName == "CrescendoButton")
        {
            buttonNumber = 1;
        }
        else if (buttonName == "PiattoButton")
        {
            buttonNumber = 2;
        }
        else if (buttonName == "ZigzagButton")
        {
            buttonNumber = 3;
        } 
        Debug.Log("BUTTONS premuto: " + buttonNumber);
    }

    // Called when the Submit Button is pressed
    // Check if the answer inserted is correct, if so destroys the door
    private void Submit()
    {
        if (buttonNumber == clipNumber)
        {
            Debug.Log("RIGHT ANSWER!");
            codePanel.SetActive(false);

            SetRaiseEvent(ENABLE_PLAYERS_MOVEMENTS);

            Debug.Log("Door name TO BE DESTROYED: " + door.name);
            SetRaiseEvent(DESTROY_DOOR_REMOVE_POSITIONS);  
        }
        else
        {
            Debug.Log("WRONG ANSWER!");
            wrongCodePanel.SetActive(true);

            // If the answer is wrong 10s are added to the total time
            Debug.Log("Adding 10 seconds");
            SetRaiseEvent(ADD_SECONDS);
        }
    }

    // Called when the inserted answer is wrong and the button Play Again is pressed
    private void StartAgain()
    {
        wrongCodePanel.SetActive(false);
        RaisePlayEvent();
    }
}