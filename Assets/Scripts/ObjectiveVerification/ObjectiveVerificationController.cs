using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Text;

public class ObjectiveVerificationController : MonoBehaviour
{
    [SerializeField] private GameObject completedTaskListsPanel;
    [SerializeField] private GameObject goalManagementPanel;

    [SerializeField] private Button completedTaskListsButton;
    [SerializeField] private Button goalManagementButton;

    [SerializeField] private Color activeColor = new Color(0.8f, 0.9f, 1f);
    [SerializeField] private Color inactiveColor = Color.white;

    [SerializeField] private TMP_Text maxDurationValueText;  
    [SerializeField] private TMP_Text targetROIValueText;    

    [SerializeField] private TMP_InputField submittedDurationField;
    [SerializeField] private TMP_InputField submittedROIField;

    private Dictionary<string, string> ev1TaskLists = new();
    private Dictionary<string, string> ev2TaskLists = new();
    private HashSet<string> taskTitles = new();
    private List<string> listOrder = new();

    [SerializeField] private TMP_Text ev1TaskText;
    [SerializeField] private TMP_Text ev2TaskText;

    void Start()
    {
        ShowCompletedTaskLists();

        submittedDurationField.interactable = false;
        submittedDurationField.caretWidth = 0;

        submittedROIField.interactable = false;
        submittedROIField.caretWidth = 0;

        // Also override disabled color to match editable
        var durColors = submittedDurationField.colors;
        durColors.disabledColor = Color.white;
        submittedDurationField.colors = durColors;

        var roiColors = submittedROIField.colors;
        roiColors.disabledColor = Color.white;
        submittedROIField.colors = roiColors;
    }

    private void LoadObjectiveGoals()
    {
        var config = ConfigLoader.ObjectiveVerification;

        if (config != null)
        {
            maxDurationValueText.text = config.MaxDuration.ToString();
            targetROIValueText.text = config.TargetROI.ToString();
        }
        else
        {
            Debug.LogWarning("ObjectiveVerification config is null.");
        }
        Debug.Log("IVA Objective Config: " + ConfigLoader.ObjectiveVerification?.MaxDuration + " / " + ConfigLoader.ObjectiveVerification?.TargetROI);
    }

    public void ShowCompletedTaskLists()
    {
        completedTaskListsPanel.SetActive(true);
        goalManagementPanel.SetActive(false);

        HighlightButton(goalManagementButton, false);
        HighlightButton(completedTaskListsButton, true);
    }

    public void ShowGoalManagement()
    {
        LoadObjectiveGoals();
        goalManagementPanel.SetActive(true);
        completedTaskListsPanel.SetActive(false);

        HighlightButton(goalManagementButton, true);
        HighlightButton(completedTaskListsButton, false);
    }

    private void HighlightButton(Button button, bool active)
    {
        Image image = button.GetComponent<Image>();
        if (image != null)
        {
            image.color = active ? activeColor : inactiveColor;
        }
    }

    public void SetTotals(int durationTotal, int roiTotal)
    {
        Debug.Log($"[ObjectiveVerificationController] Setting totals: Duration={durationTotal}, ROI={roiTotal}");

        if (submittedDurationField != null)
            submittedDurationField.text = durationTotal.ToString();
        if (submittedROIField != null)
            submittedROIField.text = roiTotal.ToString();
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
        System.Text.StringBuilder sb = new();
        bool first = true;

        foreach (string title in listOrder)
        {
            if (taskDict.TryGetValue(title, out string taskBlock))
            {
                if (!first) sb.AppendLine("-------------");
                sb.AppendLine(taskBlock.Trim());
                first = false;
            }
        }

        return sb.ToString().TrimEnd();
    }

}
