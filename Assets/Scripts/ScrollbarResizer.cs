using UnityEngine;
using UnityEngine.UI;

public class ScrollbarResizer : MonoBehaviour
{
    public RectTransform targetToResize; // The Scroll View or Scrollbar background
    public Button resizeToggleButton;

    private float originalHeight;
    private bool isExpanded = false;

    void Start()
    {
        if (targetToResize == null)
        {
            Debug.LogError("Target to resize not assigned.");
            return;
        }

        // Store the original height
        originalHeight = targetToResize.sizeDelta.y;

        // Hook up the button
        resizeToggleButton.onClick.AddListener(ToggleSize);
    }

    void ToggleSize()
    {
        if (!isExpanded)
        {
            targetToResize.sizeDelta = new Vector2(
                targetToResize.sizeDelta.x,
                originalHeight * 10f
            );
        }
        else
        {
            targetToResize.sizeDelta = new Vector2(
                targetToResize.sizeDelta.x,
                originalHeight
            );
        }

        isExpanded = !isExpanded;
    }
}
