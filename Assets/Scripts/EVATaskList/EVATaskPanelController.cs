using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Text;
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

    private PhotonView photonView;
    private bool suppressChange = false;


    void Awake()
    {
        photonView = GetComponent<PhotonView>();
    }

    void OnEnable()
    {
        if (photonView.IsMine)
        {
            photonView.RPC("RPC_RequestFullTaskSync", RpcTarget.MasterClient);
        }
    }

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
    }
    private void OnTitleChanged(string newText)
    {
        if (suppressChange) return;
        photonView.RPC("RPC_UpdateTaskListTitle", RpcTarget.Others, newText);
    }
    [PunRPC]
    void RPC_UpdateTaskListTitle(string newTitle)
    {
        if (taskListTitleInput == null) return;

        suppressChange = true;
        taskListTitleInput.text = newTitle;
        suppressChange = false;
    }

    [PunRPC]
    void RPC_RequestFullTaskSync(PhotonMessageInfo info)
    {
        string[] ev1Texts = ev1Tasks.ConvertAll(field => field.text).ToArray();
        string[] ev2Texts = ev2Tasks.ConvertAll(field => field.text).ToArray();

        photonView.RPC("RPC_ReceiveFullTaskSync", info.Sender, ev1Texts, ev2Texts);
    }

    [PunRPC]
    void RPC_ReceiveFullTaskSync(string[] ev1Texts, string[] ev2Texts)
    {
        suppressChange = true;

        RebuildTaskList(ev1Container, ev1Tasks, ev1Texts);
        RebuildTaskList(ev2Container, ev2Tasks, ev2Texts);

        suppressChange = false;
    }

    void RebuildTaskList(Transform container, List<TMP_InputField> list, string[] texts)
    {
        foreach (Transform child in container)
            Destroy(child.gameObject);
        list.Clear();

        for (int i = 0; i < texts.Length; i++)
        {
            GameObject taskItem = Instantiate(taskItemPrefab, container);
            TMP_Text numLabel = taskItem.transform.Find("NumberLabel")?.GetComponent<TMP_Text>();
            TMP_InputField fld = taskItem.transform.Find("TaskInputField")?.GetComponent<TMP_InputField>();
            TaskInputFieldMonitor mon = taskItem.AddComponent<TaskInputFieldMonitor>();

            if (numLabel != null) numLabel.text = (i + 1) + ".";
            if (fld == null) continue;

            fld.lineType = TMP_InputField.LineType.SingleLine;
            fld.text = texts[i];

            fld.onSubmit.AddListener(_ => InsertTaskAfterNetworked(fld, container));
            fld.onValueChanged.AddListener(value =>
            {
                if (suppressChange) return;
                int index = list.IndexOf(fld);
                string containerName = container == ev1Container ? "EV1" : "EV2";
                photonView.RPC("RPC_UpdateTaskText", RpcTarget.Others, containerName, index, value);
            });

            list.Add(fld);
            mon.inputField = fld;
            mon.Initialize(this, container);
        }
    }

    void AddTask(Transform container, List<TMP_InputField> taskList)
    {
        int taskNumber = taskList.Count + 1;

        GameObject taskItem = Instantiate(taskItemPrefab, container);
        TMP_Text numLabel = taskItem.transform.Find("NumberLabel")?.GetComponent<TMP_Text>();
        TMP_InputField fld = taskItem.transform.Find("TaskInputField")?.GetComponent<TMP_InputField>();
        TaskInputFieldMonitor mon = taskItem.AddComponent<TaskInputFieldMonitor>();

        if (numLabel != null) numLabel.text = taskNumber + ".";
        if (fld == null) return;

        fld.lineType = TMP_InputField.LineType.SingleLine;

        // ——  Add NEW task after this one every time Enter is pressed ——
        fld.onSubmit.RemoveAllListeners();
        fld.onSubmit.AddListener(_ => InsertTaskAfterNetworked(fld, container));
         fld.onValueChanged.AddListener(value =>
        {
            if (suppressChange) return;
            int fieldIndex = taskList.IndexOf(fld);
            string containerName = container == ev1Container ? "EV1" : "EV2";
            photonView.RPC("RPC_UpdateTaskText", RpcTarget.Others, containerName, fieldIndex, value);
        });

        taskList.Add(fld);
        mon.inputField = fld;
        mon.Initialize(this, container);
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

     [PunRPC]
    void RPC_InsertTaskAfterWithText(string containerName, int index, string text)
    {
        Transform container = containerName == "EV1" ? ev1Container : ev2Container;
        List<TMP_InputField> list = container == ev1Container ? ev1Tasks : ev2Tasks;

        GameObject taskItem = Instantiate(taskItemPrefab, container);
        TMP_Text numLabel = taskItem.transform.Find("NumberLabel")?.GetComponent<TMP_Text>();
        TMP_InputField newFld = taskItem.transform.Find("TaskInputField")?.GetComponent<TMP_InputField>();
        TaskInputFieldMonitor mon = taskItem.AddComponent<TaskInputFieldMonitor>();

        if (numLabel != null) numLabel.text = (index + 2) + ".";

        if (newFld != null)
        {
            newFld.lineType = TMP_InputField.LineType.SingleLine;
            newFld.text = text;

            newFld.onSubmit.AddListener(_ => InsertTaskAfterNetworked(newFld, container));

            // Live update sync
            newFld.onValueChanged.AddListener(value =>
            {
                if (suppressChange) return;
                int fieldIndex = list.IndexOf(newFld);
                photonView.RPC("RPC_UpdateTaskText", RpcTarget.Others, containerName, fieldIndex, value);
            });

            list.Insert(index + 1, newFld);
            mon.inputField = newFld;
            mon.Initialize(this, container);
            if (this.gameObject.activeInHierarchy)
            {
                StartCoroutine(FocusNextFrame(newFld));
            }

        }

        taskItem.transform.SetParent(container, false);
        taskItem.transform.SetSiblingIndex(index + 1);

        RenumberTasks(container, list);
    }
    [PunRPC]
    void RPC_UpdateTaskText(string containerName, int index, string newText)
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

    private IEnumerator<UnityEngine.WaitForEndOfFrame> FocusNextFrame(TMP_InputField input)
    {
        yield return new WaitForEndOfFrame();
        input.Select();
        input.ActivateInputField();
    }
    public void InsertTaskAfterNetworked(TMP_InputField currentField, Transform container)
    {
        int index = container == ev1Container ? ev1Tasks.IndexOf(currentField) : ev2Tasks.IndexOf(currentField);
        if (index == -1) return;

        string containerName = container == ev1Container ? "EV1" : "EV2";
        string text = "";

        photonView.RPC("RPC_InsertTaskAfterWithText", RpcTarget.All, containerName, index, text);
    }


    public void TryDeleteTask(TMP_InputField inputField, Transform container)
    {
        List<TMP_InputField> taskList = container == ev1Container ? ev1Tasks : ev2Tasks;

        if (taskList.Count <= 1) return;

        int index = taskList.IndexOf(inputField);
        if (index != -1)
        {
            string containerName = container == ev1Container ? "EV1" : "EV2";
            photonView.RPC("RPC_TryDeleteTaskAtIndex", RpcTarget.All, containerName, index);
        }
    }

    [PunRPC]
    void RPC_TryDeleteTaskAtIndex(string containerName, int index)
    {
        List<TMP_InputField> taskList = containerName == "EV1" ? ev1Tasks : ev2Tasks;
        Transform container = containerName == "EV1" ? ev1Container : ev2Container;

        if (taskList.Count <= 1 || index < 0 || index >= taskList.Count) return;

        TMP_InputField toDelete = taskList[index];
        taskList.RemoveAt(index);
        Destroy(toDelete.transform.parent.gameObject);

        RenumberTasks(container, taskList);
    }

    void ClearAll()
    {
        string title = taskListTitleInput.text.Trim();

        if (string.IsNullOrEmpty(title))
        {
            ShowWarning("Please provide a title for the task list.");
            return;
        }

        bool hasEv1Content = ev1Tasks.Exists(t => !string.IsNullOrWhiteSpace(t.text));
        bool hasEv2Content = ev2Tasks.Exists(t => !string.IsNullOrWhiteSpace(t.text));

        if (!hasEv1Content || !hasEv2Content)
        {
            ShowWarning("Both EV1 and EV2 must have at least one task.");
            return;
        }

        // Prepare body text to pass along with the RPC
        string ev1Body = title + "\n";
        string ev2Body = title + "\n";

        foreach (var task in ev1Tasks)
            if (!string.IsNullOrWhiteSpace(task.text))
                ev1Body += "- " + task.text.Trim() + "\n";

        foreach (var task in ev2Tasks)
            if (!string.IsNullOrWhiteSpace(task.text))
                ev2Body += "- " + task.text.Trim() + "\n";

        //full body to everyone
        photonView.RPC("RPC_ClearAllTasks", RpcTarget.All, "", ev1Body, ev2Body);
    }

    [PunRPC]
    void RPC_ClearAllTasks(string title, string ev1Body, string ev2Body)
    {
        suppressChange = true;

        foreach (var task in ev1Tasks)
            Destroy(task.transform.parent.gameObject);
        foreach (var task in ev2Tasks)
            Destroy(task.transform.parent.gameObject);

        ev1Tasks.Clear();
        ev2Tasks.Clear();

        taskListTitleInput.text = title;
        PhotonView.Find(997).RPC("RPC_GlobalTaskUpdate", RpcTarget.All, title, ev1Body, ev2Body);
        // Set task lists on the mission tab controller
        if (missionInfoController != null)
        {
            
            missionInfoController.SetTaskLists(title, ev1Body, ev2Body);
        }

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
