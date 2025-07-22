using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using System.Linq;

public class MissionInfoTabController : MonoBehaviour
{
    [SerializeField] private TMP_Text missionDescriptionText;
    [SerializeField] private TMP_Text alertsText;
    [SerializeField] private TMP_Text ev1TaskText;
    [SerializeField] private TMP_Text ev2TaskText;

    private Dictionary<string, string> ev1TaskLists = new();
    private Dictionary<string, string> ev2TaskLists = new();
    private HashSet<string> taskTitles = new(); 
    private List<string> listOrder = new();             

    public MissionViewConfigRoot config; 
    void Awake()
    {
        var config = ConfigLoader.MissionConfig;

        if (config?.MissionInfo?.All != null)
        {
            Debug.Log("Loaded mission info: " + config.MissionInfo.All.MissionDescription);
            SetMissionDescription(config.MissionInfo.All.MissionDescription);
            SetAlerts(config.MissionInfo.All.Alerts);
            GlobalManager.Instance.InitializeGlobalDurationsFromConfig(ConfigLoader.TaskPlanning);

        }
        else
        {
            Debug.LogWarning("Mission config not set or incomplete.");
        }
    }

    public void SetMissionDescription(string text)
    {
        missionDescriptionText.text = text;
    }

    public void SetAlerts(List<string> alerts)
    {
        if (alerts == null || alerts.Count == 0)
        {
            alertsText.text = "No alerts for this mission.";
        }
        else
        {
            alertsText.text = string.Join("\n• ", new[] { "" }.Concat(alerts)); // Adds bullet point to each line
        }
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
