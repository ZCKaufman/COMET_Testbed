using UnityEngine;
using UnityEngine.UI;
using TMPro;

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

    void Start()
    {
        ShowCompletedTaskLists();
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
}
