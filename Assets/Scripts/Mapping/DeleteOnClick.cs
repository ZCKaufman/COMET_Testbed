using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class DeleteOnClick : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public RouteDrawingManager manager;


    private Image image;
    private Color originalColor;
    private Color highlightColor = new Color(1f, 0.4f, 0.4f);


    private void Awake()
    {
        image = GetComponent<Image>();
        originalColor = image.color;
    }


    public void OnPointerClick(PointerEventData eventData)
    {
        if (manager != null && manager.IsInDeleteMode())
        {
            manager.DeleteLine(gameObject);
        }
    }


    public void OnPointerEnter(PointerEventData eventData)
    {
        if (manager != null && manager.IsInDeleteMode())
        {
            image.color = highlightColor;
        }
    }


    public void OnPointerExit(PointerEventData eventData)
    {
        if (manager != null && manager.IsInDeleteMode())
        {
            image.color = originalColor;
        }
    }
}
