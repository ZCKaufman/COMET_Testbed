using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;


public class POIDisplay : MonoBehaviour, IPointerClickHandler
{
    public TMP_Text descriptionText;
    private bool isVisible = false;


    public void SetDescription(string description)
    {
        if (descriptionText != null)
        {
            descriptionText.text = description;
            descriptionText.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning("DescriptionText is not assigned in the Inspector.");
        }
    }


    public void OnPointerClick(PointerEventData eventData)
    {
        if (descriptionText != null)
        {
            isVisible = !isVisible;
            descriptionText.gameObject.SetActive(isVisible);
        }
    }
}

