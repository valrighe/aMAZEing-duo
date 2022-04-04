using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using ExitGames.Client.Photon;
using Photon.Realtime;

// Handles the room infos/behaviours and the level and character selection
public class RoomController : MonoBehaviourPunCallbacks, IOnEventCallback
{
    // Panels
    [SerializeField]
    private GameObject lobbyPanel;
    [SerializeField]
    private GameObject roomPanel;

    // Texts with best time for each level
    [SerializeField]
    private Text easyLevTimeText;
    [SerializeField]
    private Text mediumLevTimeText;
    [SerializeField]
    private Text hardLevTimeText;

    // Start game button
    [SerializeField]
    private GameObject startButton;

    // Room infos
    [SerializeField]
    private Text roomNameDisplay;
    [SerializeField]
    private Text roomInfoDisplay;

    // Players' infos
    [SerializeField]
    private Text deafPlayerName;
    [SerializeField]
    private Text blindPlayerName;
    [SerializeField]
    private Button deafPlayerButton;
    [SerializeField]
    private Button blindPlayerButton;
    private string[] playerList;
    public List<Player> playersPrefabList;

    // Levels' infos
    [SerializeField]
    private Button easyLevelButton;
    [SerializeField]
    private Button mediumLevelButton;
    [SerializeField]
    private Button hardLevelButton;
    private string levelName;

    private string pressedButtonName;

    // PlayerPrefs variables
    private int seed;
    private string role;

    // Constants for Raise Event function
    private const int CHANGENAME_ROOM_DEAFBUTTON = 3;
    private const int CHANGENAME_ROOM_BLINDBUTTON = 4;
    private const int SET_SEED = 24;

    // Initialization of some variables and update of best time for each level
    void Start()
    {
        playerList = new string[2];
        playersPrefabList = new List<Player>();

        if (PlayerPrefs.HasKey("minutesEasyLevel"))
        {
            easyLevTimeText.text = PlayerPrefs.GetFloat("minutesEasyLevel") + ":" + PlayerPrefs.GetFloat("secondsEasyLevel");
        }
        if (PlayerPrefs.HasKey("minutesMediumLevel"))
        {
            mediumLevTimeText.text = PlayerPrefs.GetFloat("minutesMediumLevel") + ":" + PlayerPrefs.GetFloat("secondsMediumLevel");
        }
        if (PlayerPrefs.HasKey("minutesHardLevel"))
        {
            hardLevTimeText.text = PlayerPrefs.GetFloat("minutesHardLevel") + ":" + PlayerPrefs.GetFloat("secondsHardLevel");
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

    // Called when a player enters a room, no matter if this client created it or simply joined 
    public override void OnJoinedRoom()
    {
        roomPanel.SetActive(true);
        lobbyPanel.SetActive(false);
        roomNameDisplay.text = PhotonNetwork.CurrentRoom.Name;

        CreatePlayersNameList();
        // playerList[0] is always the Master
        roomInfoDisplay.text = playerList[0] + " is setting the game options";

        if (PhotonNetwork.IsMasterClient)
        {
            startButton.SetActive(true);
        }
        else
        {
            startButton.SetActive(false);
        }
    }

    // Called when a remote player enteres the room
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        CreatePlayersNameList();
    }

    // Called when a remote player leaves the room or becomes inactive
    // Recreates the Players Name List in order to set the new Master
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        CreatePlayersNameList();
        roomInfoDisplay.text = playerList[0] + " is setting the game options";
        
        if (PhotonNetwork.IsMasterClient)
        {
            startButton.SetActive(true);
        }
    }

    // Starts the game only if both players are in the room
    // Sets seed and role as Master's PlayerPrefs
    private void StartGame()
    {   
        if (PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom.PlayerCount == 2)
        {
            seed = PlayerPrefs.GetInt("seed");
            role = PlayerPrefs.GetString("Role");

            object[] content = new object[] { seed, role };
            RaiseEventOptions raiseEventOptions = new RaiseEventOptions{ Receivers = ReceiverGroup.All };
            PhotonNetwork.RaiseEvent(SET_SEED, content, raiseEventOptions, SendOptions.SendReliable);
        }
    }

    // Loads the level chosen by the Master
    private void LoadGame()
    {
        if (PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom.PlayerCount == 2)
        {
            PhotonNetwork.CurrentRoom.IsOpen = false;
            PhotonNetwork.LoadLevel(levelName);
        }
    }

    // Waits 1 second and gets the player back in the Lobby
    public IEnumerator rejoinLobby()
    {
        yield return new WaitForSeconds(1);
        PhotonNetwork.JoinLobby();
    }

    // Called when the Back Button in the room is clicked
    // Takes back to the Lobby
    private void BackOnClick()
    {
        lobbyPanel.SetActive(true);
        roomPanel.SetActive(false);
        PhotonNetwork.LeaveRoom();
        PhotonNetwork.LeaveLobby();

        StartCoroutine(rejoinLobby());
    }

    // Creates a list of players nicknames and a list of players prefabs
    private void CreatePlayersNameList()
    {
        int i = 0;
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            playerList[i] = player.NickName;
            playersPrefabList.Add(player);
            i++;
        }
    }

    // Handles the character selection (by the Master)
    // Changes the text name under the characters on screen and sets the role as PlayerPrefs
    private void OnCharacterSelection()
    {
        CreatePlayersNameList();

        GameObject pressedButton = EventSystem.current.currentSelectedGameObject;
        pressedButtonName = pressedButton.name;

        if (PhotonNetwork.IsMasterClient)
        {
            if (pressedButtonName == "DeafPlayerButton")
            { 
                SetRaiseEvent(CHANGENAME_ROOM_DEAFBUTTON);
                PlayerPrefs.SetString("Role", "deaf");
            }
            else if (pressedButtonName == "BlindPlayerButton")
            {
                SetRaiseEvent(CHANGENAME_ROOM_BLINDBUTTON);
                PlayerPrefs.SetString("Role", "blind");
            }
        }
    }

    // Handles the events raised with RaiseEvent
    public void OnEvent(EventData photonEvent)
    {
        byte eventCode = photonEvent.Code;
        
        if (eventCode == CHANGENAME_ROOM_DEAFBUTTON)
        {
            deafPlayerName.text = playerList[0];   
            blindPlayerName.text = playerList[1];
        }
        else if (eventCode == CHANGENAME_ROOM_BLINDBUTTON)
        {
            blindPlayerName.text = playerList[0];
            deafPlayerName.text = playerList[1];
        }
        else if (eventCode == SET_SEED)
        {
            object[] data = (object[])photonEvent.CustomData;
            int finalSeed = (int)data[0];
            string finalRole = (string)data[1];

            PlayerPrefs.SetInt("seed", finalSeed);
            PlayerPrefs.SetString("Role", finalRole);

            LoadGame();
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

    // Handles the level selection (by the Master)
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
}