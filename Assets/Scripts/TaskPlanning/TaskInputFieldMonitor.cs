using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class TaskInputFieldMonitor : MonoBehaviour
{
    public TMP_InputField inputField;
    private EVATaskPanelController parentController;
    private Transform container;
    //private bool wasEmptyLastFrame = false;

    public void Initialize(EVATaskPanelController controller, Transform parentContainer)
    {
        parentController = controller;
        container      = parentContainer;
    }

    void Update()
    {

        if (inputField == null) return;

        if (inputField.isFocused && Input.GetKeyDown(KeyCode.Tab))
        {
            // Prevent default tab behavior
            EventSystem.current.SetSelectedGameObject(null);

            // Ask the parent controller to move to the next task
            parentController.FocusNextField(container, inputField);
        }
    }

}
