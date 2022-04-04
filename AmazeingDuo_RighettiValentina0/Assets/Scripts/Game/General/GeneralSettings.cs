using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using System.IO;
using ExitGames.Client.Photon;
using Photon.Realtime;

// Handles the main features of every level
public class GeneralSettings : MonoBehaviourPunCallbacks, IOnEventCallback
{
    // Time update variables
    [SerializeField]
    private Text timeText;
    private float seconds;
    private float minutes;
    private bool timeStop;

    // Players
    private GameObject blindPlayer;
    private GameObject deafPlayer;
    
    // Exit handling variables/panels
    private int countExit;
    [SerializeField]
    private Text totalTimeText;
    [SerializeField]
    private GameObject endGamePanel;
    [SerializeField]
    private GameObject settingsPanelMaster;
    [SerializeField]
    private GameObject waitingForMasterPanel;
    private string levelName;
    [SerializeField]
    private Text deafPlayerName;
    [SerializeField]
    private Text blindPlayerName;
    private string pressedButtonName;
    private string[] playerList;
    [SerializeField]
    private GameObject playAgainButton;
    private int nextSeed;
    private string nextRole;

    [SerializeField]
    private GameObject pausePanel;
    private bool getPlayersController;

    // Text that warn the players if one of them leaves the game
    [SerializeField]
    private Text infosDisplay;

    private TileHandler tileHandler;

    // Constants for Raise Event function
    private const int PAUSE = 1;
    private const int BACK = 2;
    private const int TIME_UPDATE = 5;
    private const int END_REACHED = 15;
    private const int OPEN_SETTINGS_ENDGAME = 22;
    private const int LEAVE_ROOM = 23;
    private const int SET_SETTINGS = 25;

    // Initialization of some variables
    void Start()
    {   
        seconds = 0;
        minutes = 0;
        timeStop = false;

        countExit = 0;

        playerList = new string[2];
        
        getPlayersController = true;
    }

    // Updates time count
    void Update()
    {
        if (PhotonNetwork.IsMasterClient)
        {   
            SetRaiseEvent(TIME_UPDATE);
        }
    }

    // Raises events based on the constant chosen
    // The events are risen for both of the players
    private void SetRaiseEvent(byte constant)
    {
        object[] options = new object[] {};
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions{ Receivers = ReceiverGroup.All };
        PhotonNetwork.RaiseEvent(constant, options, raiseEventOptions, SendOptions.SendReliable);
    }

    // Mutes/unmutes the Tiles sounds depending on what's going on
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

    // Handles the Pause panel
    private void PauseGame()
    {
        SetTileSoundState(false);
        SetRaiseEvent(PAUSE);
    }

    // Gets the two players' Game Objects
    private IEnumerator GetPlayers()
    {
        while (getPlayersController)
        {
            if ((blindPlayer != null) && (deafPlayer != null))
            {
                getPlayersController = false;
            }
            else 
            {
                blindPlayer = GameObject.Find("BlindPlayer(Clone)");
                deafPlayer = GameObject.Find("DeafPlayer(Clone)");
                // If the players aren't in the scene the method waits
                // 1s and searches again
                yield return new WaitForSecondsRealtime(1f);
            }
        }
    }

    // Handles the events raised with RaiseEvent
    public void OnEvent(EventData photonEvent)
    {
        byte eventCode = photonEvent.Code;

        if (eventCode == PAUSE)
        {
            StartCoroutine(GetPlayers());

            blindPlayer.SetActive(false);
            deafPlayer.SetActive(false);
            
            Debug.Log("Pausing game");

            timeStop = true;
            pausePanel.SetActive(true);
        }
        else if (eventCode == BACK)
        {
            blindPlayer.SetActive(true);
            deafPlayer.SetActive(true);
            
            Debug.Log("Going back to game");

            timeStop = false;
            pausePanel.SetActive(false);
        }
        else if (eventCode == TIME_UPDATE)
        {
            if (!timeStop)
            {
                seconds += Time.deltaTime;
                if (seconds >= 59)
                {
                    seconds = 0;
                    seconds += Time.deltaTime;

                    minutes += 1;
                }
                timeText.text = minutes + ":" + Mathf.Round(seconds);
            }
        }
        else if (eventCode == END_REACHED)
        {
            countExit++;
            Debug.Log("TRIGGER HANDLING countExit = " + countExit);

            if (countExit == 2)
            {
                SetTileSoundState(false);

                timeStop = true;
                Debug.Log("END!");

                endGamePanel.SetActive(true);
                totalTimeText.text = minutes + ":" + Mathf.Round(seconds);

                if (SceneManager.GetActiveScene().name == "EasyLevel")
                {
                    FinalTimeControl("minutesEasyLevel", "secondsEasyLevel");
                }
                else if (SceneManager.GetActiveScene().name == "MediumLevel")
                {
                    FinalTimeControl("minutesMediumLevel", "secondsMediumLevel");
                }
                else if (SceneManager.GetActiveScene().name == "HardLevel")
                {
                    FinalTimeControl("minutesHardLevel", "secondsHardLevel");
                }

                if (PhotonNetwork.IsMasterClient)
                {
                    playAgainButton.SetActive(true);
                }
            }
        }
        else if (eventCode == OPEN_SETTINGS_ENDGAME)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                settingsPanelMaster.SetActive(true);
            }
            else 
            {
                waitingForMasterPanel.SetActive(true);
            }
        }
        else if (eventCode == LEAVE_ROOM)
        {
            timeStop = true;
            infosDisplay.text = "Your friend has left the game! You'll be taken back to the lobby.";
            StartCoroutine(LeaveRoomAfterFriend());
        }
        else if (eventCode == SET_SETTINGS)
        {
            // Sets a new seed for both the players
            object[] data = (object[])photonEvent.CustomData;
            int finalSeed = (int)data[0];
            string finalRole = (string)data[1];

            Debug.Log("SET SEED for client at " + finalSeed);
            PlayerPrefs.SetInt("seed", finalSeed);
            PlayerPrefs.SetString("Role", finalRole);
            Debug.Log("SET SEED for client AFTER at " + finalSeed);
            Debug.Log("SET MASTER ROLE for client AFTER: " + finalRole);

            LoadGame();
        }
    }

    // Loads the level chosen by the Master after the end of the played level
    private void LoadGame()
    {
        if (PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom.PlayerCount == 2)
        {
            PhotonNetwork.CurrentRoom.IsOpen = false;
            PhotonNetwork.LoadLevel(levelName);
        }
    }

    // Takes the player who is left in the room to the lobby
    // It takes 5s so the player left can see the message on screen while still in the room
    private IEnumerator LeaveRoomAfterFriend()
    {
        yield return new WaitForSecondsRealtime(5);
        
        PhotonNetwork.Disconnect();
        PhotonNetwork.LoadLevel("Lobby");
    }

    // Disables the Pause Panel and takes back to the game
    private void GoBackToGame()
    {
        Debug.Log("Going back to game");
        SetTileSoundState(true);

        SetRaiseEvent(BACK);
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

    // Called when a player leaves the room
    // Can be called during the game or at its end
    private void LeaveGame()
    {
        PhotonNetwork.CurrentRoom.IsOpen = false;
        Debug.Log("LEAVING THE GAME - REJOINING THE ROOM");

        SetTileSoundState(false);

        // The event is raised for the other player only
        object[] options = new object[] {};
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions{ Receivers = ReceiverGroup.Others };
        PhotonNetwork.RaiseEvent(LEAVE_ROOM, options, raiseEventOptions, SendOptions.SendReliable);
        
        StartCoroutine(LeaveRoomFirst());
    }

    // Called when a player wants to leave the room
    private IEnumerator LeaveRoomFirst()
    {
        yield return new WaitForSecondsRealtime(1);

        // The player must be disconnected in order to join again a room
        PhotonNetwork.Disconnect();
        PhotonNetwork.LoadLevel("Lobby");
    }

    // Called when the players get the Position Door code wrong
    // Adds 10 seconds to the time
    public void AddSeconds()
    {
        if (seconds <= 50)
        {
            seconds = seconds + 10;
        }
        else
        {
            //ES with seconds = 51. Have to add 9s and get to the next min + 1s
            float tempSec = 10 - (60 - seconds);    //tempSec = 1 

            seconds = seconds + (60 - seconds);     //seconds = 51 + 9 = 60
            seconds = 0;
            minutes += 1;
            seconds += tempSec;                     //minutes:1
        }
    }

    // Handles the moment in which on player reaches the exit of the maze
    public void ExitHandling(Collider2D collider)
    {
        GameObject player = collider.gameObject;
        Debug.Log("EXIT REACHED by " + player.name);
        PhotonNetwork.Destroy(player);
        
        SetRaiseEvent(END_REACHED);
    }

    // Check if there's a final time already set as PlayerPrefs
    // If so there's a control between the saved time and the new one
    private void FinalTimeControl(string min, string sec)
    {
        if (PlayerPrefs.HasKey(sec) && PlayerPrefs.HasKey(min))
        {
            if (PlayerPrefs.GetFloat(min) > minutes)
            {
                PlayerPrefs.SetFloat(min, minutes);
                PlayerPrefs.SetFloat(sec, Mathf.Round(seconds));
            }
            else if (PlayerPrefs.GetFloat(min) == minutes)
            {
                if (PlayerPrefs.GetFloat(sec) > seconds)
                {
                    PlayerPrefs.SetFloat(sec, Mathf.Round(seconds));
                }
            }
        }
        else 
        {
            PlayerPrefs.SetFloat(min, minutes);
            PlayerPrefs.SetFloat(sec, Mathf.Round(seconds));
        }

        PlayerPrefs.Save();
    }

    // Called when the Master decides to play another round
    private void PlayAgain()
    {
        Debug.Log("OPENING NEXT GAME SETTINGS");
        SetRaiseEvent(OPEN_SETTINGS_ENDGAME);
    }

    // Character selection at the end of the level
    // Activated if the Master chooses to play another level
    private void CharacterSelection()
    {
        GameObject pressedButton = EventSystem.current.currentSelectedGameObject;
        pressedButtonName = pressedButton.name;

        int i = 0;
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            playerList[i] = player.NickName;
            i++;
        }

        // Updates the players' names under the chosen characters
        // and sets the new roles as PlayerPrefs
        if (PhotonNetwork.IsMasterClient)
        {
            if (pressedButtonName == "DeafPlayerButton")
            { 
                deafPlayerName.text = playerList[0];   
                blindPlayerName.text = playerList[1];
                PlayerPrefs.SetString("Role", "deaf");
            }
            else if (pressedButtonName == "BlindPlayerButton")
            {
                blindPlayerName.text = playerList[0];
                deafPlayerName.text = playerList[1];
                PlayerPrefs.SetString("Role", "blind");
            }
        }
    }

    // Level selection at the end of the game
    // Activated if the Master chooses to play another level
    private void LevelSelection()
    {
        string buttonName = EventSystem.current.currentSelectedGameObject.name;
        
        if (PhotonNetwork.IsMasterClient)
        {
            if (buttonName == "EasyLevelButton")
            {
                levelName = "EasyLevel";
            }
            else if (buttonName == "MediumLevelButton")
            {
                levelName = "MediumLevel";
            }
            else if (buttonName == "HardLevelButton")
            {
                levelName = "HardLevel";
            }
        }
    }

    // Sets the new roles chosen by the Master as PlayerPrefs and start a new game
    // with the new characters and level chosen
    private void StartGame()
    {
        if (PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom.PlayerCount == 2)
        {
            nextSeed = Random.Range(0, 500);
            nextRole = PlayerPrefs.GetString("Role");
            
            object[] content = new object[] { nextSeed, nextRole };
            RaiseEventOptions raiseEventOptions = new RaiseEventOptions{ Receivers = ReceiverGroup.All };
            PhotonNetwork.RaiseEvent(SET_SETTINGS, content, raiseEventOptions, SendOptions.SendReliable);
        }
    }
}