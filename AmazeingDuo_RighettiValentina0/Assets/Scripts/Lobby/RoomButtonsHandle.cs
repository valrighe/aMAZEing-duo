using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

// Handles the connection to a room
public class RoomButtonsHandle : MonoBehaviour
{
    // Room infos
    [SerializeField]
    private Text nameText;
    [SerializeField]
    private Text sizeText;
    private string roomName;
    private int roomSize;
    private int playerCount;

    // Called when the player clicks on a room name to join it
    public void JoinRoomOnClick()
    {
        PhotonNetwork.JoinRoom(roomName);
    }

    // Sets the room infos and size
    public void SetRoom(string nameInput, int sizeInput, int countInput)
    {
        roomName = nameInput;
        roomSize = sizeInput;
        playerCount = countInput;
        nameText.text = nameInput;
        sizeText.text = countInput + "/2";
    }
}