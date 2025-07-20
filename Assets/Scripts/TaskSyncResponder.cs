using Photon.Pun;
using UnityEngine;

public class TaskSyncResponder : MonoBehaviourPunCallbacks
{
    public static TaskSyncResponder Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    [PunRPC]
    void RPC_UpdateDuration(string containerName, int index, int newDuration)
    {
        // Update EVA (optional)
        var evaPanel = FindFirstObjectByType<EVATaskPanelController>();
        if (evaPanel != null)
        {
            // Optionally forward the update to EVA
        }

        // Update IVA
        var ivaPanel = FindFirstObjectByType<IVATaskPanelController>();
        if (ivaPanel != null)
        {
            ivaPanel.ReceiveDurationUpdate(containerName, index, newDuration);
        }
    }

    public void BroadcastDurationUpdate(string containerName, int index, int newDuration)
    {
        photonView.RPC("RPC_UpdateDuration", RpcTarget.All, containerName, index, newDuration);
    }

    public void BroadcastTaskClear(string title, string ev1Body, string ev2Body)
    {
        photonView.RPC("RPC_PerformTaskClear", RpcTarget.All, title, ev1Body, ev2Body);
    }

    [PunRPC]
    void RPC_PerformTaskClear(string title, string ev1Body, string ev2Body)
    {
        var panel = FindFirstObjectByType<EVATaskPanelController>();
        if (panel != null)
        {
            panel.PerformClearAll(title, ev1Body, ev2Body);
        }
        GlobalManager.Instance?.UpdateTaskList(title, ev1Body, ev2Body);
    }

    public void BroadcastTaskListTitleUpdate(string newTitle)
    {
        photonView.RPC("RPC_UpdateTaskListTitle", RpcTarget.All, newTitle);
    }

    [PunRPC]
    void RPC_UpdateTaskListTitle(string newTitle)
    {
        var panel = FindFirstObjectByType<EVATaskPanelController>();
        if (panel != null)
        {
            panel.SetTaskListTitle(newTitle);
        }
    }

    public void BroadcastTaskTextUpdate(string containerName, int index, string newText)
    {
        photonView.RPC("RPC_UpdateTaskText", RpcTarget.All, containerName, index, newText);
    }

    [PunRPC]
    void RPC_UpdateTaskText(string containerName, int index, string newText)
    {
        var panel = FindFirstObjectByType<EVATaskPanelController>();
        if (panel != null)
        {
            panel.ReceiveTaskTextUpdate(containerName, index, newText);

        }
    }

    public void BroadcastInsertTask(string containerName, int index, string text)
    {
        photonView.RPC("RPC_InsertTaskAfterWithText", RpcTarget.All, containerName, index, text);
    }

    [PunRPC]
    void RPC_InsertTaskAfterWithText(string containerName, int index, string text)
    {
        var panel = FindFirstObjectByType<EVATaskPanelController>();
        if (panel != null)
        {
            panel.InsertTaskAtIndex(containerName, index, text);
        }
    }

    public void BroadcastDeleteTask(string containerName, int index)
    {
        photonView.RPC("RPC_TryDeleteTaskAtIndex", RpcTarget.All, containerName, index);
    }

    [PunRPC]
    void RPC_TryDeleteTaskAtIndex(string containerName, int index)
    {
        var panel = FindFirstObjectByType<EVATaskPanelController>();
        if (panel != null)
        {
            panel.DeleteTaskAtIndex(containerName, index);
        }
    }

    public void BroadcastFullSyncRequest()
    {
        photonView.RPC("RPC_RequestFullTaskSync", RpcTarget.MasterClient);
    }

    [PunRPC]
    void RPC_RequestFullTaskSync(PhotonMessageInfo info)
    {
        var panel = FindFirstObjectByType<EVATaskPanelController>();
        if (panel != null)
        {
            string[] ev1Texts = panel.GetEv1Texts();
            string[] ev2Texts = panel.GetEv2Texts();
            photonView.RPC("RPC_ReceiveFullTaskSync", info.Sender, ev1Texts, ev2Texts);
        }
    }

    [PunRPC]
    void RPC_ReceiveFullTaskSync(string[] ev1Texts, string[] ev2Texts)
    {
        var panel = FindFirstObjectByType<EVATaskPanelController>();
        if (panel != null)
        {
            panel.ReceiveFullTaskSync(ev1Texts, ev2Texts);
        }
    }
}
