using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public class MissionInfoTabController : MonoBehaviour
{
    [SerializeField] private TMP_Text missionDescriptionText;
    [SerializeField] private TMP_Text ev1TaskText;
    [SerializeField] private TMP_Text ev2TaskText;

    private Dictionary<string, string> ev1TaskLists = new();
    private Dictionary<string, string> ev2TaskLists = new();
    private HashSet<string> taskTitles = new();         // use this to ensure sync
    private List<string> listOrder = new();             // maintains submission order

    public void SetMissionDescription(string text)
    {
        missionDescriptionText.text = text;
    }

    public void SetTaskLists(string title, string ev1List, string ev2List)
    {
        title = title.Trim();

        if (string.IsNullOrEmpty(title))
            title = "Untitled Task List";

        // If it's a new title, add it to the end of the order list
        if (!taskTitles.Contains(title))
        {
            taskTitles.Add(title);
            listOrder.Add(title);  // ✅ Add to the bottom
        }

        // Replace content, but do NOT move it in the listOrder
        ev1TaskLists[title] = ev1List.Trim();
        ev2TaskLists[title] = ev2List.Trim();

        // Rebuild UI text
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
