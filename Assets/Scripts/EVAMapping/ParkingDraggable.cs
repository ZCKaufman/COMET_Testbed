using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class ParkingDraggable : MonoBehaviour, IPointerClickHandler
{
    public RectTransform dragTargetPrefab; 
    public Canvas canvas;                
    public RectTransform mapImageRect;  

    private RectTransform clone;
    private bool dragging = false;

    private List<GameObject> placedParkings = new List<GameObject>();

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

            // scroll wheel
            float scroll = Input.mouseScrollDelta.y;
            if (Mathf.Abs(scroll) > 0.01f)
            {
                clone.Rotate(0f, 0f, scroll * 10f);
            }

            // with R/E key
            if (Input.GetKey(KeyCode.R))
            {
                clone.Rotate(0f, 0f, 1.5f);
            }
            else if (Input.GetKey(KeyCode.E))
            {
                clone.Rotate(0f, 0f, -1.5f);
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
                GameObject placed = clone.gameObject;

                DeleteOnParkingClick deleteScript = placed.AddComponent<DeleteOnParkingClick>();
                deleteScript.routeManager = Object.FindFirstObjectByType<RouteDrawingManager>();
                deleteScript.parkingDraggable = this;

                placedParkings.Add(placed);

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
        foreach (GameObject parking in placedParkings)
        {
            if (parking != null)
                Destroy(parking);
        }

        placedParkings.Clear();
    }
}

