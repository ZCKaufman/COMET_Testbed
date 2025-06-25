using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class POIDisplay : MonoBehaviour, IPointerClickHandler
{
    public GameObject descriptionContainer;
    public TMP_Text descriptionText;
    private bool isVisible = false;

    public void SetDescription(string rawDescription)
    {
        if (descriptionText == null)
        {
            Debug.LogWarning("DescriptionText is not assigned.");
            return;
        }

        string[] parts = rawDescription.Split('|');
        string title = parts.Length > 0 ? parts[0] : "";
        string body = parts.Length > 1 ? parts[1] : "";

        descriptionText.text = $"<b><size=120%>{title}</size></b>\n{body}";

        var scrollRect = descriptionText.GetComponentInParent<UnityEngine.UI.ScrollRect>();
        if (scrollRect != null)
            scrollRect.verticalNormalizedPosition = 1f;

        if (descriptionContainer != null)
        {
            descriptionContainer.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

            if (title == "Lava Tube")
            {
                var rectTransform = descriptionContainer.GetComponent<RectTransform>();
                if (rectTransform != null)
                    rectTransform.anchoredPosition += new Vector2(160f, 0f); 
            }

            descriptionContainer.SetActive(false);
        }
    }



    public void OnPointerClick(PointerEventData eventData)
    {
        isVisible = !isVisible;
        if (descriptionContainer != null)
            descriptionContainer.SetActive(isVisible);
    }
}


