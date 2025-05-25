using UnityEngine;

public class GlobalManager : MonoBehaviour
{
    public static GlobalManager Instance;

    // player's role in the game
    public string PlayerRole { get; set; }

    //player ID in EVA OR IVA
    public string PlayerID { get; set; }

    //optional- not yet used
    public string PlayerName { get; private set; } // Player's name - not yet defined anywhere but can be


    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetPlayerRole(string role, string id)
    {
        PlayerRole = role;
        PlayerID = id;

        Debug.Log($"[GameManager] Assigned Role: {PlayerRole}, ID: {PlayerID}");
    }


}
