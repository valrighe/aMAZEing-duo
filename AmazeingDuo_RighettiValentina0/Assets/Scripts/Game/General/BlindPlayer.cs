using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;

// Handles blind player's movements and behaviours
public class BlindPlayer : MonoBehaviourPunCallbacks
{
    // Blind player's features
    private float speed = 200.0f;
    private Rigidbody2D rb;
    Vector2 movement;
    private PhotonView playerPV;

    // Blind player's camera and black panels that hide the labyrinth
    private GameObject blackLabyrinthPanel;

    void Start()
    {
        // Getting all the player's needed components
        rb = GetComponent<Rigidbody2D>();
        playerPV = GetComponent<PhotonView>();

        // Finds the black panel that hides the scene
        blackLabyrinthPanel = GameObject.Find("UI - Game (General)/Canvas/BlindPlayerLabyrinthBlackPanel");

        // Activates black panel and sets camera color to black
        if (playerPV.IsMine)
        {
            Camera.main.backgroundColor = Color.black;
            blackLabyrinthPanel.SetActive(true);
            // Sets its AudioListener as the one "correct" to use and disables the camera's one
            if ((SceneManager.GetActiveScene().name == "MediumLevel") || (SceneManager.GetActiveScene().name == "HardLevel"))
            {
                Camera.main.GetComponent<AudioListener>().enabled = false;
                GetComponent<AudioListener>().enabled = true;
            }
        }
    }
    
    // Updates the player's movements
    void Update()
    {
        // For developing purpose with Q/E the black panel that hides the 
        // labyrinth can be activated/disactivated
        if (Input.GetKeyDown(KeyCode.Q))
        {
            blackLabyrinthPanel.SetActive(false);
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            blackLabyrinthPanel.SetActive(true);
        }
        
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
            stream.SendNext(this.rb.velocity);
        }
        else
        {
            rb.position = (Vector3) stream.ReceiveNext();
            rb.velocity = (Vector3) stream.ReceiveNext();

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