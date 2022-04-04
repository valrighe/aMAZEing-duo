using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

// Handles the collision with the maze walls 
public class LabyrinthCollisionHandler : MonoBehaviour
{
    // Sound played when the Blind Player hits the maze walls
    private AudioClip bumpClip;
    private AudioSource bumpAudio;

    private PhotonView blindPhotonView;
    private GameObject blindPlayer;

    // Initializes the variables
    void Start()
    {
        bumpClip = Resources.Load<AudioClip>("Audio/Bump") as AudioClip;
        bumpAudio = GetComponent<AudioSource>();
        bumpAudio.clip = bumpClip;
        bumpAudio.volume = 0.7f;
    }
    
    // When there's a collision gets the Blind Player Photon View and plays a sound
    void OnCollisionEnter2D(Collision2D collision)
    {
        blindPlayer = GameObject.Find("BlindPlayer(Clone)");
        blindPhotonView = blindPlayer.GetComponent<PhotonView>();

        if (collision.gameObject.name == "BlindPlayer(Clone)" && blindPhotonView.IsMine)
        {
            bumpAudio.Play();
        }
    }
}