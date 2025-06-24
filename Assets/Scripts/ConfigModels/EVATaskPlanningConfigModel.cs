using System;
using System.Collections.Generic;

/// Array wrapper so JsonUtility can read EVATaskPlanning.IVA
[Serializable] public class IVATaskArray
{
    public List<IVATaskEntry> IVA;
}

/// One IVA-sample entry
[Serializable] public class IVATaskEntry
{
    public string        Name;
    public List<string>  EVA;   // single consolidated list
}

/// Runtime lookup dictionary
public class IVASampleTaskRoot
{
    public Dictionary<string, IVATaskEntry> Samples = new();
}
