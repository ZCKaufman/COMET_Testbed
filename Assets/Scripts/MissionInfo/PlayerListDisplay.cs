using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using System.Text;

public class PlayerListDisplay : MonoBehaviourPunCallbacks
{
    public TextMeshProUGUI playerListText;

    void Start()
    {
        RefreshPlayerList();
    }

    public void RefreshPlayerList()
    {
        StringBuilder sb = new StringBuilder();

        foreach (Player player in PhotonNetwork.PlayerList)
        {
            object roleObj;
            object idObj;

            player.CustomProperties.TryGetValue("Role", out roleObj);
            player.CustomProperties.TryGetValue("ID", out idObj);

            string role = roleObj != null ? roleObj.ToString() : "Unknown";
            string id = idObj != null ? idObj.ToString() : "N/A";

            sb.AppendLine($"{role} #{id}");
        }

        playerListText.text = sb.ToString();
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        RefreshPlayerList();
    }
}
