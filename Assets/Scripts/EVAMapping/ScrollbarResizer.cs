using UnityEngine;
using TMPro;

public class ScrollBarResizer : MonoBehaviour
{
    public RectTransform targetContainer;
    public TextMeshProUGUI buttonLabel;
    private bool isExpanded = false;
    private Vector2 originalSize;

    void Start()
    {
        if (targetContainer != null)
            originalSize = targetContainer.sizeDelta;

        UpdateButtonLabel();
    }

    public void ToggleHeight()
    {
        if (targetContainer == null) return;

        float newHeight = isExpanded ? originalSize.y : originalSize.y * 5f;
        targetContainer.sizeDelta = new Vector2(originalSize.x, newHeight);

        isExpanded = !isExpanded;

        UpdateButtonLabel();
    }

    private void UpdateButtonLabel()
    {
        if (buttonLabel != null)
            buttonLabel.text = isExpanded ? "Collapse" : "Expand";
    }
}
