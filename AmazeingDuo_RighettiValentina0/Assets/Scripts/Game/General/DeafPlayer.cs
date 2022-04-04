using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

// Handles deaf player's movements and behaviours
public class DeafPlayer : MonoBehaviourPunCallbacks
{
    // Deaf player's features
    private float speed = 200.0f;
    private Rigidbody2D rb;
    Vector2 movement;
    private PhotonView playerPV;

    void Start()
    {
        // Getting all the player's needed components
        rb = GetComponent<Rigidbody2D>();
        playerPV = GetComponent<PhotonView>();
    }
    
    // Updates the player's movements
    void Update()
    {
        if (playerPV.IsMine)
        {
            PlayerMovement();
        }
    }
    
    // Handles the player's movements
    // Movements are handled with arrows/WASD
    private void PlayerMovement()
    {
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        if (Mathf.Abs(movement.x) > Mathf.Abs(movement.y))
        {
            movement.y = 0;
        }
        else
        {
            movement.x = 0;
        }

        rb.MovePosition(rb.position + movement * speed * Time.deltaTime);
    }

    // Updates the player's movements also on the network so everyone can see them
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(this.rb.position);
            Debug.Log(this.rb.position);
            stream.SendNext(this.rb.velocity);
            Debug.Log(this.rb.velocity);
        }
        else
        {
            rb.position = (Vector3) stream.ReceiveNext();
            Debug.Log("other position: " + rb.position);
            rb.velocity = (Vector3) stream.ReceiveNext();
            Debug.Log("other velocity: " + rb.velocity);
            

            float lag = Mathf.Abs((float) (PhotonNetwork.Time - info.SentServerTime));
            rb.position += rb.velocity * lag;
        }
    }
    
    // Allows to see the other player's updates movements realtime
    public void FixedUpdate()
    {
        if (!playerPV.IsMine)
        {
            rb.position = Vector3.MoveTowards(rb.transform.position, rb.position, Time.fixedDeltaTime);
        }
    }
}