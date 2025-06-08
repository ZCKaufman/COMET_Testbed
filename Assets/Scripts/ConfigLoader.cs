using System.IO;
using UnityEngine;


public class ConfigLoader : MonoBehaviour
{
    public static ConfigRoot LoadedConfig;


    void Awake()
    {
        string path = Path.Combine(Application.streamingAssetsPath, "config.json");
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            Debug.Log("Loaded JSON: " + json);
            LoadedConfig = JsonUtility.FromJson<ConfigRoot>(json);
        }
        else
        {
            Debug.LogError("Config file not found: " + path);
        }
    }


}
