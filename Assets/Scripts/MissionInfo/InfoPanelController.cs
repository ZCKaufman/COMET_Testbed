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
        var mission = MissionLoader.Instance.CurrentMission;
        string role = PhotonNetwork.LocalPlayer.NickName;

        if (mission == null)
        {
            Debug.LogError("[MissionInfoPanel] No mission loaded.");
            return;
        }

        // Mission Info
        missionText.text = mission.MissionInfo.All.MissionInfo;

        // Role-specific Task Info
        if (role == "EVA" && mission.MissionInfo.EVA != null)
            taskText.text = mission.MissionInfo.EVA.TaskInfo;
        else if (role == "IVA" && mission.MissionInfo.IVA != null)
            taskText.text = mission.MissionInfo.IVA.TaskInfo;
        else
            taskText.text = "No task info available for this role.";

        // Alerts (optional)
        alertText.text = string.IsNullOrEmpty(mission.MissionInfo.All.Alerts)
            ? "No alerts at this time."
            : mission.MissionInfo.All.Alerts;
    }
}
