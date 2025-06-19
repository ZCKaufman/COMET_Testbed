using UnityEngine;
using UnityEngine.EventSystems;

public class DeleteOnPOIClick : MonoBehaviour, IPointerClickHandler
{
    public RouteDrawingManager routeManager;
    public POIDraggable poiDraggable;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (routeManager != null && routeManager.IsInDeleteMode())
        {
            poiDraggable.UnregisterPOI(gameObject);
            Destroy(gameObject);
        }
    }
}
