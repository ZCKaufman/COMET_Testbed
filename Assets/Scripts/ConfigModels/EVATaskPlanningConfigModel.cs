using System;
using System.Collections.Generic;

/// ---------- models used ONLY for the IVA Sample-Task feature ----------

[Serializable]
public class IVATaskEntry         // 1 entry in the IVA array
{
    public string        Name;
    public List<string>  EV1;
    public List<string>  EV2;
}

[Serializable]
public class IVATaskArray        // wrapper so JsonUtility can parse an array
{
    public List<IVATaskEntry> IVA;
}

/* Runtime lookup dictionary */
public class IVASampleTaskRoot
{
    public Dictionary<string, IVATaskEntry> Samples = new();
}
