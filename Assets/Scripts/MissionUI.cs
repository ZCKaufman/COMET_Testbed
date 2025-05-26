using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using System.Collections;

public class MissionUI : MonoBehaviourPunCallbacks
{
    [SerializeField] private TextMeshProUGUI roleText;
    [SerializeField] private TextMeshProUGUI pingMessage;
    [SerializeField] private Button pingButton;

    private PhotonView view;

    private void Awake()
    {
        view = GetComponent<PhotonView>();
    }

    private IEnumerator Start()
    {
        // Defensive null checks for debugging
        if (roleText == null) Debug.LogError("Role Text not assigned!");
        if (pingMessage == null) Debug.LogError("Ping Message not assigned!");
        if (pingButton == null) Debug.LogError("Ping Button not assigned!");

        // Wait until Photon is ready
        while (!PhotonNetwork.InRoom)
            yield return null;

        // Display user role (EVA or IVA)
        string role = PhotonNetwork.LocalPlayer.NickName;
        roleText.text = "You are: " + role;

        // Add ping button event
        pingButton.onClick.AddListener(() =>
        {
            view.RPC("RPC_PingTeam", RpcTarget.Others, role);
        });
    }

    [PunRPC]
    private void RPC_PingTeam(string fromRole)
    {
        pingMessage.text = "Ping received from: " + fromRole;
        CancelInvoke(nameof(ClearPing));
        Invoke(nameof(ClearPing), 3f);
    }

    private void ClearPing()
    {
        pingMessage.text = "";
    }
}
