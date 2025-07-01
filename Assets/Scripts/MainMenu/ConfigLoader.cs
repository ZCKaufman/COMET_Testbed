using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class ConfigLoader : MonoBehaviour
{
    public static MissionViewConfigRoot MissionConfig;
    public static EVAMappingConfigRoot  EVAMapConfig;
    public static IVASampleTaskRoot     IVASampleTasks;

    public static bool IsLoaded = false;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        StartCoroutine(WaitForGlobalManagerAndLoadConfig());
    }

    IEnumerator WaitForGlobalManagerAndLoadConfig()
    {
        // Wait until GlobalManager is ready and has a valid file set
        while (GlobalManager.Instance == null || string.IsNullOrEmpty(GlobalManager.Instance.SelectedMissionFile))
        {
            yield return null;
        }

        string filename = GlobalManager.Instance.SelectedMissionFile;
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

    [System.Serializable] private class Wrapper
    {
        public MissionInfoSection MissionInfo;
        public EVAMapping         EVAMapping;
        public IVATaskArray       EVATaskPlanning;
    }

    void ParseConfigs(string json)
    {
        try
        {
            var full = JsonUtility.FromJson<Wrapper>(json);

            MissionConfig = new MissionViewConfigRoot { MissionInfo = full.MissionInfo };
            EVAMapConfig  = new EVAMappingConfigRoot  { EVAMapping  = full.EVAMapping  };

            IVASampleTasks = new IVASampleTaskRoot();
            foreach (var entry in full.EVATaskPlanning.IVA)
                IVASampleTasks.Samples[entry.Name] = entry;

            Debug.Log("[ConfigLoader] Loaded config from: " + GlobalManager.Instance.SelectedMissionFile);
            IsLoaded = true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Config parse error: " + ex.Message);
        }
    }
}
