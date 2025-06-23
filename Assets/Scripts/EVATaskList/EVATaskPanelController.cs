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
            StartCoroutine(FocusNextFrame(newFld));
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
            TMP_InputField toDelete = taskList[index];
            taskList.RemoveAt(index);
            Destroy(toDelete.transform.parent.gameObject);

            RenumberTasks(container, taskList);
        }
    }

    void ClearAll()
    {
        string title = taskListTitleInput.text.Trim();

        if (string.IsNullOrEmpty(title))
        {
            ShowWarning("Please provide a title for the task list.");
            return;
        }

        StringBuilder ev1Body = new StringBuilder(title + "\n");
        StringBuilder ev2Body = new StringBuilder(title + "\n");

        bool hasEv1Content = false;
        bool hasEv2Content = false;

        foreach (var task in ev1Tasks)
        {
            string taskText = task.text.Trim();
            if (!string.IsNullOrEmpty(taskText))
            {
                ev1Body.AppendLine("- " + taskText);
                hasEv1Content = true;
            }
        }

        foreach (var task in ev2Tasks)
        {
            string taskText = task.text.Trim();
            if (!string.IsNullOrEmpty(taskText))
            {
                ev2Body.AppendLine("- " + taskText);
                hasEv2Content = true;
            }
        }

        if (!hasEv1Content || !hasEv2Content)
        {
            ShowWarning("Both EV1 and EV2 must have at least one task.");
            return;
        }

        ShowWarning(""); // Clear warning if valid

        foreach (var task in ev1Tasks)
            Destroy(task.transform.parent.gameObject);
        foreach (var task in ev2Tasks)
            Destroy(task.transform.parent.gameObject);

        taskListTitleInput.text = "";
        ev1Tasks.Clear();
        ev2Tasks.Clear();

        if (missionInfoController != null)
            missionInfoController.SetTaskLists(title, ev1Body.ToString(), ev2Body.ToString());

        AddTask(ev1Container, ev1Tasks);
        AddTask(ev2Container, ev2Tasks);
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
