using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;

public class MainMenuUI : MonoBehaviourPunCallbacks
{
    private string selectedRole;

    void Start()
    {
        PhotonNetwork.ConnectUsingSettings(); // Connects to Photon automatically
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
        RoomOptions options = new RoomOptions { MaxPlayers = 2 };
        PhotonNetwork.JoinOrCreateRoom("MoonMissionRoom", options, TypedLobby.Default);
    }

    public override void OnJoinedRoom()
    {
        PhotonNetwork.LocalPlayer.NickName = selectedRole;
        PhotonNetwork.LoadLevel("Mission"); // This will sync across clients
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Photon master server.");
    }
}
