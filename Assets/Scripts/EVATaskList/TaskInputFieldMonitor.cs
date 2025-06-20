using TMPro;
using UnityEngine;

public class TaskInputFieldMonitor : MonoBehaviour
{
    public TMP_InputField inputField;
    private EVATaskPanelController taskController;
    private Transform container;
    private bool wasEmptyLastFrame = false;

    public void Initialize(EVATaskPanelController controller, Transform parentContainer)
    {
        taskController = controller;
        container      = parentContainer;
    }

    void Update()
    {
        if (inputField == null || !inputField.isFocused) return;

        bool deleteKey = Input.GetKeyDown(KeyCode.Backspace) || Input.GetKeyDown(KeyCode.Delete);

        if (deleteKey && wasEmptyLastFrame && string.IsNullOrWhiteSpace(inputField.text))
            taskController.TryDeleteTask(inputField, container);

        wasEmptyLastFrame = string.IsNullOrWhiteSpace(inputField.text);
    }
}
