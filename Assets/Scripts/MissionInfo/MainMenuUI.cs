using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using System.Collections;

public class MainMenuUI : MonoBehaviourPunCallbacks
{
    
    private string selectedRole;
    public TMP_Dropdown roleDropdown;
    public TextMeshProUGUI passwordErrorText;
    public TMP_InputField passwordInput;


    void Start()
    {
        PhotonNetwork.ConnectUsingSettings(); // connects to Photon
        StartCoroutine(EnableUIWhenReady());
    }

    IEnumerator EnableUIWhenReady()
    {
        // disable interaction until config is ready
        roleDropdown.interactable = false;
        passwordInput.interactable = false;
        passwordErrorText.text = "Loading configuration...";

        while (!ConfigLoader.IsLoaded)
            yield return null;

        roleDropdown.interactable = true;
        passwordInput.interactable = true;
        passwordErrorText.text = ""; 
    }



    public void OnRoleSelected()
    {
        int selectedIndex = roleDropdown.value;

        switch (selectedIndex)
        {
            case 0:
                JoinAsEVA();
                break;
            case 1:
                JoinAsIVA();
                break;
            default:
                Debug.LogError("Invalid role selected.");
                break;
        }
    }

    public void JoinAsEVA()
    {
        selectedRole = "EVA";
        JoinRoom();
    }

    public void JoinAsIVA()
    {
        selectedRole = "IVA";
        JoinRoom();
    }


    private void JoinRoom()
{
    string enteredPassword = passwordInput.text.ToLower();
    string requiredPassword = selectedRole.ToLower();       // "eva" or "iva"

    if (enteredPassword != requiredPassword)
    {
        passwordErrorText.text = "Incorrect password for " + selectedRole;
        return;
    }
    passwordErrorText.text = "";

    RoomOptions options = new RoomOptions { MaxPlayers = 4 };

    options.CustomRoomProperties = new ExitGames.Client.Photon.Hashtable
    {
        { "EVA_Count", 0 },
        { "IVA_Count", 0 }
    };

    options.CustomRoomPropertiesForLobby = new string[] { "EVA_Count", "IVA_Count" };

    PhotonNetwork.JoinOrCreateRoom("MoonMissionRoom", options, TypedLobby.Default);
}

    public override void OnJoinedRoom()
    {
        ExitGames.Client.Photon.Hashtable roomProps = PhotonNetwork.CurrentRoom.CustomProperties;

        // Determine current count for the selected role
        string countKey = selectedRole + "_Count";
        int currentCount = roomProps.ContainsKey(countKey) ? (int)roomProps[countKey] : 0;

        // Set local player nickname and GameManager values
        PhotonNetwork.LocalPlayer.NickName = $"{selectedRole}";


        // Assign in GameManager (make sure GameManager uses .SetPlayerRole(role, id))
        GlobalManager.Instance.SetPlayerRole(selectedRole, currentCount.ToString());
        ExitGames.Client.Photon.Hashtable playerProps = new ExitGames.Client.Photon.Hashtable
        {
            { "Role", selectedRole },
            { "ID", currentCount.ToString() }
        };
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerProps);

        // Only the master client updates room properties to avoid race conditions
        if (PhotonNetwork.IsMasterClient)
        {
            roomProps[countKey] = currentCount + 1;
            PhotonNetwork.CurrentRoom.SetCustomProperties(roomProps);
        }

        PhotonNetwork.LoadLevel("Mission"); // sync scene load
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Photon master server.");
    }
}
