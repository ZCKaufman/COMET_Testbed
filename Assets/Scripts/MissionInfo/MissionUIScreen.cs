using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class MissionUIManager : MonoBehaviourPunCallbacks
{
    [Header("Buttons")]
    [SerializeField] private Button missionInfoButton;
    [SerializeField] private Button evaMappingButton;
    [SerializeField] private Button evaTasksButton;
    [SerializeField] private Button ivaPlanningButton;

    [Header("Panels")]
    [SerializeField] private GameObject missionInfoPanel;
    [SerializeField] private GameObject evaMappingPanel;
    [SerializeField] private GameObject evaTasksPanel;
    [SerializeField] private GameObject ivaPlanningPanel;

    private string userRole;

    public string UserRole => userRole;

    void Start()
    {
        userRole = PhotonNetwork.LocalPlayer.NickName;
        HookupButtons();
        ShowPanel(missionInfoPanel); // Default tab
    }

    void HookupButtons()
    {
        missionInfoButton.onClick.AddListener(() => ShowPanel(missionInfoPanel));
        evaMappingButton.onClick.AddListener(() => ShowPanel(evaMappingPanel));
        evaTasksButton.onClick.AddListener(() => ShowPanel(evaTasksPanel));
        ivaPlanningButton.onClick.AddListener(() => ShowPanel(ivaPlanningPanel));
    }

    void ShowPanel(GameObject panelToShow)
    {
        missionInfoPanel.SetActive(false);
        evaMappingPanel.SetActive(false);
        evaTasksPanel.SetActive(false);
        ivaPlanningPanel.SetActive(false);

        if (panelToShow != null)
            panelToShow.SetActive(true);
    }
}
