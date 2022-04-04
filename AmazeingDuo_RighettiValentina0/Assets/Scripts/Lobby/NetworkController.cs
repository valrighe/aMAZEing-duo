using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Handles the starting connection to the region and the Master Server
public class NetworkController : MonoBehaviourPunCallbacks
{
    void Start()
    {
        // Connects to the EU server so the clients can surely connect to each other
        PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion = "eu";
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        // Tells if the operation has gone well
        Debug.Log("We are now connected to the " + PhotonNetwork.CloudRegion + " server!");
    }
}
