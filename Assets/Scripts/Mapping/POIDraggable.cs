using UnityEngine;
using UnityEngine.EventSystems;
using Photon.Pun;
using System.Collections.Generic;

public class POIDraggable : MonoBehaviour, IPointerClickHandler
{
    public RectTransform dragTargetPrefab;
    public Canvas canvas;
    public RectTransform mapImageRect;

    private RectTransform clone;
    private bool dragging = false;

    private List<GameObject> placedPOIs = new List<GameObject>();

    private PhotonView photonView;

    void Awake()
    {
        photonView = GetComponent<PhotonView>();
    }

    void Update()
    {
        if (dragging && clone != null)
        {
            Vector2 mousePos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                clone.parent as RectTransform,
                Input.mousePosition,
                canvas.worldCamera,
                out mousePos
            );
            clone.anchoredPosition = mousePos;

        }

        if (dragging && Input.GetMouseButtonDown(0))
        {
            Vector2 localPoint;
            bool insideMap = RectTransformUtility.ScreenPointToLocalPointInRectangle(
                mapImageRect,
                Input.mousePosition,
                canvas.worldCamera,
                out localPoint
            );

            if (insideMap && mapImageRect.rect.Contains(localPoint))
            {
                GameObject placed = Photon.Pun.PhotonNetwork.Instantiate(
                    "Prefabs/POI_Marker",
                    Vector3.zero,
                    Quaternion.identity
                );

                RectTransform placedRect = placed.GetComponent<RectTransform>();

                // Parent under MappingPanel before positioning
                RectTransform parentPanel = GameObject.Find("MappingPanel")?.transform as RectTransform;
                
                placedRect.SetParent(parentPanel, worldPositionStays: false);

                // Get mouse pos relative to MappingPanel
                Vector2 dropPos;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    parentPanel,
                    Input.mousePosition,
                    canvas.worldCamera,
                    out dropPos
                );
                placedRect.anchoredPosition = dropPos;

                // Clean up
                Destroy(clone.gameObject);
                clone = null;
                dragging = false;

                placedPOIs.Add(placed);
            }


            else
            {
                Destroy(clone.gameObject);
                clone = null;
                dragging = false;
            }
        }
    }
    public void UnregisterPOI(GameObject poi)
    {
        if (placedPOIs.Contains(poi))
            placedPOIs.Remove(poi);
    }


    public void OnPointerClick(PointerEventData eventData)
    {
        if (dragging) return;

        if (dragTargetPrefab == null || canvas == null) return;

        clone = Instantiate(dragTargetPrefab, canvas.transform);
        clone.transform.SetAsLastSibling();
        clone.sizeDelta = dragTargetPrefab.sizeDelta;
        clone.pivot = dragTargetPrefab.pivot;

        dragging = true;
    }

    public void ClearAllPOIs()
    {
        photonView.RPC("RPC_ClearAllPOIs", RpcTarget.AllBuffered);
    }

    [PunRPC]
    void RPC_ClearAllPOIs()
    {
        GameObject[] allPOIs = GameObject.FindGameObjectsWithTag("POI");
        foreach (GameObject poi in allPOIs)
        {
            PhotonView view = poi.GetComponent<PhotonView>();
            if (view != null && view.IsMine)
            {
                PhotonNetwork.Destroy(poi);
            }
            else if (view == null)
            {
                Destroy(poi);  // fallback for non-networked POIs
            }
        }

        placedPOIs.Clear(); 
    }
}
