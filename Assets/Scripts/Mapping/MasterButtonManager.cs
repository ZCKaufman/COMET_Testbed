using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class MasterButtonManager : MonoBehaviour
{
    public List<ButtonGroupManager> groupManagers = new List<ButtonGroupManager>();
    public Button clearAllButton;

    private void Start()
    {
        if (clearAllButton != null)
        {
            clearAllButton.onClick.AddListener(DeselectAllGroups);
        }
    }

    public void DeselectAllGroups()
    {
        foreach (var group in groupManagers)
        {
            group.DeselectCurrent();
        }
    }
}
