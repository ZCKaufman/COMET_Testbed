using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;

public class AdminPanelUI : MonoBehaviour
{
    public TMP_Dropdown missionDropdown;
    public Button applyButton;
    private List<string> missionFiles = new();

    IEnumerator Start()
    {
        // Wait until GlobalManager is initialized
        while (GlobalManager.Instance == null)
            yield return null;

        yield return StartCoroutine(LoadMissionList());

        applyButton.onClick.AddListener(() =>
        {
            int selectedIndex = missionDropdown.value;
            if (selectedIndex >= 0 && selectedIndex < missionFiles.Count)
            {
                string selectedFile = missionFiles[selectedIndex];
                GlobalManager.Instance.SetSelectedMission(selectedFile);
                Debug.Log($"[AdminPanelUI] Mission set to {selectedFile}");
            }
        });
    }

    IEnumerator LoadMissionList()
    {
        string url = System.IO.Path.Combine(Application.streamingAssetsPath, "MissionsList.txt");

        UnityWebRequest www = UnityWebRequest.Get(url);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("[AdminPanelUI] Failed to load MissionsList.txt: " + www.error);
            missionDropdown.ClearOptions();
            missionDropdown.AddOptions(new List<string> { "No missions found" });
            yield break;
        }

        string[] lines = www.downloadHandler.text.Split('\n');
        missionFiles.Clear();
        missionDropdown.ClearOptions();

        List<string> displayNames = new();

        foreach (string line in lines)
        {
            string trimmed = line.Trim();
            if (!string.IsNullOrEmpty(trimmed))
            {
                missionFiles.Add(trimmed);
                displayNames.Add(System.IO.Path.GetFileNameWithoutExtension(trimmed));
            }
        }

        if (displayNames.Count == 0)
        {
            displayNames.Add("No .json files found");
        }

        missionDropdown.AddOptions(displayNames);
    }
}
