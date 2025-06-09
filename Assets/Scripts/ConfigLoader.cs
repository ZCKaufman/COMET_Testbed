using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class ConfigLoader : MonoBehaviour
{
    public static ConfigRoot LoadedConfig;
    public static bool IsLoaded = false; 

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        StartCoroutine(LoadConfig());
    }

    IEnumerator LoadConfig()
    {
        string path = Path.Combine(Application.streamingAssetsPath, "config.json");

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
            Debug.Log("Loaded config.json (WebGL): " + json);
            LoadedConfig = JsonUtility.FromJson<ConfigRoot>(json);
            IsLoaded = true;
        }
        yield break;
#else
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            Debug.Log("Loaded config.json (local file): " + json);
            LoadedConfig = JsonUtility.FromJson<ConfigRoot>(json);
            IsLoaded = true;
        }
        else
        {
            Debug.LogError("Config file not found at: " + path);
        }
        yield break;
#endif
    }
}
