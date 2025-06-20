using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

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
        TMP_Text numLabel   = taskItem.transform.Find("NumberLabel") ?.GetComponent<TMP_Text>();
        TMP_InputField fld  = taskItem.transform.Find("TaskInputField")?.GetComponent<TMP_InputField>();
        TaskInputFieldMonitor mon = taskItem.AddComponent<TaskInputFieldMonitor>();

        if (numLabel != null) numLabel.text = taskNumber + ".";
        if (fld == null) return;

        fld.lineType = TMP_InputField.LineType.SingleLine;

        // ——  Add NEW task after this one every time Enter is pressed ——
        fld.onSubmit.RemoveAllListeners();
        fld.onSubmit.AddListener(_ => InsertTaskAfter(fld, container));

        taskList.Add(fld);
        mon.inputField = fld;
        mon.Initialize(this, container);
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

    public void InsertTaskAfter(TMP_InputField currentField, Transform container)
{
    List<TMP_InputField> list = container == ev1Container ? ev1Tasks : ev2Tasks;
    int idx = list.IndexOf(currentField);
    if (idx == -1) return;

    GameObject taskItem = Instantiate(taskItemPrefab, container);
    TMP_Text numLabel   = taskItem.transform.Find("NumberLabel") ?.GetComponent<TMP_Text>();
    TMP_InputField newFld = taskItem.transform.Find("TaskInputField")?.GetComponent<TMP_InputField>();
    TaskInputFieldMonitor mon = taskItem.AddComponent<TaskInputFieldMonitor>();

    if (numLabel != null) numLabel.text = (idx + 2) + ".";
    if (newFld != null)
    {
        newFld.lineType = TMP_InputField.LineType.SingleLine;
        newFld.onSubmit.AddListener(_ => InsertTaskAfter(newFld, container));
        list.Insert(idx + 1, newFld);
        mon.inputField = newFld;
        mon.Initialize(this, container);
        StartCoroutine(FocusNextFrame(newFld));
    }

    taskItem.transform.SetParent(container, false);
    taskItem.transform.SetSiblingIndex(idx + 1);

    RenumberTasks(container, list);
}


    private IEnumerator FocusNextFrame(TMP_InputField field)
    {
        yield return null;
        field.Select();
        field.ActivateInputField();
    }

    public void TryDeleteTask(TMP_InputField inputField, Transform container)
    {
        List<TMP_InputField> taskList = container == ev1Container ? ev1Tasks : ev2Tasks;

        if (taskList.Count <= 1) return;

        int index = taskList.IndexOf(inputField);
        if (index != -1)
        {
            TMP_InputField toDelete = taskList[index];
            taskList.RemoveAt(index);
            Destroy(toDelete.transform.parent.gameObject);

            RenumberTasks(container, taskList);
        }
    }

    void ClearAll()
    {
        string title = taskListTitleInput.text.Trim();
        if (string.IsNullOrWhiteSpace(title))
        {
            title = "Untitled Task List";
        }

        string ev1Body = title + "\n";
        string ev2Body = title + "\n";

        foreach (var task in ev1Tasks)
        {
            if (!string.IsNullOrWhiteSpace(task.text))
                ev1Body += "- " + task.text.Trim() + "\n";

            Destroy(task.transform.parent.gameObject);
        }

        foreach (var task in ev2Tasks)
        {
            if (!string.IsNullOrWhiteSpace(task.text))
                ev2Body += "- " + task.text.Trim() + "\n";

            Destroy(task.transform.parent.gameObject);
        }

        taskListTitleInput.text = "";
        ev1Tasks.Clear();
        ev2Tasks.Clear();

        // Send to Mission Info tab
        if (missionInfoController != null)
        {
            missionInfoController.SetTaskLists(title, ev1Body, ev2Body);
        }

        // Add back one blank task to each list
        AddTask(ev1Container, ev1Tasks);
        AddTask(ev2Container, ev2Tasks);
    }

}
