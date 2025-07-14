using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;

public class ConfigLoader : MonoBehaviour
{
    // ---------- Public static configs ----------
    public static MissionViewConfigRoot MissionConfig;
    public static MappingConfigRoot MapConfig;
    public static TaskPlanningSection TaskPlanning;

    public static bool IsLoaded = false;
    public static ConfigLoader Instance;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            StartCoroutine(LoadConfig());
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ---------- Public method to load config manually ----------
    public void LoadConfigFromFilename(string filename)
    {
        StartCoroutine(LoadConfig(filename));
    }

    // ---------- Load config by filename (default is Mission001.json) ----------
    public IEnumerator LoadConfig(string filename = "Mission001.json")
    {
        IsLoaded = false;
        string path = Path.Combine(Application.streamingAssetsPath, filename);

#if UNITY_WEBGL && !UNITY_EDITOR
        UnityWebRequest www = UnityWebRequest.Get(path);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("JSON load error (WebGL): " + www.error);
        }
        else
        {
            ParseConfigs(www.downloadHandler.text);
        }
#else
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            ParseConfigs(json);
        }
        else
        {
            Debug.LogError("JSON file not found: " + path);
        }
#endif

        yield break;
    }

    // ---------- JSON parse wrapper ----------
    [System.Serializable]
    private class Wrapper
    {
        public MissionInfoSection MissionInfo;
        public Mapping Mapping;
        public TaskPlanningSection TaskPlanning;
    }

    void ParseConfigs(string json)
    {
        try
        {
            var full = JsonConvert.DeserializeObject<Wrapper>(json);

            MissionConfig = new MissionViewConfigRoot { MissionInfo = full.MissionInfo };
            MapConfig = new MappingConfigRoot { Mapping = full.Mapping };
            TaskPlanning = full.TaskPlanning;

            IsLoaded = true;
            Debug.Log("Config loaded successfully.");
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Config parse error: " + ex.Message);
        }
    }
}
