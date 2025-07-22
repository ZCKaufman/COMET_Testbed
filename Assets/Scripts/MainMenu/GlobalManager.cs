using System.Collections.Generic;
using UnityEngine;
using System;

public class GlobalManager : MonoBehaviour
{
    public static GlobalManager Instance;

    // player's role in the game
    public string PlayerRole { get; set; }

    //player ID in EVA OR IVA
    public string PlayerID { get; set; }

    //optional- not yet used
    public string PlayerName { get; private set; } // Player's name - not yet defined anywhere but can be


    public string LatestTaskListTitle { get; private set; } = "";
    public string LatestEv1Tasks { get; private set; } = "";
    public string LatestEv2Tasks { get; private set; } = "";

    public delegate void TaskListUpdated(string title, string ev1, string ev2);
    public event TaskListUpdated OnTaskListUpdated;


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

    public void UpdateTaskList(string title, string ev1, string ev2)
    {
        Debug.Log("[RPC_GlobalTaskUpdate] Called with title: " + title + ev1 + ev2);
        LatestTaskListTitle = title;
        LatestEv1Tasks = ev1;
        LatestEv2Tasks = ev2;
        OnTaskListUpdated?.Invoke(title, ev1, ev2);
    }
    public Dictionary<string, (int Duration, int ROI)> TaskListSummaries { get; private set; } = new();

    public delegate void TaskListSummaryUpdated(string title, int duration, int roi);
    public event TaskListSummaryUpdated OnTaskListSummaryUpdated;

    public void UpdateTaskListSummary(string title, int duration, int roi)
    {
        title = title.Trim();

        TaskListSummaries[title] = (duration, roi);

        Debug.Log($"[GlobalManager] TaskList Summary Updated: {title} => Duration={duration}, ROI={roi}");

        OnTaskListSummaryUpdated?.Invoke(title, duration, roi);

    }


}
