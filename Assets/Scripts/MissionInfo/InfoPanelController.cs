using UnityEngine;
using TMPro;
using Photon.Pun;

public class MissionInfoPanelController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI missionText;
    [SerializeField] private TextMeshProUGUI taskText;
    [SerializeField] private TextMeshProUGUI alertText;

    void Start()
    {
        if (!ConfigLoader.IsLoaded)
        {
            Debug.LogError("[MissionInfoPanel] Config not loaded yet.");
            return;
        }

        var missionInfo = ConfigLoader.LoadedConfig.MissionInfo;
        string role = PhotonNetwork.LocalPlayer.NickName;

        if (missionInfo == null)
        {
            Debug.LogError("[MissionInfoPanel] MissionInfo section is null.");
            return;
        }

        missionText.text = missionInfo.All?.MissionInfo ?? "Mission info not available.";

        alertText.text = string.IsNullOrEmpty(missionInfo.All?.Alerts)
            ? "No alerts at this time."
            : missionInfo.All.Alerts;

        if (role == "EVA" && missionInfo.EVA != null)
        {
            taskText.text = missionInfo.EVA.TaskInfo;
        }
        else
        {
            taskText.text = "No task info available for this role.";
        }
    }
}
