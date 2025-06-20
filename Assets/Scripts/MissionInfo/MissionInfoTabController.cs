using TMPro;
using UnityEngine;
using System.Collections;

public class MissionInfoTabController : MonoBehaviour
{
    [SerializeField] private TMP_Text missionDescriptionText;
    [SerializeField] private TMP_Text ev1TasksText;
    [SerializeField] private TMP_Text ev2TasksText;

    private bool hasSubmittedEV1 = false;
    private bool hasSubmittedEV2 = false;

    void Start()
    {
        StartCoroutine(WaitForMissionLoad());
    }

    IEnumerator WaitForMissionLoad()
    {
        while (!ConfigLoader.IsLoaded)
            yield return null;

        if (ConfigLoader.MissionConfig != null && ConfigLoader.MissionConfig.MissionInfo != null)
        {
            string missionDesc = ConfigLoader.MissionConfig.MissionInfo.All.MissionInfo;
            missionDescriptionText.text = missionDesc;
        }
    }

    public void SetTaskLists(string ev1Text, string ev2Text)
    {
        if (!hasSubmittedEV1)
        {
            ev1TasksText.text = ev1Text;
            hasSubmittedEV1 = true;
        }
        else
        {
            ev1TasksText.text += "\n" + ev1Text;
        }

        if (!hasSubmittedEV2)
        {
            ev2TasksText.text = ev2Text;
            hasSubmittedEV2 = true;
        }
        else
        {
            ev2TasksText.text += "\n" + ev2Text;
        }
    }
}
