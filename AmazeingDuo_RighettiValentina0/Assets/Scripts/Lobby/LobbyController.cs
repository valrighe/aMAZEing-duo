using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Handles the Lobby scene behaviours/connections
public class LobbyController : MonoBehaviourPunCallbacks
{
    // Panels
    [SerializeField]
    private GameObject homePanel;
    [SerializeField]
    private GameObject lobbyConnectButton;
    [SerializeField]
    private GameObject lobbyPanel;
    [SerializeField]
    private GameObject mainPanel;

    // Player info
    [SerializeField]
    private InputField playerNameInput;
    [SerializeField]
    private Text playerName;

    // Room info
    private string roomName;
    private List<RoomInfo> roomListings;
    [SerializeField]
    private Transform roomsContainer;
    [SerializeField]
    private GameObject roomListingPrefab;

    // Called when Start Button is pressed in the main title panel
    public void OnClickStart()
    {
        homePanel.SetActive(false);
        mainPanel.SetActive(true);
    }

    // Called when the client is connected to the Master Server 
    // and ready for matchmaking and other tasks
    public override void OnConnectedToMaster()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        lobbyConnectButton.SetActive(true);
        roomListings = new List<RoomInfo>();

        if (PlayerPrefs.HasKey("NickName"))
        {
            if (PlayerPrefs.GetString("NickName") == "")
            {
                // Gives a random nickname to the player if he hasn't chosen one
                PhotonNetwork.NickName = "Player " + Random.Range(0, 1000);
            }
            else
            {
                PhotonNetwork.NickName = PlayerPrefs.GetString("NickName");
            }
        }
        else
        {
            PhotonNetwork.NickName = "Player " + Random.Range(0, 1000);
        }
        // Updates the text on screen with the player's name
        playerNameInput.text = PhotonNetwork.NickName;
    }

    // Sets the player's name as PlayerPrefs
    public void PlayerNameUpdate(string nameInput)
    {
        PhotonNetwork.NickName = nameInput;
        PlayerPrefs.SetString("NickName", nameInput);
    }

    // Called when Join Button is pressed to join the Lobby after inserting the name
    public void JoinLobbyOnClick()
    {
        mainPanel.SetActive(false);
        lobbyPanel.SetActive(true);
        PhotonNetwork.JoinLobby();
        // The "title" of the menu becomes the player's name
        playerName.text = PhotonNetwork.NickName;
    }

    // Called for any update of the room-listing while in a lobby (InLobby) on the Master Server
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        int tempIndex;
        foreach (RoomInfo room in roomList)
        {
            if (roomListings != null)
            {
                tempIndex = roomListings.FindIndex(ByName(room.Name));
            }
            else
            {
                tempIndex = -1;
            }
            if (tempIndex != -1)
            {
                roomListings.RemoveAt(tempIndex);
                Destroy(roomsContainer.GetChild(tempIndex).gameObject);
            }
            // The room is still open
            if (room.PlayerCount > 0)
            {
                roomListings.Add(room);
                ListRoom(room);
            }
        }
    }

    // Defines the Room criteria and determines whether the specified object meets those criteria
    // Returns true if so, false if not
    static System.Predicate<RoomInfo> ByName(string name)
    {
        return delegate (RoomInfo room)
        {
            Debug.Log("Predicate RoomInfo room name: " + room.Name);
            return room.Name == name;
        };
    }

    // Instantiation of the RoomListing Game Object that shows the list of open rooms
    void ListRoom(RoomInfo room)
    {
        GameObject tempListing = Instantiate(roomListingPrefab, roomsContainer);
        RoomButtonsHandle tempButton = tempListing.GetComponent<RoomButtonsHandle>();
        tempButton.SetRoom(room.Name, room.MaxPlayers, room.PlayerCount);
    }

    // Updates the room name if changed
    public void OnRoomNameChanged(string nameIn)
    {
        roomName = nameIn;
    }

    // Creates the room based on the option defined in RoomOptions and sets a random
    // seed as PlayerPrefs of the Master (= the player who created the room) 
    public void CreateRoom()
    { 
        Debug.Log("Creating new room");
        RoomOptions roomOps = new RoomOptions() 
        { 
            IsVisible = true, 
            IsOpen = true,
            MaxPlayers = 2,
            BroadcastPropsChangeToAll = true
        };

        int seed = Random.Range(0, 500);
        Debug.Log("CREATE ROOM seed for Master set to: " + seed);
        PlayerPrefs.SetInt("seed", seed);

        PhotonNetwork.CreateRoom(roomName, roomOps);
    }
    
    // Called when the server couldn't create a room 
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.Log("Tried to create a new room but failed, there must be already a room with the same name");
    }

    // Called when the BackButton in the LobbyPanel is clicked
    public void MatchmakingCancel()
    {
        mainPanel.SetActive(true);
        lobbyPanel.SetActive(false);
        PhotonNetwork.LeaveLobby();
    }

    // Called when the X button is clicked
    private void QuitApplication()
    {
        Debug.Log("APPLICATION QUIT");
        Application.Quit();
    }
}