using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Photon.Pun;
using ExitGames.Client.Photon;
using Photon.Realtime;
using Random = System.Random;
using UnityEngine.SceneManagement;

// Handles the Code Door (the blue one) and its features
public class CodeDoorController : MonoBehaviourPunCallbacks, IOnEventCallback
{
    // Panels
    [SerializeField]
    private GameObject infoCodePanel;
    [SerializeField]
    private GameObject codePanel;
    [SerializeField]
    private GameObject wrongCodePanel;
    [SerializeField]
    private GameObject pauseButton;

    private GameObject door;
 
    // Clips handle variables
    private AudioClip[] clips;
    private const string resourcePath = "Audio/CodeDoor/";
    private int numberOfBips;

    // Inserted code numbers
    [SerializeField]
    private Text no1Text;
    [SerializeField]
    private Text no2Text;
    [SerializeField]
    private Text no3Text;
    [SerializeField]
    private Text no4Text;

    // Arrays that contain the right code and the one inserted by the player
    private int[] codeList;
    private string code;
    private int[] insertCode;
    private int codeCounter;

    private bool isCorrect;

    // Players Game Objects and deaf player's PhotonView
    private GameObject blindPlayer;
    private GameObject deafPlayer;
    private PhotonView deafPV;

    private Random rnd;

    // Sounds of door collision and destruction
    private AudioClip toctocClip;
    private AudioSource toctocAudio;
    private AudioClip explosionClip;
    private AudioSource explosionSound;

    // Constants for Raise Event method
    private const int PLAY_MUSIC = 7;
    private const int ENABLE_PLAYERS_MOVEMENTS = 17;
    private const int DISABLE_PLAYERS_MOVEMENTS = 18;
    private const int DESTROY_DOOR = 12;

    // Initializes some variables
    void Start()
    {
        clips = Resources.LoadAll<AudioClip>(resourcePath);

        codeList = new int[4];
        insertCode = new int[4];
        codeCounter = 0;

        isCorrect = true;

        toctocClip = Resources.Load<AudioClip>("Audio/Toctoc") as AudioClip;
        toctocAudio = GetComponent<AudioSource>();
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

    // Raises Events based on the constant used
    private void SetRaiseEvent(byte constant)
    {
        object[] options = new object[] {};
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions{ Receivers = ReceiverGroup.All };
        PhotonNetwork.RaiseEvent(constant, options, raiseEventOptions, SendOptions.SendReliable);
    }

    // Handles collision with the Code Door
    public void CollisionHandler(Collision2D collision)
    {   
        // At this time all the players should be in the labyrinth
        GetPlayers();

        SetTileSoundState(false);

        if (!deafPV.IsMine)
        {
            toctocAudio.PlayOneShot(toctocClip, 0.7f);
        }

        // Disables player's movements
        SetRaiseEvent(DISABLE_PLAYERS_MOVEMENTS);

        door = collision.otherCollider.gameObject;

        // Only the deaf player can see the info panel
        if (deafPV.IsMine)
        {
            infoCodePanel.SetActive(true);
        }
    }

    // Finds the two tiles in the maze in order to mute/unmute their sounds
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

    // Activates the right panels and starts the Coroutine for PlayMusic()
    private void StartPlayingMusic()
    {
        isCorrect = true;
        infoCodePanel.SetActive(false);
        wrongCodePanel.SetActive(false);
        codePanel.SetActive(true);

        SetRaiseEvent(PLAY_MUSIC);
    }

    // Handles the events raised with RaiseEvent
    public void OnEvent(EventData photonEvent)
    {
        byte eventCode = photonEvent.Code;

        if (eventCode == PLAY_MUSIC)
        {
            StartCoroutine(PlayMusic());
        }
        if (eventCode == ENABLE_PLAYERS_MOVEMENTS)
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
        else if (eventCode == DESTROY_DOOR)
        {
            if (!deafPV.IsMine)
            {
                explosionSound.PlayOneShot(explosionClip);
            }
            
            PhotonNetwork.Destroy(this.door);
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

    // Gets 4 random numbers for 4 different audios that will form the final code to insert
    private IEnumerator PlayMusic()
    {  
        AudioSource temp = GetComponent<AudioSource>();
        
        for (int i = 0; i < 4; i++)
        {
            numberOfBips = rnd.Next(1, 7);
            Debug.Log(i + " clip number: " + numberOfBips);

            temp.clip = clips[numberOfBips-1];

            // Only the blind player can hear the code
            if (deafPV.IsMine)
            {
                temp.mute = true;
            }
            else 
            {
                temp.mute = false;
                temp.volume = 0.7f;
                temp.Play();
            }

            codeList[i] = numberOfBips;
            
            while (temp.isPlaying)
            {
                yield return null;
            }
            // There's 1.5s between an audio an another
            yield return new WaitForSecondsRealtime(1.5f);
        }

        CreateCode();
    }

    // Creates the final right code to guess
    private void CreateCode()
    {
        code = codeList[0].ToString() + codeList[1].ToString() + codeList[2].ToString() + codeList[3].ToString();
        Debug.Log("code [step 1 - just created]: " + code);
    }

    // Creates the inserted code based on the buttons the player presses
    private void ButtonPressed()
    {
        if (codeCounter < 4)
        {
            string buttonName = EventSystem.current.currentSelectedGameObject.name;
        
            if (buttonName == "Button1")
            {
                insertCode[codeCounter] = 1;
            }
            else if (buttonName == "Button2")
            {
                insertCode[codeCounter] = 2;
            }
            else if (buttonName == "Button3")
            {
                insertCode[codeCounter] = 3;
            }
            else if (buttonName == "Button4")
            {
                insertCode[codeCounter] = 4;
            }
            else if (buttonName == "Button5")
            {
                insertCode[codeCounter] = 5;
            }
            else if (buttonName == "Button6")
            {
                insertCode[codeCounter] = 6;
            }                                    

            codeCounter++;

            TextUpdate();
        }
    }

    // Updates the code on the screen
    private void TextUpdate()
    {
        no1Text.text = insertCode[0].ToString();
        no2Text.text = insertCode[1].ToString();
        no3Text.text = insertCode[2].ToString();
        no4Text.text = insertCode[3].ToString();
    }

    // Handles the code submission based on the two arrays
    // If the numbers at the same index are equals for all 4 indexes then the code submitted is right
    private void SubmitCode()
    {
        for (int i=0; i<4; i++)
        {
            if (insertCode[i] != codeList[i])
            {
                isCorrect = false;
                break;
            }
        }

        if (isCorrect)
        {
            Debug.Log("RIGHT CODE!");
            codePanel.SetActive(false);
            
            Debug.Log("Door name TO BE DESTROYED: " + door.name);
            SetRaiseEvent(DESTROY_DOOR);
            SetRaiseEvent(ENABLE_PLAYERS_MOVEMENTS);

            SetTileSoundState(true);

            CancelCode();
        }
        else
        {
            Debug.Log("WRONG CODE!");
            CancelCode();
            codePanel.SetActive(false);
            wrongCodePanel.SetActive(true);
        }
    }

    // "Cancels" the code on the UI in order to have it at "0000" everytime it has to be inserted again
    private void CancelCode()
    {
        no1Text.text = "0";
        no2Text.text = "0";
        no3Text.text = "0";
        no4Text.text = "0";
        
        codeCounter = 0;

        for (int i=0; i<insertCode.Length; i++)
        {
            insertCode[i] = 0;
        }
    }
}