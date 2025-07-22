using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using Photon.Pun;

public class IVAMissionInfoTabController : MonoBehaviour
{
    [SerializeField] private TMP_Text missionDescriptionText;
    [SerializeField] private TMP_Text ev1TaskText;
    [SerializeField] private TMP_Text ev2TaskText;

    private Dictionary<string, string> ev1TaskLists = new();
    private Dictionary<string, string> ev2TaskLists = new();
    private HashSet<string> taskTitles = new();         // use this to ensure sync
    private List<string> listOrder = new();             // maintains submission order
    private PhotonView photonView;

    void Awake()
    {
        photonView = GetComponent<PhotonView>();

        var config = ConfigLoader.MissionConfig;

        if (config?.MissionInfo?.All != null)
        {
            Debug.Log("Loaded mission info: " + config.MissionInfo.All.MissionDescription);
            SetMissionDescription(config.MissionInfo.All.MissionDescription);
            GlobalManager.Instance.InitializeGlobalDurationsFromConfig(ConfigLoader.TaskPlanning);
        }
        else
        {
            Debug.LogWarning("Mission config not set or incomplete.");
        }
    }

    void OnEnable()
    {
        Debug.Log("[IVA] OnEnable() called");

        if (GlobalManager.Instance != null)
        {
            Debug.Log("[IVA] Subscribing to OnTaskListUpdated");
            GlobalManager.Instance.OnTaskListUpdated -= HandleGlobalTaskListUpdate; // avoid double-subscribe
            GlobalManager.Instance.OnTaskListUpdated += HandleGlobalTaskListUpdate;

            string latestTitle = GlobalManager.Instance.LatestTaskListTitle;
            if (!string.IsNullOrEmpty(latestTitle) && !taskTitles.Contains(latestTitle))
            {
                Debug.Log("[IVA] Applying latest task list: " + latestTitle);
                HandleGlobalTaskListUpdate(
                    latestTitle,
                    GlobalManager.Instance.LatestEv1Tasks,
                    GlobalManager.Instance.LatestEv2Tasks
                );
            }
            else
            {
                Debug.Log("[IVA] Skipping duplicate task list load");
            }
        }
        else
        {
            Debug.LogWarning("[IVA] GlobalManager.Instance is null on enable");
        }
    }

    void OnDisable()
    {
        if (GlobalManager.Instance != null)
            GlobalManager.Instance.OnTaskListUpdated -= HandleGlobalTaskListUpdate;
    }


    private void HandleGlobalTaskListUpdate(string title, string ev1List, string ev2List)
    {
        Debug.Log("Global update received in IVA: " + title);
        SetTaskLists(title, ev1List, ev2List);
    }


    public void SetMissionDescription(string text)
    {
        missionDescriptionText.text = text;
    }

    public void SetTaskLists(string title, string ev1List, string ev2List)
    {
        title = title.Trim();
        if (string.IsNullOrEmpty(title))
            title = "Untitled Task List";

        if (!taskTitles.Contains(title))
        {
            taskTitles.Add(title);
            listOrder.Add(title); 
        }

        ev1TaskLists[title] = ev1List.Trim();
        ev2TaskLists[title] = ev2List.Trim();

        ev1TaskText.text = GenerateText(ev1TaskLists);
        ev2TaskText.text = GenerateText(ev2TaskLists);
    }

    private string GenerateText(Dictionary<string, string> taskDict)
    {
        StringBuilder sb = new();
        bool first = true;

        foreach (string title in listOrder)
        {
            if (taskDict.TryGetValue(title, out string taskBlock))
            {
                if (!first)
                {
                    sb.AppendLine("-------------");
                }
                sb.AppendLine(taskBlock.Trim());
                first = false;
            }
        }

        return sb.ToString().TrimEnd();
    }

}
