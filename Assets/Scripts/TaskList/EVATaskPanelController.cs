
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class EVATaskPanelController : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown poiDropdown;
    [SerializeField] private GameObject taskItemPrefab;
    [SerializeField] private Transform ev1Container;
    [SerializeField] private Transform ev2Container;
    [SerializeField] private TMP_InputField taskListTitleInput;
    [SerializeField] private Button submitButton;
    [SerializeField] private MissionInfoTabController missionInfoController;
    [SerializeField] private TMP_Text warningText;
    [SerializeField] private GameObject EV1HeaderRow;
    [SerializeField] private GameObject EV2HeaderRow;

    private List<TMP_InputField> ev1Tasks = new();
    private List<TMP_InputField> ev2Tasks = new();
    private List<TMP_InputField> ev1Durations = new();
    private List<TMP_InputField> ev2Durations = new();

    private Dictionary<string, List<TaskEntry>> savedEv1Tasks = new();
    private Dictionary<string, List<TaskEntry>> savedEv2Tasks = new();

    private bool suppressChange = false;

    void Start()
    {
        PopulateDropdown();
        poiDropdown.onValueChanged.AddListener(OnDropdownChanged);

        if (submitButton != null)
            submitButton.onClick.AddListener(ClearAll);

        if (taskListTitleInput != null)
            taskListTitleInput.onValueChanged.AddListener(OnTitleChanged);

        if (PhotonNetwork.IsConnectedAndReady && PhotonNetwork.IsMasterClient)
            TaskSyncResponder.Instance?.BroadcastFullSyncRequest();
    }

    private void PopulateDropdown()
    {
        poiDropdown.ClearOptions();
        List<string> options = new() { "Select a Point of Interest" };
        foreach (var poi in ConfigLoader.MapConfig.Mapping.POIs)
            options.Add(poi.description.Split('|')[0]);
        poiDropdown.AddOptions(options);
        poiDropdown.value = 0;
    }

    private void OnDropdownChanged(int index)
    {
        if (index == 0)
        {
            taskListTitleInput.text = "";
            ClearTaskList(ev1Container, ev1Tasks, ev1Durations);
            ClearTaskList(ev2Container, ev2Tasks, ev2Durations);
            return;
        }

        var selectedPOI = ConfigLoader.MapConfig.Mapping.POIs[index - 1];
        string poiName = selectedPOI.description.Split('|')[0];
        string templateType = selectedPOI.type;

        taskListTitleInput.text = poiName;

        if (savedEv1Tasks.TryGetValue(poiName, out var ev1ListToLoad) &&
            savedEv2Tasks.TryGetValue(poiName, out var ev2ListToLoad))
        {
            RebuildTaskList(ev1Container, ev1Tasks, ev1Durations, ev1ListToLoad);
            RebuildTaskList(ev2Container, ev2Tasks, ev2Durations, ev2ListToLoad);
        }
        else
        {
            var template = ConfigLoader.TaskPlanning.POIs.Find(p => p.Name == templateType);
            if (template == null)
            {
                Debug.LogWarning($"No template found for type '{templateType}'");
                return;
            }

            RebuildTaskList(ev1Container, ev1Tasks, ev1Durations, template.Tasks);
            RebuildTaskList(ev2Container, ev2Tasks, ev2Durations, template.Tasks);
        }

        if (EV1HeaderRow != null)
            Debug.Log("Setting EV1HeaderRow to true");
            EV1HeaderRow.SetActive(true);

        if (EV2HeaderRow != null)
            EV2HeaderRow.SetActive(true);
    }

    private void ClearTaskList(Transform container, List<TMP_InputField> taskList, List<TMP_InputField> durationList)
    {
        foreach (Transform child in container)
            if (child.name != "EV1HeaderRow")  // <-- Don't destroy the header!
                Destroy(child.gameObject);
        taskList.Clear();
        durationList.Clear();
    }

    private void CreateTask(Transform container, List<TMP_InputField> taskList, List<TMP_InputField> durationList, TaskEntry task, int index)
    {
        GameObject item = Instantiate(taskItemPrefab, container);
        TMP_Text numberLabel = item.transform.Find("NumberLabel")?.GetComponent<TMP_Text>();
        TMP_InputField taskField = item.transform.Find("TaskInputField")?.GetComponent<TMP_InputField>();
        TMP_InputField durationField = item.transform.Find("DurationInputField")?.GetComponent<TMP_InputField>();

        if (numberLabel != null) numberLabel.text = (index + 1) + ".";
        if (taskField != null) taskField.text = task.Description;
        if (durationField != null) durationField.text = task.Duration.ToString();

        bool isRequired = task.Required;
        if (taskField != null)
        {
            taskField.interactable = !isRequired;
            taskField.image.color = isRequired ? Color.white : new Color(0.9f, 0.95f, 1f);
        }

        if (durationField != null)
        {
            durationField.interactable = !isRequired;
            durationField.image.color = isRequired ? Color.white : new Color(0.9f, 0.95f, 1f);
        }

        taskList.Insert(index, taskField);
        durationList.Insert(index, durationField);
    }

    private void RebuildTaskList(Transform container, List<TMP_InputField> taskList, List<TMP_InputField> durationList, List<TaskEntry> tasks)
    {
        ClearTaskList(container, taskList, durationList);
        if (EV1HeaderRow != null)
            Debug.Log("Setting EV1HeaderRow to true");
            EV1HeaderRow.SetActive(true);

        if (EV2HeaderRow != null)
            EV2HeaderRow.SetActive(true);
        for (int i = 0; i < tasks.Count; i++)
        {
            CreateTask(container, taskList, durationList, tasks[i], i);
        }
    }

    private void OnTitleChanged(string newTitle)
    {
        if (suppressChange) return;
        TaskSyncResponder.Instance?.BroadcastTaskListTitleUpdate(newTitle);
    }

    public void SetTaskListTitle(string newTitle)
    {
        suppressChange = true;
        taskListTitleInput.text = newTitle;
        suppressChange = false;
    }

    public void ClearAll()
    {
        string title = taskListTitleInput.text.Trim();
        if (string.IsNullOrEmpty(title)) { ShowWarning("Please provide a title for the task list."); return; }

        List<TaskEntry> ev1Final = new(), ev2Final = new();
        string ev1Body = title + "\n", ev2Body = title + "\n";
        bool hasEv1 = false, hasEv2 = false;

        for (int i = 0; i < ev1Tasks.Count; i++)
        {
            string text = ev1Tasks[i].text.Trim();
            int.TryParse(ev1Durations[i].text, out int duration);
            if (!string.IsNullOrWhiteSpace(text))
            {
                var required = !ev1Tasks[i].interactable;
                ev1Final.Add(new TaskEntry { Description = text, Duration = duration, Required = required });
                if (duration > 0) { ev1Body += "- " + text + "\n"; hasEv1 = true; }
            }
        }

        for (int i = 0; i < ev2Tasks.Count; i++)
        {
            string text = ev2Tasks[i].text.Trim();
            int.TryParse(ev2Durations[i].text, out int duration);
            if (!string.IsNullOrWhiteSpace(text))
            {
                var required = !ev2Tasks[i].interactable;
                ev2Final.Add(new TaskEntry { Description = text, Duration = duration, Required = required });
                if (duration > 0) { ev2Body += "- " + text + "\n"; hasEv2 = true; }
            }
        }

        if (!hasEv1 || !hasEv2)
        {
            ShowWarning("Both EV1 and EV2 must have at least one task with a non-zero duration.");
            return;
        }

        savedEv1Tasks[title] = ev1Final;
        savedEv2Tasks[title] = ev2Final;

        TaskSyncResponder.Instance?.BroadcastTaskClear(title, ev1Body, ev2Body);

        if (EV1HeaderRow != null)
            Debug.Log("Setting EV1HeaderRow to false");
            EV1HeaderRow.SetActive(false);

        if (EV2HeaderRow != null)
            EV2HeaderRow.SetActive(false);
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

    public void InsertTaskAtIndex(string containerName, int index, string text)
    {
        Transform container = containerName == "EV1" ? ev1Container : ev2Container;
        List<TMP_InputField> tasks = containerName == "EV1" ? ev1Tasks : ev2Tasks;
        List<TMP_InputField> durations = containerName == "EV1" ? ev1Durations : ev2Durations;

        //CreateTask(container, taskList, durationList, new TaskEntry { Description = "", Duration = 0, Required = false }, taskList.Count);
        RenumberTasks(container, tasks);
    }

    public void DeleteTaskAtIndex(string containerName, int index)
    {
        List<TMP_InputField> tasks = containerName == "EV1" ? ev1Tasks : ev2Tasks;
        List<TMP_InputField> durations = containerName == "EV1" ? ev1Durations : ev2Durations;
        Transform container = containerName == "EV1" ? ev1Container : ev2Container;

        if (tasks.Count <= 1 || index < 0 || index >= tasks.Count) return;

        Destroy(tasks[index].transform.parent.gameObject);
        tasks.RemoveAt(index);
        durations.RemoveAt(index);
        RenumberTasks(container, tasks);
    }

    private void RenumberTasks(Transform container, List<TMP_InputField> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            TMP_Text label = list[i].transform.parent.Find("NumberLabel")?.GetComponent<TMP_Text>();
            if (label != null)
                label.text = (i + 1) + ".";
        }
    }

    public void ReceiveFullTaskSync(string[] ev1Texts, string[] ev2Texts)
    {
        suppressChange = true;

        RebuildTaskList(ev1Container, ev1Tasks, ev1Durations, ConvertToEntries(ev1Texts));
        RebuildTaskList(ev2Container, ev2Tasks, ev2Durations, ConvertToEntries(ev2Texts));

        suppressChange = false;
    }

    public string[] GetEv1Texts() => ev1Tasks.ConvertAll(field => field.text).ToArray();
    public string[] GetEv2Texts() => ev2Tasks.ConvertAll(field => field.text).ToArray();

    private List<TaskEntry> ConvertToEntries(string[] texts)
    {
        List<TaskEntry> list = new();
        foreach (var text in texts)
        {
            list.Add(new TaskEntry { Description = text, Duration = 0 });
        }
        return list;
    }

    public void PerformClearAll(string title, string ev1Body, string ev2Body)
    {
        suppressChange = true;
        ClearTaskList(ev1Container, ev1Tasks, ev1Durations);
        ClearTaskList(ev2Container, ev2Tasks, ev2Durations);
        taskListTitleInput.text = title;
        missionInfoController?.SetTaskLists(title, ev1Body, ev2Body);
        if (poiDropdown != null)
            poiDropdown.value = 0;
        suppressChange = false;
        ShowWarning("");
    }

    private void ShowWarning(string msg)
    {
        if (warningText != null)
        {
            warningText.text = msg;
            warningText.gameObject.SetActive(!string.IsNullOrEmpty(msg));
        }
    }
}
