using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.IO;

#if UNITY_WEBGL
using UnityEngine.Networking;
#endif

[RequireComponent(typeof(TMP_Dropdown))]
public class LoadConfigFiles : MonoBehaviour
{
    private TMP_Dropdown dropdown;
    private const string manifestFileName = "json_manifest.txt";

    void Awake()
    {
        dropdown = GetComponent<TMP_Dropdown>();
    }

    void Start()
    {
        StartCoroutine(LoadDropdownOptions());
    }

    IEnumerator LoadDropdownOptions()
    {
        string path = Path.Combine(Application.streamingAssetsPath, manifestFileName);
        Debug.Log("Looking for manifest at: " + path);
        List<string> jsonFileNames = new List<string>();

    #if UNITY_WEBGL && !UNITY_EDITOR
        using (UnityWebRequest www = UnityWebRequest.Get(path))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Manifest loaded (WebGL): " + www.downloadHandler.text);
                jsonFileNames.AddRange(www.downloadHandler.text.Split('\n'));
            }
            else
            {
                Debug.LogError("Failed to load manifest (WebGL): " + www.error);
                yield break;
            }
        }
    #else
        if (File.Exists(path))
        {
            Debug.Log("Manifest loaded (Local build):");
            jsonFileNames.AddRange(File.ReadAllLines(path));
            foreach (var f in jsonFileNames) Debug.Log(" - " + f);
        }
        else
        {
            Debug.LogError("Manifest file not found at: " + path);
            yield break;
        }
    #endif

        dropdown.ClearOptions();
        dropdown.options.Add(new TMP_Dropdown.OptionData("-- Select Configuration --"));

        foreach (string file in jsonFileNames)
        {
            string trimmed = file.Trim();
            if (!string.IsNullOrEmpty(trimmed))
            {
                Debug.Log("Adding option: " + trimmed);
                dropdown.options.Add(new TMP_Dropdown.OptionData(trimmed));
            }
        }

        dropdown.value = 0;
        dropdown.RefreshShownValue();
    }

}
