using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class ConfigLoader : MonoBehaviour
{
    public static MissionViewConfigRoot MissionConfig;
    public static EVAMappingConfigRoot EVAMapConfig;
    public static bool IsLoaded = false;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        StartCoroutine(LoadConfig());
    }

    IEnumerator LoadConfig()
    {
        string path = Path.Combine(Application.streamingAssetsPath, "Mission001.json");

#if UNITY_WEBGL && !UNITY_EDITOR
        UnityWebRequest www = UnityWebRequest.Get(path);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Failed to load config.json from StreamingAssets (WebGL): " + www.error);
        }
        else
        {
            string json = www.downloadHandler.text;
            ParseBothConfigs(json);
        }
#else
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            ParseConfigs(json);
        }
        else
        {
            Debug.LogError("Config file not found at: " + path);
        }
#endif
        yield break;
    }

    void ParseConfigs(string json)
    {
        try
        {
            // deserialize whole object to a temporary structure
            var fullJson = JsonUtility.FromJson<Wrapper>(json);

            // populate the all separate configs
            MissionConfig = new MissionViewConfigRoot { MissionInfo = fullJson.MissionInfo };
            EVAMapConfig = new EVAMappingConfigRoot { EVAMapping = fullJson.EVAMapping };

            IsLoaded = true;        }
        catch (System.Exception ex)
        {
            Debug.LogError("Failed to parse config: " + ex.Message);
        }
    }

    [System.Serializable]
    private class Wrapper
    {
        public MissionInfoSection MissionInfo;
        public EVAMapping EVAMapping;
    }
}
