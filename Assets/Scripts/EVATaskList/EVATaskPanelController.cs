using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class EVATaskPanelController : MonoBehaviour
{
    [SerializeField] private GameObject taskItemPrefab;
    [SerializeField] private Transform ev1Container;
    [SerializeField] private Transform ev2Container;
    [SerializeField] private TMP_InputField taskListTitleInput;
    [SerializeField] private Button submitButton;
    [SerializeField] private MissionInfoTabController missionInfoController;
    [SerializeField] private TMP_Text warningText;

    private List<TMP_InputField> ev1Tasks = new List<TMP_InputField>();
    private List<TMP_InputField> ev2Tasks = new List<TMP_InputField>();

    private bool suppressChange = false;

    void Start()
    {
        AddTask(ev1Container, ev1Tasks);
        AddTask(ev2Container, ev2Tasks);

        if (submitButton != null)
            submitButton.onClick.AddListener(ClearAll);

        if (taskListTitleInput != null)
        {
            taskListTitleInput.onValueChanged.AddListener(OnTitleChanged);
        }

        if (PhotonNetwork.IsConnectedAndReady && PhotonNetwork.IsMasterClient)
        {
            TaskSyncResponder.Instance?.BroadcastFullSyncRequest();
        }
    }

    private void OnTitleChanged(string newText)
    {
        if (suppressChange) return;
        TaskSyncResponder.Instance?.BroadcastTaskListTitleUpdate(newText);
    }

    public void SetTaskListTitle(string newTitle)
    {
        suppressChange = true;
        taskListTitleInput.text = newTitle;
        suppressChange = false;
    }

    public void ReceiveFullTaskSync(string[] ev1Texts, string[] ev2Texts)
    {
        suppressChange = true;
        RebuildTaskList(ev1Container, ev1Tasks, ev1Texts);
        RebuildTaskList(ev2Container, ev2Tasks, ev2Texts);
        suppressChange = false;
    }

    public string[] GetEv1Texts() => ev1Tasks.ConvertAll(field => field.text).ToArray();
    public string[] GetEv2Texts() => ev2Tasks.ConvertAll(field => field.text).ToArray();

    private void RebuildTaskList(Transform container, List<TMP_InputField> list, string[] texts)
    {
        foreach (Transform child in container)
            Destroy(child.gameObject);
        list.Clear();

        for (int i = 0; i < texts.Length; i++)
        {
            CreateTask(container, list, texts[i], i);
        }
    }

    void AddTask(Transform container, List<TMP_InputField> taskList)
    {
        CreateTask(container, taskList, "", taskList.Count);
    }

    void CreateTask(Transform container, List<TMP_InputField> list, string text, int index)
    {
        GameObject taskItem = Instantiate(taskItemPrefab, container);
        TMP_Text numLabel = taskItem.transform.Find("NumberLabel")?.GetComponent<TMP_Text>();
        TMP_InputField fld = taskItem.transform.Find("TaskInputField")?.GetComponent<TMP_InputField>();
        TaskInputFieldMonitor mon = taskItem.AddComponent<TaskInputFieldMonitor>();

        if (numLabel != null) numLabel.text = (index + 1) + ".";
        if (fld == null) return;

        fld.lineType = TMP_InputField.LineType.SingleLine;
        fld.text = text;
        fld.onSubmit.AddListener(_ => InsertTaskAfterNetworked(fld, container));
        fld.onValueChanged.AddListener(value =>
        {
            if (suppressChange) return;
            int fieldIndex = list.IndexOf(fld);
            string containerName = container == ev1Container ? "EV1" : "EV2";
            TaskSyncResponder.Instance?.BroadcastTaskTextUpdate(containerName, fieldIndex, value);
        });

        list.Insert(index, fld);
        mon.inputField = fld;
        mon.Initialize(this, container);
    }

    public void InsertTaskAfterNetworked(TMP_InputField currentField, Transform container)
    {
        int index = container == ev1Container ? ev1Tasks.IndexOf(currentField) : ev2Tasks.IndexOf(currentField);
        if (index == -1) return;

        string containerName = container == ev1Container ? "EV1" : "EV2";
        TaskSyncResponder.Instance?.BroadcastInsertTask(containerName, index, "");
    }

    public void InsertTaskAtIndex(string containerName, int index, string text)
    {
        Transform container = containerName == "EV1" ? ev1Container : ev2Container;
        List<TMP_InputField> list = containerName == "EV1" ? ev1Tasks : ev2Tasks;

        GameObject taskItem = Instantiate(taskItemPrefab, container);
        TMP_Text numLabel = taskItem.transform.Find("NumberLabel")?.GetComponent<TMP_Text>();
        TMP_InputField fld = taskItem.transform.Find("TaskInputField")?.GetComponent<TMP_InputField>();
        TaskInputFieldMonitor mon = taskItem.AddComponent<TaskInputFieldMonitor>();

        if (numLabel != null) numLabel.text = (index + 2) + ".";
        if (fld == null) return;

        fld.lineType = TMP_InputField.LineType.SingleLine;
        fld.text = text;
        fld.onSubmit.AddListener(_ => InsertTaskAfterNetworked(fld, container));
        fld.onValueChanged.AddListener(value =>
        {
            if (suppressChange) return;
            int fieldIndex = list.IndexOf(fld);
            TaskSyncResponder.Instance?.BroadcastTaskTextUpdate(containerName, fieldIndex, value);
        });

        list.Insert(index + 1, fld);
        mon.inputField = fld;
        mon.Initialize(this, container);

        taskItem.transform.SetParent(container, false);
        taskItem.transform.SetSiblingIndex(index + 1);

        RenumberTasks(container, list);
    }

    public void DeleteTaskAtIndex(string containerName, int index)
    {
        List<TMP_InputField> list = containerName == "EV1" ? ev1Tasks : ev2Tasks;
        Transform container = containerName == "EV1" ? ev1Container : ev2Container;

        if (list.Count <= 1 || index < 0 || index >= list.Count) return;

        TMP_InputField toDelete = list[index];
        list.RemoveAt(index);
        Destroy(toDelete.transform.parent.gameObject);
        RenumberTasks(container, list);
    }

    private void RenumberTasks(Transform container, List<TMP_InputField> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            Transform tf = list[i].transform.parent;
            TMP_Text label = tf.Find("NumberLabel")?.GetComponent<TMP_Text>();
            if (label != null)
                label.text = (i + 1) + ".";
        }
    }

    public void TryDeleteTask(TMP_InputField inputField, Transform container)
    {
        List<TMP_InputField> list = container == ev1Container ? ev1Tasks : ev2Tasks;
        int index = list.IndexOf(inputField);
        if (index != -1)
        {
            string containerName = container == ev1Container ? "EV1" : "EV2";
            TaskSyncResponder.Instance?.BroadcastDeleteTask(containerName, index);
        }
    }
    public void ReceiveTaskTextUpdate(string containerName, int index, string newText)
    {
        List<TMP_InputField> list = containerName == "EV1" ? ev1Tasks : ev2Tasks;
        if (index < 0 || index >= list.Count) return;

        TMP_InputField targetField = list[index];
        if (targetField != null)
        {
            suppressChange = true;
            targetField.text = newText;
            suppressChange = false;
        }
    }

    public void FocusNextField(Transform container, TMP_InputField current)
    {
        List<TMP_InputField> list = container == ev1Container ? ev1Tasks : ev2Tasks;
        int index = list.IndexOf(current);
        if (index != -1 && index + 1 < list.Count)
        {
            TMP_InputField next = list[index + 1];
            StartCoroutine(FocusNextFrame(next));
        }
    }

    private IEnumerator<UnityEngine.WaitForEndOfFrame> FocusNextFrame(TMP_InputField input)
    {
        yield return new WaitForEndOfFrame();
        input.Select();
        input.ActivateInputField();
    }

    public void ClearAll()
    {
        string title = taskListTitleInput.text.Trim();
        if (string.IsNullOrEmpty(title)) { ShowWarning("Please provide a title for the task list."); return; }
        bool hasEv1Content = ev1Tasks.Exists(t => !string.IsNullOrWhiteSpace(t.text));
        bool hasEv2Content = ev2Tasks.Exists(t => !string.IsNullOrWhiteSpace(t.text));
        if (!hasEv1Content || !hasEv2Content)
        {
            ShowWarning("Both EV1 and EV2 must have at least one task.");
            return;
        }

        string ev1Body = title + "\n";
        string ev2Body = title + "\n";
        foreach (var task in ev1Tasks)
            if (!string.IsNullOrWhiteSpace(task.text)) ev1Body += "- " + task.text.Trim() + "\n";
        foreach (var task in ev2Tasks)
            if (!string.IsNullOrWhiteSpace(task.text)) ev2Body += "- " + task.text.Trim() + "\n";

        TaskSyncResponder.Instance?.BroadcastTaskClear(title, ev1Body, ev2Body);
    }

    public void PerformClearAll(string title, string ev1Body, string ev2Body)
    {
        suppressChange = true;

        foreach (var task in ev1Tasks)
            Destroy(task.transform.parent.gameObject);
        foreach (var task in ev2Tasks)
            Destroy(task.transform.parent.gameObject);

        ev1Tasks.Clear();
        ev2Tasks.Clear();

        taskListTitleInput.text = "";

        missionInfoController?.SetTaskLists(title, ev1Body, ev2Body);

        AddTask(ev1Container, ev1Tasks);
        AddTask(ev2Container, ev2Tasks);

        suppressChange = false;
        ShowWarning("");
    }

    private void ShowWarning(string message)
    {
        if (warningText != null)
        {
            warningText.text = message;
            warningText.gameObject.SetActive(!string.IsNullOrEmpty(message));
        }
        else
        {
            Debug.LogWarning(message);
        }
    }
}
