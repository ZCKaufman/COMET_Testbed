using UnityEngine;
using UnityEngine.EventSystems;


public class ClickCatcher : MonoBehaviour, IPointerClickHandler
{
    public RectTransform poiContainer;
    public RouteDrawingManager drawingManager;


    public void OnPointerClick(PointerEventData eventData)
    {
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            poiContainer,
            eventData.position,
            eventData.pressEventCamera,
            out localPoint
        );


        Vector2 anchored = localPoint + (poiContainer.rect.size * 0.5f);
        drawingManager.HandleClick(anchored);
    }
}
