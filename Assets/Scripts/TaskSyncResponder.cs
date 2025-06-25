using UnityEngine;
using Photon.Pun;

public class TaskSyncResponder : MonoBehaviourPun
{
    [PunRPC]
    void RPC_RequestLatestGlobalTasks(PhotonMessageInfo info)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        Debug.Log("[Master] Received task sync request");

        GlobalManager mgr = GlobalManager.Instance;
        if (mgr == null || string.IsNullOrEmpty(mgr.LatestTaskListTitle)) return;

        photonView.RPC("RPC_GlobalTaskUpdate", info.Sender,
            mgr.LatestTaskListTitle,
            mgr.LatestEv1Tasks,
            mgr.LatestEv2Tasks);
    }

    [PunRPC]
    void RPC_GlobalTaskUpdate(string title, string ev1, string ev2)
    {
        Debug.Log("[Global Update] Received on any client");
        GlobalManager.Instance?.UpdateTaskList(title, ev1, ev2);
    }
}

