using Photon.Pun;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = System.Random;

// Instantiates doors and players based on the level
// Every level has a different number of obstacles and different positions for obstacles and players
public class GameSetup : MonoBehaviourPunCallbacks
{
    // Players' positions
    private Vector3 deafPlayerStartPosition;
    private Vector3 blindPlayerStartPosition;

    // Arrays of Position Door (the orange one) positions (based on the level)
    // Position Doors can be placed horizontally or vertically
    // Position Doors are in Easy and Hard levels
    private static Vector3[] easylevPosDoorPositions =
    {
        new Vector3(120, 185, -1),
        new Vector3(-120, 231, -1),
        new Vector3(-140, -15, -2),
        new Vector3(-262, 144, -2)
    };
    private static Vector3[] hardlevPosDoorPositions =
    {
        new Vector3(598, 480, -2),
        new Vector3(198, 160, -2),
        new Vector3(-282, 400, -2),
        new Vector3(-24, -167, -2),
        new Vector3(376, 153, -2),
        new Vector3(617, 313, -2)
    };
    // Paths for horizontal and vertical Position Door
    private const string horizontalPosdoorPath = "Prefabs/PositionDoorHorizontal";
    private const string verticalPosdoorPath = "Prefabs/PositionDoorVertical";

    // Arrays of Code Door (the blue one) positions (based on the level)
    // Code Doors can be placed horizontally or vertically
    // Code Doors are in Easy and Hard levels
    private static Vector3[] easylevCodeDoorPositions =
    {
        new Vector3(-407, 79, -1),
        new Vector3(-87, -200, -1),
        new Vector3(-63, 224, -1),
        new Vector3(-262, -96, -1)
    };
    private static Vector3[] hardlevCodeDoorPositions = 
    {
        new Vector3(631, 8, -1),
        new Vector3(151, 488, -1),
        new Vector3(-89, -392, -1),
        new Vector3(296, -87, -1),
        new Vector3(-103, 73, -1),
        new Vector3(776, 73, -1)
    };
    // Paths for horizontal and vertical Position Door
    private const string horizontalCodedoorPath = "Prefabs/CodeDoorHorizontal";
    private const string verticalCodedoorPath = "Prefabs/CodeDoorVertical";

    // Final door position and door's GameObject
    private Vector3 doorPosition;
    private GameObject door;

    // Temporary variables for in-between calculation
    private int posDoorTemp;
    private int codeDoorTemp;
    private int temp;

    // Arrays of Tiles positions (based on the level)
    // Tiles are in Medium and Hard levels
    private static Vector3[] mediumlevTilesPositions =
    {
        new Vector3(48, -415, -1),
        new Vector3(208, -94, -1),
        new Vector3(450, 63, -1),
        new Vector3(608, 224, -1),
        new Vector3(607, -177, -1)
    };
    private static Vector3[] hardlevTilesPositions =
    {
        new Vector3(-304, 246, -1),
        new Vector3(-145, -31, -1),
        new Vector3(257, -272, -1),
        new Vector3(335, 49, -1),
        new Vector3(497, -513, -1),
        new Vector3(175, 367, -1),
        new Vector3(736, 288, -1),
        new Vector3(704, 608, -1)
    };
    // Tile's path and temporary variables
    private const string tilePath = "Prefabs/Tile";
    private int tilePos1;
    private int tilePos2;

    private Random rnd;
    
    void Start()
    {   
        // Sets the right player's positions based on the level
        SetPlayersSpawningPositions();

        // Instantiates the character chosen by the Master
        if (PhotonNetwork.IsMasterClient)
        {
            if (PlayerPrefs.GetString("Role") == "deaf")
            {
                PhotonNetwork.Instantiate("Prefabs/DeafPlayer", deafPlayerStartPosition, Quaternion.identity);
            }
            else if (PlayerPrefs.GetString("Role") == "blind")
            {
                PhotonNetwork.Instantiate("Prefabs/BlindPlayer", blindPlayerStartPosition, Quaternion.identity);
            }
        }
        // Instantiates the character not chosen by the Master for the Client
        else if (!PhotonNetwork.IsMasterClient)
        {
            if (PlayerPrefs.GetString("Role") == "deaf")
            {
                PhotonNetwork.Instantiate("Prefabs/BlindPlayer", blindPlayerStartPosition, Quaternion.identity);
            }
            else if (PlayerPrefs.GetString("Role") == "blind")
            {
                PhotonNetwork.Instantiate("Prefabs/DeafPlayer", deafPlayerStartPosition, Quaternion.identity);
            }
        }

        // Initialisation of variables
        temp = -1;
        doorPosition = new Vector3(0, 0, -2);

        // Initialises Random rnd with the seed that players have saved as PlayerPrefs
        // The seed is set as the same for both players
        if (PlayerPrefs.HasKey("seed"))
        {
            rnd = new Random(PlayerPrefs.GetInt("seed"));
            Debug.Log("GAME SETUP set seed is " + PlayerPrefs.GetInt("seed"));
        }
        else    // Should never be called
        {
            Debug.Log("Something went wrong - START seed is 0");
            rnd = new Random(0);
        }

        // Handles the doors and tiles istantiation based on the level
        if (PhotonNetwork.IsMasterClient)
        {
            if ((SceneManager.GetActiveScene().name == "EasyLevel") || (SceneManager.GetActiveScene().name == "HardLevel"))
            {
                DoorsHandler();
            }

            if ((SceneManager.GetActiveScene().name == "MediumLevel") || (SceneManager.GetActiveScene().name == "HardLevel"))
            {
                TilesHandler();
            }
        }
    }

    // Sets the right players' positions based on the level name
    // Positions change based on how the maze is built
    private void SetPlayersSpawningPositions()
    {
        if (SceneManager.GetActiveScene().name == "EasyLevel")
        {
            deafPlayerStartPosition = new Vector3(-300, -280, -1);
            blindPlayerStartPosition = new Vector3(-460, -280, -1);
        }
        else if (SceneManager.GetActiveScene().name == "MediumLevel")
        {
            deafPlayerStartPosition = new Vector3(-112, -421, -1);
            blindPlayerStartPosition = new Vector3(287, -417, -1);
        }
        else if (SceneManager.GetActiveScene().name == "HardLevel")
        {
            deafPlayerStartPosition = new Vector3(-300, -436, -1);
            blindPlayerStartPosition = new Vector3(-166, -518, -1);
        }
    }

    // Returns the Blind Player starting positions
    public Vector3 GetBlindPlayerSpawningPositions()
    {
        return blindPlayerStartPosition;
    }

    // Returns the Deaf Player starting positions
    public Vector3 GetDeafPlayerSpawningPosition()
    {
        return deafPlayerStartPosition;
    }

    // Instantiates the Tiles based on level and positions
    // There are 2 Tiles for each level
    private void TilesHandler()
    {
        if (SceneManager.GetActiveScene().name == "MediumLevel")
        {
            tilePos1 = rnd.Next(0, 5);
            tilePos2 = rnd.Next(0, 5);

            while (tilePos1 == tilePos2)
            {
                tilePos1 = rnd.Next(0, 5);
                tilePos2 = rnd.Next(0, 5);
            }
            
            PhotonNetwork.Instantiate(tilePath, mediumlevTilesPositions[tilePos1], Quaternion.identity);
            PhotonNetwork.Instantiate(tilePath, mediumlevTilesPositions[tilePos2], Quaternion.identity);
        }
        else if (SceneManager.GetActiveScene().name == "HardLevel")
        {
            tilePos1 = rnd.Next(0, 7);
            tilePos2 = rnd.Next(0, 7);

            while (tilePos1 == tilePos2)
            {
                tilePos1 = rnd.Next(0, 7);
                tilePos2 = rnd.Next(0, 7);
            }
            
            PhotonNetwork.Instantiate(tilePath, hardlevTilesPositions[tilePos1], Quaternion.identity);
            PhotonNetwork.Instantiate(tilePath, hardlevTilesPositions[tilePos2], Quaternion.identity);
        }
    }

    // Depending on the level the number of doors and their position change
    private void DoorsHandler()
    {
        if (SceneManager.GetActiveScene().name == "EasyLevel")
        {
            //1 Code Door
            codeDoorTemp = rnd.Next(0, 4);
            if (codeDoorTemp <= 1) { InstantiateDoors(easylevCodeDoorPositions, codeDoorTemp, horizontalCodedoorPath); }
            else if (codeDoorTemp > 1) { InstantiateDoors(easylevCodeDoorPositions, codeDoorTemp, verticalCodedoorPath); }
            //1 Position Door
            posDoorTemp = rnd.Next(0, 4);
            if (posDoorTemp <= 1) { InstantiateDoors(easylevPosDoorPositions, posDoorTemp, horizontalPosdoorPath); }
            else if (posDoorTemp > 1) { InstantiateDoors(easylevPosDoorPositions, posDoorTemp, verticalPosdoorPath); }
        }
        
        if (SceneManager.GetActiveScene().name == "HardLevel")
        {
            //2 Code Doors
            for (int i=0; i<2; i++)
            {
                codeDoorTemp = rnd.Next(0, 6);
                while (codeDoorTemp == temp)
                {
                    codeDoorTemp = rnd.Next(0, 6);
                }
                if (codeDoorTemp <= 2) { InstantiateDoors(hardlevCodeDoorPositions, codeDoorTemp, horizontalCodedoorPath); } 
                else if (codeDoorTemp > 2) { InstantiateDoors(hardlevCodeDoorPositions, codeDoorTemp, verticalCodedoorPath); }
                temp = codeDoorTemp;
            }
            // 1 Position Door
            posDoorTemp = rnd.Next(0, 6);
            if (posDoorTemp <= 2) { InstantiateDoors(hardlevPosDoorPositions, posDoorTemp, horizontalPosdoorPath); }
            else if (posDoorTemp > 2) { InstantiateDoors(hardlevPosDoorPositions, posDoorTemp, verticalPosdoorPath); }
        }
    }

    // Instantiates the doors depending on the right position defined in the DoorsHandler method
    private void InstantiateDoors(Vector3[] positions, int tempPos, string path)
    {
        doorPosition = positions[tempPos];
        door = Resources.Load(path) as GameObject;

        PhotonNetwork.Instantiate(path, doorPosition, Quaternion.identity);
    }
}