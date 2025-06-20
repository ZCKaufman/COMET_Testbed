using UnityEngine;
using UnityEngine.EventSystems;

public class DeleteOnParkingClick : MonoBehaviour, IPointerClickHandler
{
    public RouteDrawingManager routeManager;
    public ParkingDraggable parkingDraggable;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (routeManager != null && routeManager.IsInDeleteMode())
        {
            parkingDraggable.UnregisterParking(gameObject);
            Destroy(gameObject);
        }
    }
}