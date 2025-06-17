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
            Vector2 localPoint;
            bool insideMap = RectTransformUtility.ScreenPointToLocalPointInRectangle(
                mapImageRect,
                Input.mousePosition,
                canvas.worldCamera,
                out localPoint
            );

            if (insideMap && mapImageRect.rect.Contains(localPoint))
            {
                GameObject placed = clone.gameObject;

                DeleteOnPOIClick deleteScript = placed.AddComponent<DeleteOnPOIClick>();
                deleteScript.routeManager = Object.FindFirstObjectByType<RouteDrawingManager>();
                deleteScript.poiDraggable = this;

                placedPOIs.Add(placed);

                dragging = false;
                clone = null;
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

