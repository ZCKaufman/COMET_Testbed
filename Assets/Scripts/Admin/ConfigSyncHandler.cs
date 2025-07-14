using System.Linq;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using System.Collections;
using PhotonHashtable = ExitGames.Client.Photon.Hashtable;


public class ConfigSyncHandler : MonoBehaviourPunCallbacks
{
    public override void OnRoomPropertiesUpdate(PhotonHashtable changedProps)
    {
        if (changedProps.ContainsKey("ConfigFile"))
        {
            string filename = changedProps["ConfigFile"] as string;
            Debug.Log("[Sync] Received new config: " + filename);
            StartCoroutine(LoadConfigAndSwitchScene(filename));
        }
    }

    private IEnumerator LoadConfigAndSwitchScene(string filename)
    {
        // Reset flag and start loading config
        ConfigLoader.IsLoaded = false;
        ConfigLoader.Instance.LoadConfigFromFilename(filename);

        // Wait until config is done loading
        float timeout = 5f;
        float timer = 0f;

        while (!ConfigLoader.IsLoaded && timer < timeout)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        if (ConfigLoader.IsLoaded)
        {
            Debug.Log("[Sync] Config loaded. Now switching to scene based on role.");
        }
        else
        {
            Debug.LogWarning("[Sync] Timed out waiting for config to load.");
        }

        LoadSceneBasedOnRole();
    }

    void LoadSceneBasedOnRole()
    {
        string role = GlobalManager.Instance.PlayerRole;

        if (role == "EVA")
        {
            SceneManager.LoadScene("EVA_Mission");
        }
        else if (role == "IVA")
        {
            SceneManager.LoadScene("IVA_Mission");
        }
        else if (role == "Admin")
        {
            SceneManager.LoadScene("Admin");
        }
        else
        {
            Debug.LogWarning("Unknown role: " + role);
        }
    }
}
