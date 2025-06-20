using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EVATaskPanelController : MonoBehaviour
{
    [SerializeField] private GameObject taskItemPrefab;
    [SerializeField] private Transform ev1Container;
    [SerializeField] private Transform ev2Container;
    [SerializeField] private TMP_InputField taskListTitleInput;
    [SerializeField] private Button submitButton;

    [SerializeField] private MissionInfoTabController missionInfoController;

    private List<TMP_InputField> ev1Tasks = new List<TMP_InputField>();
    private List<TMP_InputField> ev2Tasks = new List<TMP_InputField>();

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
        TMP_Text numberLabel = taskItem.transform.Find("NumberLabel")?.GetComponent<TMP_Text>();
        TMP_InputField inputField = taskItem.transform.Find("TaskInputField")?.GetComponent<TMP_InputField>();

        if (numberLabel != null)
            numberLabel.text = taskNumber + ".";

        if (inputField != null)
        {
            inputField.lineType = TMP_InputField.LineType.SingleLine;

            // Add to list
            taskList.Add(inputField);

            // Enter key = add new task
            inputField.onSubmit.AddListener((input) =>
            {
                if (!string.IsNullOrWhiteSpace(input) && inputField == taskList[taskList.Count - 1])
                {
                    AddTask(container, taskList);
                }
            });

            // Delete key = remove task if empty
            inputField.onSelect.AddListener((_) =>
            {
                inputField.onValueChanged.AddListener((text) =>
                {
                    inputField.onValidateInput = (string _, int __, char c) =>
                    {
                        if (c == '\b' || c == (char)127) // Backspace or Delete
                        {
                            if (string.IsNullOrWhiteSpace(inputField.text) &&
                                taskList.Count > 1)
                            {
                                int index = taskList.IndexOf(inputField);
                                if (index != -1)
                                {
                                    // Remove and destroy
                                    taskList.RemoveAt(index);
                                    Destroy(taskItem);

                                    // Renumber remaining items
                                    RenumberTasks(container, taskList);
                                }
                                return '\0'; // ignore character input
                            }
                        }
                        return c;
                    };
                });
            });
        }
    }

    void RenumberTasks(Transform container, List<TMP_InputField> taskList)
    {
        for (int i = 0; i < taskList.Count; i++)
        {
            TMP_Text numberLabel = taskList[i].transform.parent.Find("NumberLabel")?.GetComponent<TMP_Text>();
            if (numberLabel != null)
            {
                numberLabel.text = (i + 1) + ".";
            }
        }
    }


    void ClearAll()
    {
        string timestamp = System.DateTime.Now.ToString("HH:mm:ss");
        
        string ev1Text = $"--- Submitted at {timestamp} ---\n";
        string ev2Text = $"--- Submitted at {timestamp} ---\n";

        ev1Text += taskListTitleInput.text + "\n";
        ev2Text += taskListTitleInput.text + "\n";

        foreach (var task in ev1Tasks)
        {
            if (!string.IsNullOrWhiteSpace(task.text))
                ev1Text += "- " + task.text + "\n";

            Destroy(task.transform.parent.gameObject);
        }

        foreach (var task in ev2Tasks)
        {
            if (!string.IsNullOrWhiteSpace(task.text))
                ev2Text += "- " + task.text + "\n";

            Destroy(task.transform.parent.gameObject);
}


        taskListTitleInput.text = "";
        ev1Tasks.Clear();
        ev2Tasks.Clear();

        // Append to Mission Info tab
        if (missionInfoController != null)
        {
            missionInfoController.SetTaskLists(ev1Text, ev2Text);
        }

        AddTask(ev1Container, ev1Tasks);
        AddTask(ev2Container, ev2Tasks);
    }

}
