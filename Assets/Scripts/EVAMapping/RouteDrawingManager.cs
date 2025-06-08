using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;


public class RouteDrawingManager : MonoBehaviour
{
    public RectTransform poiContainer;
    public GameObject uiLinePrefab;


    private string currentDrawType = null;
    private List<Vector2> drawingPoints = new List<Vector2>();
    private List<GameObject> userDrawnLines = new List<GameObject>();
    private bool isDeleteMode = false;
    public bool IsInDeleteMode() => isDeleteMode;
    public void DeleteLine(GameObject line)
    {
        userDrawnLines.Remove(line);
        Destroy(line);
    }


    public void ToggleDrawingMode(string type)
    {
        // If same button clicked again, turn off
        if (currentDrawType == type)
        {
            //Debug.Log("Exiting drawing mode: " + type);
            currentDrawType = null;
            drawingPoints.Clear();
        }
        else
        {
            //Debug.Log("Switched to drawing mode: " + type);
            currentDrawType = type;
            drawingPoints.Clear();
        }
    }


    public void ToggleDeleteMode(bool forceValue)
    {
        isDeleteMode = forceValue;
        //Debug.Log("Delete Mode: " + isDeleteMode);
    }




    public void HandleClick(Vector2 localPos)
    {
        if (string.IsNullOrEmpty(currentDrawType)) return;


        drawingPoints.Add(localPos);


        if (drawingPoints.Count >= 2)
        {
            Vector2 start = drawingPoints[drawingPoints.Count - 2];
            Vector2 end = drawingPoints[drawingPoints.Count - 1];


            Color color = currentDrawType == "walk"
                ? new Color(0.2f, 0.5f, 0.2f)
                : new Color(0.6f, 0.1f, 0.1f);


            bool dashed = false;
            DrawLine(start, end, color, dashed);
        }
    }




    private void DrawLine(Vector2 start, Vector2 end, Color color, bool dashed)
    {
        if (!dashed)
        {
            CreateSegment(start, end, color);
            return;
        }


        float len = Vector2.Distance(start, end);
        Vector2 dir = (end - start).normalized;
        float dashLen = 10f, gapLen = 5f;
        float curr = 0;


        while (curr < len)
        {
            float segEnd = Mathf.Min(curr + dashLen, len);
            Vector2 p1 = start + dir * curr;
            Vector2 p2 = start + dir * segEnd;
            CreateSegment(p1, p2, color);
            curr += dashLen + gapLen;
        }
    }


    private void CreateSegment(Vector2 start, Vector2 end, Color color)
    {
        GameObject go = Instantiate(uiLinePrefab, poiContainer);
        go.name = "UserDrawnLine";
        userDrawnLines.Add(go);


        Image img = go.GetComponent<Image>();
        img.color = color;


        RectTransform rt = go.GetComponent<RectTransform>();
        Vector2 dir = (end - start).normalized;
        float dist = Vector2.Distance(start, end);


        rt.sizeDelta = new Vector2(dist, 4f);
        rt.anchorMin = rt.anchorMax = new Vector2(0, 0);
        rt.pivot = new Vector2(0, 0.5f);
        rt.anchoredPosition = start;
        rt.localEulerAngles = new Vector3(0, 0, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);


        // Add collider and handler
        BoxCollider2D col = go.AddComponent<BoxCollider2D>();
        col.size = rt.sizeDelta;


        DeleteOnClick del = go.AddComponent<DeleteOnClick>();
        del.manager = this;
    }




    public void ClearUserLines()
    {
        foreach (var line in userDrawnLines)
        {
            if (line != null)
                Destroy(line);
        }
        userDrawnLines.Clear();
    }


    public void FinishDrawing()
    {
        currentDrawType = null;
        drawingPoints.Clear();
    }


    public void DeleteAllUserLines()
    {
        ClearUserLines();
    }




}




