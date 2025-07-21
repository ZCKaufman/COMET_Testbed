using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using System.Collections;

public class IVATaskPanelController : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown poiDropdown;
    [SerializeField] private GameObject taskItemPrefab;
    [SerializeField] private Transform taskContainer;
    [SerializeField] private GameObject headerRow;
    [SerializeField] private GameObject loadingText;
    [SerializeField] private ScrollRect taskScrollView;

    private List<GameObject> currentTaskItems = new();

    private GameObject totalRowItem = null;

    private string currentTemplateType;

    private List<int> ev1Durations = new();
    private List<int> ev2Durations = new();

    void Start()
    {
        PopulateDropdown();
        poiDropdown.onValueChanged.AddListener(OnDropdownChanged);
    }

    private void PopulateDropdown()
    {
        poiDropdown.ClearOptions();
        List<string> options = new() { "Select a Point of Interest" };
        foreach (var poi in ConfigLoader.MapConfig.Mapping.POIs)
        {
            options.Add(poi.description.Split('|')[0]);
        }
        poiDropdown.AddOptions(options);
        poiDropdown.value = 0;
    }

    private void OnDropdownChanged(int index)
    {
        if (index == 0)
        {
            ClearTaskList();
            return;
        }

        var selectedPOI = ConfigLoader.MapConfig.Mapping.POIs[index - 1];
        currentTemplateType = selectedPOI.type;
        var matchingTemplate = ConfigLoader.TaskPlanning.POIs.Find(p => p.Name == currentTemplateType);

        if (matchingTemplate != null)
        {
            LoadTaskList(matchingTemplate.Tasks);
        }
        else
        {
            Debug.LogWarning("No matching template found for type: " + currentTemplateType);
        }

        if (headerRow != null)
            headerRow.SetActive(true);

        if (loadingText != null)
            loadingText.SetActive(false);
    }

    private void ClearTaskList()
    {
        foreach (var item in currentTaskItems)
        {
            Destroy(item);
        }
        currentTaskItems.Clear();

        if (headerRow != null)
            headerRow.SetActive(false);

        if (loadingText != null)
            loadingText.SetActive(true);
    }

    private void LoadTaskList(List<TaskEntry> tasks)
    {
        ClearTaskList();

        for (int i = 0; i < tasks.Count; i++)
        {
            TaskEntry task = tasks[i];
            GameObject item = Instantiate(taskItemPrefab, taskContainer);
            currentTaskItems.Add(item);

            TMP_Text numberLabel = item.transform.Find("NumberLabel")?.GetComponent<TMP_Text>();
            TMP_InputField description = item.transform.Find("TaskInputField")?.GetComponent<TMP_InputField>();
            TMP_InputField personnel = item.transform.Find("PersonnelField")?.GetComponent<TMP_InputField>();
            TMP_InputField duration = item.transform.Find("DurationField")?.GetComponent<TMP_InputField>();
            TMP_InputField roi = item.transform.Find("ROIField")?.GetComponent<TMP_InputField>();

            if (numberLabel != null) numberLabel.text = (i + 1).ToString();
            if (description != null) description.text = task.Description;
            if (personnel != null) personnel.text = task.Personnel.ToString();
            if (duration != null) duration.text = task.Duration.ToString();
            if (roi != null && duration != null)
            {
                float roiValue = EvaluateEquation(task.roiEquation ?? "0", task.Duration);
                roi.text = roiValue.ToString("0");
            }

            // Override disabled color behavior
            var colors = description.colors;
            colors.disabledColor = description.image.color;  
            description.colors = colors;

            colors.disabledColor = personnel.image.color;  
            personnel.colors = colors;

            colors.disabledColor = duration.image.color;  
            duration.colors = colors;

            colors.disabledColor = roi.image.color;  
            roi.colors = colors;
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(taskContainer.GetComponent<RectTransform>());
        InsertTotalRow();
        UpdateLocalTotals();

        LayoutRebuilder.ForceRebuildLayoutImmediate(taskContainer.GetComponent<RectTransform>());
        Canvas.ForceUpdateCanvases();
        StartCoroutine(ScrollToBottomNextFrame());
    }

    private IEnumerator ScrollToBottomNextFrame()
    {
        // Wait until end of frame to ensure all tasks are rendered and laid out
        yield return null;
        yield return new WaitForEndOfFrame();

        // Force Unity to recalculate layout sizes
        Canvas.ForceUpdateCanvases();

        // Scroll to the bottom
        if (taskScrollView != null)
            taskScrollView.verticalNormalizedPosition = 0f;
    }

    private void UpdateLocalTotals()
    {
        float durationSum = 0;
        float roiSum = 0;

        for (int i = 0; i < currentTaskItems.Count - 1; i++) // Skip last row (totals)
        {
            Transform item = currentTaskItems[i].transform;

            var durationField = item.Find("DurationField")?.GetComponent<TMP_InputField>();
            var roiField = item.Find("ROIField")?.GetComponent<TMP_InputField>();

            if (durationField != null && float.TryParse(durationField.text, out float dur))
                durationSum += dur;

            if (roiField != null && float.TryParse(roiField.text, out float roi))
                roiSum += roi;
        }

        var totalDuration = totalRowItem.transform.Find("DurationField")?.GetComponent<TMP_InputField>();
        var totalROI = totalRowItem.transform.Find("ROIField")?.GetComponent<TMP_InputField>();

        if (totalDuration != null) totalDuration.text = durationSum.ToString("0");
        if (totalROI != null) totalROI.text = roiSum.ToString("0");
    }

    private void InsertTotalRow()
    {
        totalRowItem = Instantiate(taskItemPrefab, taskContainer);
        currentTaskItems.Add(totalRowItem);

        TMP_Text numberLabel = totalRowItem.transform.Find("NumberLabel")?.GetComponent<TMP_Text>();
        TMP_InputField description = totalRowItem.transform.Find("TaskInputField")?.GetComponent<TMP_InputField>();
        TMP_InputField personnelField = totalRowItem.transform.Find("PersonnelField")?.GetComponent<TMP_InputField>();
        TMP_InputField durationField = totalRowItem.transform.Find("DurationField")?.GetComponent<TMP_InputField>();
        TMP_InputField roiField = totalRowItem.transform.Find("ROIField")?.GetComponent<TMP_InputField>();

        // Number label blank
        if (numberLabel != null) numberLabel.text = "";

        // Description field
        if (description != null)
        {
            description.text = "Total:";
            description.interactable = false;
            description.caretWidth = 0;
            description.image.color = Color.white;

            var colors = description.colors;
            colors.disabledColor = Color.white;
            description.colors = colors;
        }

        // Personnel field
        if (personnelField != null)
        {
            personnelField.text = "--";
            personnelField.interactable = false;
            personnelField.caretWidth = 0;
            personnelField.image.color = Color.white;

            var colors = personnelField.colors;
            colors.disabledColor = Color.white;
            personnelField.colors = colors;
        }

        // Duration field
        if (durationField != null)
        {
            durationField.interactable = false;
            durationField.text = "0";
            durationField.caretWidth = 0;
            durationField.image.color = Color.white;

            var colors = durationField.colors;
            colors.disabledColor = Color.white;
            durationField.colors = colors;
        }

        // ROI field
        if (roiField != null)
        {
            roiField.interactable = false;
            roiField.text = "0";
            roiField.caretWidth = 0;
            roiField.image.color = Color.white;

            var colors = roiField.colors;
            colors.disabledColor = Color.white;
            roiField.colors = colors;
        }
    }

    public void ReceiveDurationUpdate(string role, int index, int value)
    {
        EnsureListSize(ev1Durations, index + 1);
        EnsureListSize(ev2Durations, index + 1);

        if (role == "EV1")
            ev1Durations[index] = value;
        else if (role == "EV2")
            ev2Durations[index] = value;

        int merged = Mathf.Max(ev1Durations[index], ev2Durations[index]);

        if (index < 0 || index >= currentTaskItems.Count)
        {
            Debug.LogWarning($"Invalid task index {index} for IVA update.");
            return;
        }

        TMP_InputField durationField = currentTaskItems[index].transform.Find("DurationField")?.GetComponent<TMP_InputField>();
        if (durationField != null)
            durationField.text = merged.ToString();

        TMP_InputField roiField = currentTaskItems[index].transform.Find("ROIField")?.GetComponent<TMP_InputField>();
        if (roiField != null)
        {
            string roiEq = ConfigLoader.TaskPlanning.POIs
                .Find(p => p.Name == currentTemplateType)?.Tasks[index].roiEquation ?? "0";
            float roiVal = EvaluateEquation(roiEq, merged);
            roiField.text = roiVal.ToString("0");
        }

        UpdateLocalTotals();

    }

    private void EnsureListSize(List<int> list, int size)
    {
        while (list.Count < size)
            list.Add(0);
    }

    private float EvaluateEquation(string expression, float x)
    {
        // Insert explicit multiplication between number and 'x' (e.g., 5x → 5*x)
        expression = System.Text.RegularExpressions.Regex.Replace(expression, @"(\d)(x)", "$1*$2");

        // Now replace x with the actual value
        expression = expression.Replace("x", x.ToString());

        try
        {
            System.Data.DataTable table = new System.Data.DataTable();
            object result = table.Compute(expression, "");
            return float.TryParse(result.ToString(), out float val) ? val : 0;
        }
        catch
        {
            Debug.LogWarning("Invalid ROI equation: " + expression);
            return 0;
        }
    }

}
