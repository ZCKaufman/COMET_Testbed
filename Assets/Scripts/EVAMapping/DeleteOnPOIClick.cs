using UnityEngine;
using UnityEngine.EventSystems;
using Photon.Pun;

public class DeleteOnPOIClick : MonoBehaviour, IPointerClickHandler
{
    private RouteDrawingManager routeManager;
    private POIDraggable poiDraggable;

    void Awake()
    {
        routeManager = Object.FindFirstObjectByType<RouteDrawingManager>();
        poiDraggable = Object.FindFirstObjectByType<POIDraggable>();

    }
    public void OnPointerClick(PointerEventData eventData)
    {
        if (routeManager != null && routeManager.IsInDeleteMode())
        {
            if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
            {
                PhotonView view = GetComponent<PhotonView>();
                Debug.Log($"POI Clicked: {gameObject.name}, ViewID: {view?.ViewID}");

                if (view != null)
                {
                    Debug.Log($"Attempting to delete POI: {gameObject.name}");

                    view.RPC("RPC_DestroySelf", RpcTarget.AllBuffered);
                }
            }
            else
            {
                // fallback for offline/local
                poiDraggable?.UnregisterPOI(gameObject);
                Destroy(gameObject);
            }
        }
    }

    [PunRPC]
    void RPC_DestroySelf()
    {
        poiDraggable?.UnregisterPOI(gameObject);
        Destroy(gameObject);
    }
}
