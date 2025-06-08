using System.Collections.Generic;
using UnityEngine;


public class ButtonGroupManager : MonoBehaviour
{
    public List<SelectableButton> buttons = new List<SelectableButton>();
    private SelectableButton currentlySelected;

    private void Start()
    {
        if (buttons.Count == 0)
        {
            buttons.AddRange(GetComponentsInChildren<SelectableButton>());
            foreach (var btn in buttons)
            {
                btn.groupManager = this;
            }
        }
    }

    public void SelectButton(SelectableButton selected)
    {
        if (currentlySelected == selected)
        {
            currentlySelected.SetSelected(false);
            currentlySelected = null;
            return;
        }

        if (currentlySelected != null)
        {
            currentlySelected.SetSelected(false);
        }

        currentlySelected = selected;
        currentlySelected.SetSelected(true);
    }

    public void DeselectCurrent()
    {
        if (currentlySelected != null)
        {
            currentlySelected.SetSelected(false);
            currentlySelected = null;
        }
    }

}
