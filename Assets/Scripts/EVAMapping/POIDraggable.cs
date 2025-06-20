using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
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
            // Follow mouse with the ghost clone
            Vector2 mousePos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                Input.mousePosition,
                canvas.worldCamera,
                out mousePos
            );
            clone.anchoredPosition = mousePos;
        }

        if (dragging && Input.GetMouseButtonDown(0))
        {
            // Check if the mouse is over the map
            Vector2 localPoint;
            bool insideMap = RectTransformUtility.ScreenPointToLocalPointInRectangle(
                mapImageRect,
                Input.mousePosition,
                canvas.worldCamera,
                out localPoint
            );

            if (insideMap && mapImageRect.rect.Contains(localPoint))
            {
                // Convert local UI point to world position
                Vector3 worldPos = canvas.transform.TransformPoint(clone.anchoredPosition);

                // Destroy ghost clone
                Destroy(clone.gameObject);
                clone = null;
                dragging = false;

                GameObject networkedPOI = Photon.Pun.PhotonNetwork.Instantiate(
                    "Prefabs/POI_Marker",
                    Vector3.zero,
                    Quaternion.identity
                );

                networkedPOI.transform.SetParent(canvas.transform, worldPositionStays: false);

                RectTransform rt = networkedPOI.GetComponent<RectTransform>();
                Vector2 anchoredPos;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvas.transform as RectTransform,
                    Input.mousePosition,
                    canvas.worldCamera,
                    out anchoredPos
                );
                rt.anchoredPosition = anchoredPos;

                placedPOIs.Add(networkedPOI);
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

