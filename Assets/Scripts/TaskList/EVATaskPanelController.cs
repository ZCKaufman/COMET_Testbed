using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using System;

public class EVATaskPanelController : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown poiDropdown;
    [SerializeField] private GameObject taskItemPrefab;
    [SerializeField] private Transform ev1Container;
    [SerializeField] private Transform ev2Container;
    [SerializeField] private Button submitButton;
    [SerializeField] private MissionInfoTabController missionInfoController;
    [SerializeField] private ObjectiveVerificationController objectiveVerificationController;
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

    private string currentPOIName = "";

    void Start()
    {
        PopulateDropdown();
        poiDropdown.onValueChanged.AddListener(OnDropdownChanged);

        if (submitButton != null)
            submitButton.onClick.AddListener(ClearAll);

        if (PhotonNetwork.IsConnectedAndReady && PhotonNetwork.IsMasterClient)
            TaskSyncResponder.Instance?.BroadcastFullSyncRequest();
    }

    void OnEnable()
    {
        if (poiDropdown != null && poiDropdown.value > 0)
        {
            Debug.Log("[EVATaskPanelController] Rebuilding task list on enable");
            OnDropdownChanged(poiDropdown.value);
        }

        if (GlobalManager.Instance != null)
        {
            GlobalManager.Instance.OnTaskListSummaryUpdated += HandleGlobalDurationUpdate;
        }
    }

    void OnDisable()
    {
        if (GlobalManager.Instance != null)
        {
            GlobalManager.Instance.OnTaskListSummaryUpdated -= HandleGlobalDurationUpdate;
        }
    }
    public void ReceiveDurationUpdate(string poiName, string containerName, int index, int newDuration)
    {
        if (poiName != currentPOIName) return;

        List<TMP_InputField> list = containerName == "EV1" ? ev1Durations : ev2Durations;
        if (index < 0 || index >= list.Count) return;

        TMP_InputField targetField = list[index];
        if (targetField != null)
        {
            suppressChange = true;
            targetField.SetTextWithoutNotify(newDuration.ToString());
            suppressChange = false;
        }
    }


    private void HandleGlobalDurationUpdate(string title, int _, int __)
    {
        if (GlobalManager.Instance.PlayerRole != "EVA") return;
        if (title != currentPOIName) return;

        RefreshDurationsFromGlobalManager();
    }
    public void RefreshDurationsFromGlobalManager()
    {
        if (string.IsNullOrWhiteSpace(currentPOIName)) return;

        for (int i = 0; i < ev1Durations.Count; i++)
        {
            if (GlobalManager.Instance.IndividualDurations.TryGetValue(currentPOIName, out var taskDict) &&
                taskDict.TryGetValue(i, out var roleDict) &&
                roleDict.TryGetValue("EV1", out var ev1Val))
            {
                ev1Durations[i].SetTextWithoutNotify(ev1Val.ToString());
            }
        }

        for (int i = 0; i < ev2Durations.Count; i++)
        {
            if (GlobalManager.Instance.IndividualDurations.TryGetValue(currentPOIName, out var taskDict) &&
                taskDict.TryGetValue(i, out var roleDict) &&
                roleDict.TryGetValue("EV2", out var ev2Val))
            {
                ev2Durations[i].SetTextWithoutNotify(ev2Val.ToString());
            }
        }
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
            ClearTaskList(ev1Container, ev1Tasks, ev1Durations);
            ClearTaskList(ev2Container, ev2Tasks, ev2Durations);
            return;
        }

        var selectedPOI = ConfigLoader.MapConfig.Mapping.POIs[index - 1];
        string poiName = selectedPOI.description.Split('|')[0];
        currentPOIName = poiName;
        string templateType = selectedPOI.type;

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

        if (numberLabel != null)
            numberLabel.text = (index + 1) + ".";

        if (taskField != null)
        {
            taskField.text = task.Description;
            taskField.interactable = false;
            taskField.caretWidth = 0;

            Color requiredColor = new Color(1f, 0.85f, 0.85f);
            Color optionalColor = new Color(0.85f, 0.95f, 1f);
            taskField.image.color = task.Required ? requiredColor : optionalColor;

            var colors = taskField.colors;
            colors.disabledColor = taskField.image.color;
            taskField.colors = colors;
        }

        if (durationField != null)
        {
            string poiName = currentPOIName;
            string role = container == ev1Container ? "EV1" : "EV2";
            int taskIndex = index;

            // === Get individual duration from GlobalManager.Instance.IndividualDurations ===
            int durationValue = task.Duration;
            if (GlobalManager.Instance.IndividualDurations.TryGetValue(poiName, out var poiDict) &&
                poiDict.TryGetValue(taskIndex, out var roleDict) &&
                roleDict.TryGetValue(role, out int individualValue))
            {
                durationValue = individualValue;
            }

            durationField.text = durationValue.ToString();
            durationField.interactable = true;
            durationField.image.color = Color.white;

            durationField.onValueChanged.AddListener((string newValue) =>
            {
                if (int.TryParse(newValue, out int parsed))
                {
                    TaskSyncResponder.Instance?.BroadcastDurationUpdate(poiName,role, taskIndex, parsed);
                    TaskSyncResponder.Instance?.BroadcastDurationForPOI(poiName, taskIndex, parsed);
                    TaskSyncResponder.Instance?.BroadcastIndividualDuration(poiName, taskIndex, role, parsed);
                }
            });
        }

        taskList.Insert(index, taskField);
        durationList.Insert(index, durationField);
    }


    private void RebuildTaskList(Transform container, List<TMP_InputField> taskList, List<TMP_InputField> durationList, List<TaskEntry> tasks)
    {
        ClearTaskList(container, taskList, durationList);
        if (EV1HeaderRow != null)
            //Debug.Log("Setting EV1HeaderRow to true");
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

    public void ClearAll()
    {
        string title = currentPOIName;
        if (string.IsNullOrWhiteSpace(title) || title == "Select a Point of Interest")
        {
            ShowWarning("Please select a valid Point of Interest before submitting.");
            return;
        }

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
        objectiveVerificationController?.SetTaskLists(title, ev1Body, ev2Body);

        if (EV1HeaderRow != null)
            Debug.Log("Setting EV1HeaderRow to false");
        EV1HeaderRow.SetActive(false);

        if (EV2HeaderRow != null)
            EV2HeaderRow.SetActive(false);

        var (durationTotal, roiTotal) = CalculateTotals();
        TaskSyncResponder.Instance?.BroadcastObjectiveTotals(title, durationTotal, roiTotal);

    }

    private (int totalDuration, int totalROI) CalculateTotals()
    {
        int durationSum = 0;
        int roiSum = 0;

        var poiName = currentPOIName;
        var poi = ConfigLoader.MapConfig.Mapping.POIs.Find(p => p.description.StartsWith(poiName));
        if (poi == null) return (0, 0);

        var poiType = poi.type;
        var template = ConfigLoader.TaskPlanning.POIs.Find(p => p.Name == poiType);
        if (template == null) return (0, 0);

        /*for (int i = 0; i < template.Tasks.Count; i++)
        {
            int d1 = i < ev1Durations.Count && int.TryParse(ev1Durations[i].text, out var val1) ? val1 : 0;
            int d2 = i < ev2Durations.Count && int.TryParse(ev2Durations[i].text, out var val2) ? val2 : 0;
            int maxDur = Mathf.Max(d1, d2);
            durationSum += maxDur;

            string equation = template.Tasks[i].roiEquation ?? "0";
            int roiVal = EvaluateEquation(equation, maxDur);
            roiSum += roiVal;
        }*/

        for (int i = 0; i < template.Tasks.Count; i++)
        {
            int d1 = i < ev1Durations.Count && int.TryParse(ev1Durations[i].text, out var val1) ? val1 : 0;
            int d2 = i < ev2Durations.Count && int.TryParse(ev2Durations[i].text, out var val2) ? val2 : 0;
            int maxDur = Mathf.Max(d1, d2);
            durationSum += maxDur;

            string equation = template.Tasks[i].roiEquation ?? "0";
            Debug.Log($"Task {i}: equation='{equation}', maxDur={maxDur}"); // ADD THIS LINE
            
            int roiVal = EvaluateEquation(equation, maxDur);
            Debug.Log($"Task {i}: roiVal={roiVal}"); // ADD THIS LINE
            
            roiSum += roiVal;
        }

        return (durationSum, roiSum);
    }

    private int EvaluateEquation(string expression, float x)
    {
        // Insert explicit multiplication between number and 'x' (e.g., 5x → 5*x)
        expression = System.Text.RegularExpressions.Regex.Replace(expression, @"(\d)(x)", "$1*$2", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        
        // Replace x with the actual value (case insensitive)
        expression = System.Text.RegularExpressions.Regex.Replace(expression, @"\bx\b", x.ToString(), System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        
        // Pre-process log functions
        expression = ProcessLogFunctions(expression);
        
        // Pre-process exponential functions (e^...)
        expression = ProcessExponentialFunctions(expression);
        
        try
        {
            System.Data.DataTable table = new System.Data.DataTable();
            object result = table.Compute(expression, "");
            return int.TryParse(result.ToString(), out int val) ? val : 0;
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning("Invalid ROI equation: " + expression + " | Error: " + ex.Message);
            return 0;
        }
    }

    private string ProcessExponentialFunctions(string expression)
    {
        int startIndex = 0;
        while (true)
        {
            int eIndex = expression.IndexOf("e^", startIndex);
            if (eIndex == -1) break;
            
            int parenStart = eIndex + 2;
            if (parenStart >= expression.Length || expression[parenStart] != '(')
            {
                startIndex = eIndex + 2;
                continue;
            }
            
            // Find matching closing parenthesis
            int depth = 1;
            int parenEnd = parenStart + 1;
            while (parenEnd < expression.Length && depth > 0)
            {
                if (expression[parenEnd] == '(') depth++;
                else if (expression[parenEnd] == ')') depth--;
                parenEnd++;
            }
            
            if (depth != 0)
            {
                // Unmatched parentheses
                startIndex = eIndex + 2;
                continue;
            }
            
            // Extract the exponent (without outer parens)
            string exponent = expression.Substring(parenStart + 1, parenEnd - parenStart - 2);
            
            try
            {
                System.Data.DataTable table = new System.Data.DataTable();
                object exponentResult = table.Compute(exponent, "");
                double exponentVal = Convert.ToDouble(exponentResult);
                double expResult = Math.Exp(exponentVal);
                
                // Replace e^(...) with the result
                string replacement = expResult.ToString(System.Globalization.CultureInfo.InvariantCulture);
                expression = expression.Substring(0, eIndex) + replacement + expression.Substring(parenEnd);
                
                // Continue from where we replaced
                startIndex = eIndex + replacement.Length;
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"Exponential evaluation failed for: {exponent} | Error: {ex.Message}");
                startIndex = eIndex + 2;
            }
        }
        
        return expression;
    }

    private string ProcessLogFunctions(string expression)
    {
        // Handle log10(...)
        expression = System.Text.RegularExpressions.Regex.Replace(
            expression,
            @"log10\(([^)]+)\)",
            match => {
                string inner = match.Groups[1].Value;
                try
                {
                    System.Data.DataTable table = new System.Data.DataTable();
                    object innerResult = table.Compute(inner, "");
                    double innerVal = Convert.ToDouble(innerResult);
                    double logResult = Math.Log10(innerVal);
                    return logResult.ToString(System.Globalization.CultureInfo.InvariantCulture);
                }
                catch
                {
                    return "0";
                }
            }
        );
        
        // Handle log(...) - natural log
        expression = System.Text.RegularExpressions.Regex.Replace(
            expression,
            @"log\(([^)]+)\)",
            match => {
                string inner = match.Groups[1].Value;
                try
                {
                    System.Data.DataTable table = new System.Data.DataTable();
                    object innerResult = table.Compute(inner, "");
                    double innerVal = Convert.ToDouble(innerResult);
                    double logResult = Math.Log(innerVal);
                    return logResult.ToString(System.Globalization.CultureInfo.InvariantCulture);
                }
                catch
                {
                    return "0";
                }
            }
        );
        
        return expression;
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
        if (poiDropdown != null)
            poiDropdown.value = 0;
            currentPOIName = "";
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
