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

    public Dictionary<string, List<int>> GlobalDurations { get; private set; } = new();
    public void InitializeGlobalDurationsFromConfig(TaskPlanningSection taskPlanning)
    {
        foreach (var poi in ConfigLoader.MapConfig.Mapping.POIs)
        {
            string poiName = poi.description.Split('|')[0];
            string templateType = poi.type;

            var matchingTemplate = taskPlanning.POIs.Find(p => p.Name == templateType);
            if (matchingTemplate == null)
            {
                Debug.LogWarning($"No task template found for POI type: {templateType}");
                continue;
            }

            // === Initialize GlobalDurations ===
            List<int> durations = new();
            for (int i = 0; i < matchingTemplate.Tasks.Count; i++)
            {
                int defaultDuration = matchingTemplate.Tasks[i].Duration;
                durations.Add(defaultDuration);

                // === Initialize IndividualDurations[poi][i]["EV1"] and ["EV2"] ===
                if (!IndividualDurations.ContainsKey(poiName))
                    IndividualDurations[poiName] = new Dictionary<int, Dictionary<string, int>>();

                IndividualDurations[poiName][i] = new Dictionary<string, int>
                {
                    { "EV1", defaultDuration },
                    { "EV2", defaultDuration }
                };
            }

            GlobalDurations[poiName] = durations;

            Debug.Log($"[Init] Global durations initialized for {poiName}: [{string.Join(", ", durations)}]");
        }
    }



    public void UpdateGlobalDuration(string poi, int index, int value)
    {
        if (!GlobalDurations.TryGetValue(poi, out var list))
        {
            Debug.Log(GlobalDurations);
            Debug.LogWarning($"[GlobalManager] POI '{poi}' not found. Cannot update duration.");
            return;
        }

        if (index < 0 || index >= list.Count)
        {
            Debug.LogWarning($"[GlobalManager] Invalid index {index} for POI '{poi}' with {list.Count} tasks.");
            return;
        }

        list[index] = value;
        Debug.Log($"[GlobalManager] Duration Updated - POI: {poi}, Index: {index}, Value: {value}");
    }


    public List<int> GetDurationsForPOI(string poi)
    {
        if (GlobalDurations.TryGetValue(poi, out var list))
            return list;

        Debug.LogWarning($"[GlobalManager] POI '{poi}' not found in GlobalDurations.");
        return new List<int>(); // Fallback in case it's not initialized
    }


    // Tracks: POI → Task Index → { "EV1": x, "EV2": y }
    public Dictionary<string, Dictionary<int, Dictionary<string, int>>> IndividualDurations { get; private set; } = new();
    public void UpdateIndividualDuration(string poi, int taskIndex, string role, int duration)
    {
        if (!IndividualDurations.ContainsKey(poi))
            IndividualDurations[poi] = new Dictionary<int, Dictionary<string, int>>();

        if (!IndividualDurations[poi].ContainsKey(taskIndex))
            IndividualDurations[poi][taskIndex] = new Dictionary<string, int>();

        IndividualDurations[poi][taskIndex][role] = duration;

        // ⬇️ Update GlobalDurations[poi][taskIndex] to max(EV1, EV2)
        int ev1 = IndividualDurations[poi][taskIndex].ContainsKey("EV1") ? IndividualDurations[poi][taskIndex]["EV1"] : 0;
        int ev2 = IndividualDurations[poi][taskIndex].ContainsKey("EV2") ? IndividualDurations[poi][taskIndex]["EV2"] : 0;

        if (!GlobalDurations.ContainsKey(poi))
            GlobalDurations[poi] = new List<int>();

        GlobalDurations[poi][taskIndex] = Mathf.Max(ev1, ev2);

        Debug.Log($"[GlobalManager] {role} updated duration for POI '{poi}', task {taskIndex} to {duration}. Max is now {GlobalDurations[poi][taskIndex]}.");
    }

}
