using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PhotonDebugLogger : MonoBehaviourPunCallbacks
{
    void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
        Debug.Log("[Photon] Connecting to server...");
        Debug.Log("[Photon] App ID: " + PhotonNetwork.PhotonServerSettings.AppSettings.AppIdRealtime);
        Debug.Log("[Photon] Fixed Region (if set): " + PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion);
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("[Photon] Connected to Master");
        Debug.Log("[Photon] Region Connected To: " + PhotonNetwork.CloudRegion);
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogWarning("[Photon] Disconnected: " + cause.ToString());
    }
}
