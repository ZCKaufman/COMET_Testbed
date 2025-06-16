[System.Serializable]
public class MissionData
{
    public MissionInfoData MissionInfo;
}

[System.Serializable]
public class MissionInfoData
{
    public AllTeamData All;
    public TeamTaskData EVA;
    public TeamTaskData IVA;
}

[System.Serializable]
public class AllTeamData
{
    public string MissionInfo;
    public string Alerts;
}

[System.Serializable]
public class TeamTaskData
{
    public string TaskInfo;
}
