using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using System.Collections;
using Photon.Pun.UtilityScripts;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviourPunCallbacks
{
    private string selectedRole;
    public TMP_Dropdown roleDropdown;
    public TextMeshProUGUI passwordErrorText;
    public TMP_InputField passwordInput;
    public Button submitButton;

    void Start()
    {
        Photon.Pun.PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion = "asia";
        PhotonNetwork.ConnectUsingSettings(); // connects to Photon
        StartCoroutine(EnableUIWhenReady());
    }

    IEnumerator EnableUIWhenReady()
    {
        roleDropdown.interactable = false;
        passwordInput.interactable = false;
        passwordErrorText.text = "Loading configuration...";

        while (!ConfigLoader.IsLoaded)
            yield return null;

        roleDropdown.interactable = true;
        passwordInput.interactable = true;
        passwordErrorText.text = ""; 

        passwordInput.onSubmit.AddListener(_ => OnRoleSelected());

    }

    private void SubmitLogin()
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
            case 2:
                JoinAsMCC();
                break;
            case 3:
                JoinAsLLM();
                break;
            case 4:
                JoinAsAdmin();
                break;
            default:
                Debug.LogError("Invalid role selected.");
                break;
        }
    }

    public void OnRoleSelected()
    {
        int selectedIndex = roleDropdown.value;

        if (selectedIndex == 0)
        {
            passwordErrorText.text = "Please select a role from the list.";
            return;
        }

        switch (selectedIndex)
        {
            case 1:
                JoinAsEVA();
                break;
            case 2:
                JoinAsIVA();
                break;
            case 3:
                JoinAsMCC();
                break;
            case 4:
                JoinAsAdmin();
                break;
            default:
                Debug.LogError("Invalid role selected.");
                break;
        }
    }

    public void JoinAsEVA() => AssignRoleAndJoin("EVA");
    public void JoinAsIVA() => AssignRoleAndJoin("IVA");
    public void JoinAsMCC() => AssignRoleAndJoin("MCC");
    public void JoinAsLLM() => AssignRoleAndJoin("LLM");
    public void JoinAsAdmin() => AssignRoleAndJoin("Admin");

    private void AssignRoleAndJoin(string role)
    {
        selectedRole = role;
        JoinRoom();
    }

    private void JoinRoom()
    {
        string enteredPassword = passwordInput.text.ToLower();
        string requiredPassword = selectedRole == "Admin" ? "comet" : selectedRole.ToLower();

        if (enteredPassword != requiredPassword)
        {
            passwordErrorText.text = "Incorrect password for " + selectedRole;
            return;
        }

        passwordErrorText.text = "";

        RoomOptions options = new RoomOptions { MaxPlayers = 10 };
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
        string countKey = selectedRole + "_Count";
        int currentCount = roomProps.ContainsKey(countKey) ? (int)roomProps[countKey] : 0;

        PhotonNetwork.LocalPlayer.NickName = $"{selectedRole}";
        GlobalManager.Instance.SetPlayerRole(selectedRole, currentCount.ToString());

        ExitGames.Client.Photon.Hashtable playerProps = new ExitGames.Client.Photon.Hashtable
        {
            { "Role", selectedRole },
            { "ID", currentCount.ToString() }
        };
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerProps);

        if (PhotonNetwork.IsMasterClient)
        {
            roomProps[countKey] = currentCount + 1;
            PhotonNetwork.CurrentRoom.SetCustomProperties(roomProps);
        }

        string targetScene = selectedRole switch
        {
            "EVA" => "EVA_Mission",
            "IVA" => "IVA_Mission",
            "MCC" => "IVA_Mission",
            "LLM" => "IVA_Mission",
            "Admin" => "Admin",
            _ => "IVA_Mission"
        };

        PhotonNetwork.LoadLevel(targetScene);
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Photon master server.");
    }
}
