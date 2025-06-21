using UnityEngine;
using UnityEngine.EventSystems;
using Photon.Pun;

public class DeleteOnPOIClick : MonoBehaviour, IPointerClickHandler
{
    public RouteDrawingManager routeManager;
    public POIDraggable poiDraggable;

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
                    if (!view.IsMine)
                    {
                        view.TransferOwnership(PhotonNetwork.LocalPlayer);
                        Debug.Log($"Transferring ownership of {gameObject.name} to local player.");

                    }
                    view.TransferOwnership(PhotonNetwork.LocalPlayer);

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

    [PunRPC]
    public void RPC_Init(int placerViewID)
    {
        routeManager = Object.FindFirstObjectByType<RouteDrawingManager>();
        PhotonView placerView = PhotonView.Find(placerViewID);
        if (placerView != null)
        {
            poiDraggable = placerView.GetComponent<POIDraggable>();
        }
    }
}
