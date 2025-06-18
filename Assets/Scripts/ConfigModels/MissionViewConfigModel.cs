using System;
using System.Collections.Generic;

/// <summary>
/// Mission view configuration model.
/// </summary>
[Serializable]
public class MissionInfoAll
{
    public string MissionInfo;
    public string Alerts;
}

[Serializable]
public class MissionInfoEVA
{
    public string TaskInfo;
}

[Serializable]
public class MissionInfoIVA
{
    public string TaskInfo;
}

[Serializable]
public class MissionInfoSection
{
    public MissionInfoAll All;
    public MissionInfoEVA EVA;
    public MissionInfoIVA IVA;
}

[Serializable]
public class MissionViewConfigRoot
{
    public MissionInfoSection MissionInfo;

}

