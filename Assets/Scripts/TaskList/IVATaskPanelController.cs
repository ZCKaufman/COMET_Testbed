using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IVATaskPanelController : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown poiDropdown;
    [SerializeField] private GameObject taskItemPrefab;
    [SerializeField] private Transform taskContainer;
    [SerializeField] private GameObject headerRow;
    [SerializeField] private GameObject loadingText;

    private List<GameObject> currentTaskItems = new();

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
        string templateType = selectedPOI.type;
        var matchingTemplate = ConfigLoader.TaskPlanning.POIs.Find(p => p.Name == templateType);

        if (matchingTemplate != null)
        {
            LoadTaskList(matchingTemplate.Tasks);
        }
        else
        {
            Debug.LogWarning("No matching template found for type: " + templateType);
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
            if (roi != null) roi.text = ""; // Placeholder for future ROI input
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(taskContainer.GetComponent<RectTransform>());
    }
}
