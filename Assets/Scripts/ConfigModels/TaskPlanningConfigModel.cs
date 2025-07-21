using System;
using System.Collections.Generic;

[Serializable]
public class TaskEntry
{
    public string Description;
    public int Personnel;
    public bool Required;
    public int Duration; 
    public string roiEquation;
}

[Serializable]
public class TaskPOIEntry
{
    public string Name;
    public List<TaskEntry> Tasks;
}

[Serializable]
public class TaskPlanningSection
{
    public List<TaskPOIEntry> POIs;
}
