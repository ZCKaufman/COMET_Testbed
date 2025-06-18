using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class POIDisplay : MonoBehaviour, IPointerClickHandler
{
    public GameObject descriptionContainer; // assign "Description" GameObject
    public TMP_Text descriptionText;
    private bool isVisible = false;

    public void SetDescription(string description)
    {
        if (descriptionText != null)
        {
            descriptionText.text = description;

            // Scroll back to top when setting new text
            var scrollRect = descriptionText.GetComponentInParent<UnityEngine.UI.ScrollRect>();
            if (scrollRect != null)
                scrollRect.verticalNormalizedPosition = 1f;

            if (descriptionContainer != null)
                descriptionContainer.SetActive(false);
        }
        else
        {
            Debug.LogWarning("DescriptionText is not assigned.");
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        isVisible = !isVisible;
        if (descriptionContainer != null)
            descriptionContainer.SetActive(isVisible);
    }
}


