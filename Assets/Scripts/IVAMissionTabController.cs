using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using Photon.Pun;
using System.Collections;

public class IVAMissionInfoTabController : MonoBehaviour
{
    [SerializeField] private TMP_Text missionDescriptionText;
    [SerializeField] private TMP_Text ev1TaskText;
    [SerializeField] private TMP_Text ev2TaskText;

    private Dictionary<string, string> ev1TaskLists = new();
    private Dictionary<string, string> ev2TaskLists = new();
    private HashSet<string> taskTitles = new();         // use this to ensure sync
    private List<string> listOrder = new();             // maintains submission order

    public MissionViewConfigRoot config;
    private PhotonView photonView;

    void Awake()
    {
        photonView = GetComponent<PhotonView>();

        var config = ConfigLoader.MissionConfig;

        if (config?.MissionInfo?.All != null)
        {
            Debug.Log("Loaded mission info: " + config.MissionInfo.All.MissionInfo);
            SetMissionDescription(config.MissionInfo.All.MissionInfo);
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

        if (!string.IsNullOrEmpty(GlobalManager.Instance.LatestEv1Tasks))
        {
            Debug.Log("[IVA] Immediately applying latest task list");
            HandleGlobalTaskListUpdate(
                GlobalManager.Instance.LatestTaskListTitle,
                GlobalManager.Instance.LatestEv1Tasks,
                GlobalManager.Instance.LatestEv2Tasks
            );
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
            listOrder.Insert(0, title); // insert at beginning for newest-first
        }

        string ev1Entry = ev1List.Trim();
        string ev2Entry = ev2List.Trim();

        if (ev1TaskLists.ContainsKey(title))
            ev1TaskLists[title] = ev1Entry + "\n-------------\n" + ev1TaskLists[title];  // prepend
        else
            ev1TaskLists[title] = ev1Entry;

        if (ev2TaskLists.ContainsKey(title))
            ev2TaskLists[title] = ev2Entry + "\n-------------\n" + ev2TaskLists[title];  // prepend
        else
            ev2TaskLists[title] = ev2Entry;

        ev1TaskText.text = GenerateText(ev1TaskLists);
        ev2TaskText.text = GenerateText(ev2TaskLists);
    }



    private string GenerateText(Dictionary<string, string> taskDict)
    {
        StringBuilder sb = new();
        foreach (string title in listOrder)
        {
            if (taskDict.TryGetValue(title, out string taskBlock))
            {
                sb.AppendLine(taskBlock);
                sb.AppendLine(); // extra spacing between lists
            }
        }
        return sb.ToString().Trim();
    }


}
