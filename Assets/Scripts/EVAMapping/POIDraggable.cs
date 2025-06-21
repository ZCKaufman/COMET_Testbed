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
                // STEP 1: Spawn the POI
                GameObject placed = Photon.Pun.PhotonNetwork.Instantiate(
                    "Prefabs/POI_Marker",
                    Vector3.zero,
                    Quaternion.identity
                );

                PhotonView deleteView = placed.GetComponent<PhotonView>();
                deleteView.RPC("RPC_Init", RpcTarget.AllBuffered, GetComponent<PhotonView>().ViewID);


                // STEP 2: Parent it immediately to the correct UI panel (before positioning)
                RectTransform placedRect = placed.GetComponent<RectTransform>();
                placedRect.SetParent(GameObject.Find("EVAMapPanel")?.transform, worldPositionStays: false);
                //Debug.Log("POI anchored at: " + placedRect.anchoredPosition + ", parent: " + placedRect.parent.name);


                // STEP 3: Set correct UI-local position
                Vector2 dropPos;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    GameObject.Find("EVAMapPanel")?.transform as RectTransform,
                    Input.mousePosition,
                    canvas.worldCamera,
                    out dropPos
                );
                placedRect.anchoredPosition = dropPos;
                
                // STEP 4: Clean up ghost clone
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
        foreach (GameObject poi in placedPOIs)
        {
            if (poi != null)
                Destroy(poi);
        }

        placedPOIs.Clear();
    }

}
