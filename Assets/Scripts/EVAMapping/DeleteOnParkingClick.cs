using UnityEngine;
using UnityEngine.EventSystems;
using Photon.Pun;

public class DeleteOnParkingClick : MonoBehaviour, IPointerClickHandler
{
    private RouteDrawingManager routeManager;
    private ParkingDraggable parkingDraggable;

    void Awake()
    {
        routeManager = Object.FindFirstObjectByType<RouteDrawingManager>();
        parkingDraggable = Object.FindFirstObjectByType<ParkingDraggable>();

    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (routeManager != null && routeManager.IsInDeleteMode())
        {
            if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
            {
                PhotonView view = GetComponent<PhotonView>();
                Debug.Log($"Parking spot Clicked: {gameObject.name}, ViewID: {view?.ViewID}");

                if (view != null)
                {
                    Debug.Log($"Attempting to delete parking spot: {gameObject.name}");

                    view.RPC("RPC_DestroySelf", RpcTarget.AllBuffered);
                }
            }
            else
            {
                // fallback for offline/local
                parkingDraggable?.UnregisterParking(gameObject);
                Destroy(gameObject);
            }
        }
    }

    [PunRPC]
    void RPC_DestroySelf()
    {
        parkingDraggable?.UnregisterParking(gameObject);
        Destroy(gameObject);
    }
}