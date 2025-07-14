using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using Photon.Pun;

public class ParkingDraggable : MonoBehaviour, IPointerClickHandler
{
    public RectTransform dragTargetPrefab;
    public Canvas canvas;
    public RectTransform mapImageRect;

    private RectTransform clone;
    private bool dragging = false;

    private List<GameObject> placedParkings = new List<GameObject>();
    private PhotonView photonView;

 void Awake()
{
    photonView = GetComponent<PhotonView>();
    if (photonView == null)
    {
        Debug.LogError("Missing PhotonView on object with ParkingDraggable");
    }
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

            // scroll wheel
            float scroll = Input.mouseScrollDelta.y;
            if (Mathf.Abs(scroll) > 0.01f)
            {
                clone.Rotate(0f, 0f, scroll * 10f);
            }

            if (Input.GetKey(KeyCode.R))
            {
                clone.Rotate(0f, 0f, 0.5f);
            }
            else if (Input.GetKey(KeyCode.E))
            {
                clone.Rotate(0f, 0f, -0.5f);
            }
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
                GameObject placed = PhotonNetwork.Instantiate(
                    "Prefabs/ParkSpot",
                    Vector3.zero,
                    Quaternion.identity
                );

                RectTransform placedRect = placed.GetComponent<RectTransform>();

                RectTransform parentPanel = GameObject.Find("MappingPanel")?.transform as RectTransform;
                placedRect.SetParent(parentPanel, worldPositionStays: false);

                Vector2 dropPos;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    parentPanel,
                    Input.mousePosition,
                    canvas.worldCamera,
                    out dropPos
                );
                placedRect.anchoredPosition = dropPos;
                placedRect.rotation = clone.rotation;

                Destroy(clone.gameObject);
                clone = null;
                dragging = false;
                placedParkings.Add(placed);
            }
            else
            {
                Destroy(clone.gameObject);
                clone = null;
                dragging = false;
            }
        }
    }

    public void UnregisterParking(GameObject parking)
    {
        if (placedParkings.Contains(parking))
            placedParkings.Remove(parking);
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

    public void ClearAllParkings()
    {
        photonView.RPC("RPC_ClearAllParkings", RpcTarget.AllBuffered);
    }
    
    [PunRPC]
    void RPC_ClearAllParkings()
    {
        GameObject[] allParkings = GameObject.FindGameObjectsWithTag("Parking");
        foreach (GameObject parking in allParkings)
        {
            PhotonView view = parking.GetComponent<PhotonView>();
            if (view != null && view.IsMine)
            {
                PhotonNetwork.Destroy(parking);
            }
            else if (view == null)
            {
                Destroy(parking);  // fallback for non-networked POIs
            }
        }

        placedParkings.Clear(); 
    }
}

