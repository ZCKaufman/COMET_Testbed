using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class EVATaskPanelController : MonoBehaviour
{
    [Header("Task Prefab and Containers")]
    [SerializeField] private GameObject taskItemPrefab;
    [SerializeField] private TMP_InputField taskListTitleInput;
    [SerializeField] private Button submitButton;

    [SerializeField] private Transform ev1Container;
    [SerializeField] private Transform ev2Container;

    private List<TMP_InputField> ev1Tasks = new();
    private List<TMP_InputField> ev2Tasks = new();

    void Start()
    {
        AddTask(ev1Container, ev1Tasks);
        AddTask(ev2Container, ev2Tasks);

        if (submitButton != null)
            submitButton.onClick.AddListener(ClearAll);
    }


    void AddTask(Transform container, List<TMP_InputField> taskList)
    {
        int taskNumber = taskList.Count + 1;

        GameObject taskItem = Instantiate(taskItemPrefab, container);
        taskItem.name = $"Task{taskNumber}";

        TMP_Text numberLabel = taskItem.transform.Find("NumberLabel").GetComponent<TMP_Text>();
        TMP_InputField inputField = taskItem.transform.Find("TaskInputField").GetComponent<TMP_InputField>();

        numberLabel.text = $"{taskNumber}.";

        // Add listener to detect typing
        inputField.onValueChanged.AddListener((string text) =>
        {
            if (taskList[^1] == inputField && !string.IsNullOrWhiteSpace(text))
            {
                AddTask(container, taskList);
            }
        });

        taskList.Add(inputField);
    }

    void ClearAll()
    {
        Debug.Log("[EVA] Submit clicked – clearing task lists.");

        // Clear title
        if (taskListTitleInput != null)
            taskListTitleInput.text = "";

        // Destroy old task entries
        foreach (var task in ev1Tasks)
            Destroy(task.transform.parent.gameObject);
        foreach (var task in ev2Tasks)
            Destroy(task.transform.parent.gameObject);

        ev1Tasks.Clear();
        ev2Tasks.Clear();

        // Add the initial blank task for each list
        AddTask(ev1Container, ev1Tasks);
        AddTask(ev2Container, ev2Tasks);
    }

}
