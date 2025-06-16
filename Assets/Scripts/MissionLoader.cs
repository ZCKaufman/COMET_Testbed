using UnityEngine;
using System.IO;

public class MissionLoader : MonoBehaviour
{
    public static MissionLoader Instance { get; private set; }

    public MissionData CurrentMission;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Optional: persist across scenes
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void LoadMission(string filename)
    {
        string path = Path.Combine(Application.streamingAssetsPath, filename);
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            CurrentMission = JsonUtility.FromJson<MissionData>(json);
            Debug.Log("[MissionLoader] Mission loaded.");
        }
        else
        {
            Debug.LogError($"[MissionLoader] Mission file not found: {path}");
        }
    }
}
