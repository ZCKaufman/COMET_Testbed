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
