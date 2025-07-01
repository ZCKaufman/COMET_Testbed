using UnityEngine;

public class GlobalManager : MonoBehaviour
{
    public static GlobalManager Instance;

    // player's role in the game
    public string PlayerRole { get; set; }

    // player ID in EVA or IVA
    public string PlayerID { get; set; }

    // optional - not yet used
    public string PlayerName { get; private set; }

    public string LatestTaskListTitle { get; private set; } = "";
    public string LatestEv1Tasks { get; private set; } = "";
    public string LatestEv2Tasks { get; private set; } = "";

    // mission selected by admin (default to Mission001.json)
    public string SelectedMissionFile { get; private set; } = "Mission001.json";
    

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

        Debug.Log($"[GlobalManager] Assigned Role: {PlayerRole}, ID: {PlayerID}");
    }

    public void UpdateTaskList(string title, string ev1, string ev2)
    {
        Debug.Log("[GlobalManager] TaskList Updated with title: " + title);
        LatestTaskListTitle = title;
        LatestEv1Tasks = ev1;
        LatestEv2Tasks = ev2;
        OnTaskListUpdated?.Invoke(title, ev1, ev2);
    }

    public void SetSelectedMission(string filename)
    {
        SelectedMissionFile = filename;
        Debug.Log($"[GlobalManager] Mission file set to: {SelectedMissionFile}");
    }
}
