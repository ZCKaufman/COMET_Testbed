using System;
using System.Collections.Generic;

/// <summary>
/// Mission view configuration model.
/// </summary>
[Serializable]
public class MissionInfoAll
{
    public string MissionDescription;
    public List<string> Alerts;
}

[Serializable]
public class MissionInfoSection
{
    public MissionInfoAll All;
}

[Serializable]
public class MissionViewConfigRoot
{
    public MissionInfoSection MissionInfo;
    public ObjectiveVerificationSection ObjectiveVerification;
}

