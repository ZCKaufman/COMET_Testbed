using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;


public class RouteDrawingManager : MonoBehaviour
{
    public RectTransform poiContainer;
    public GameObject uiLinePrefab;

    private string currentDrawType = null;
    private List<Vector2> drawingPoints = new List<Vector2>();
    private bool isDeleteMode = false;
    private List<LineInfo> userDrawnLineInfos = new List<LineInfo>();
    public Transform scrollViewContent;
    public GameObject lineInfoTextPrefab;


    public bool IsInDeleteMode() => isDeleteMode;

    public void ToggleDrawingMode(string type)
    {
        if (currentDrawType == type)
        {
            currentDrawType = null;
            drawingPoints.Clear();
        }
        else
        {
            currentDrawType = type;
            drawingPoints.Clear();
        }
    }

    public void ToggleDeleteMode(bool forceValue)
    {
        isDeleteMode = forceValue;
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

        BoxCollider2D col = go.AddComponent<BoxCollider2D>();
        col.size = rt.sizeDelta;

        DeleteOnClick del = go.AddComponent<DeleteOnClick>();
        del.manager = this;

        GameObject scrollEntry = AddLineToScrollView(currentDrawType, start, end);

        LineInfo info = new LineInfo
        {
            lineObject = go,
            scrollEntry = scrollEntry,
            routeType = currentDrawType,
            start = start,
            end = end
        };

        userDrawnLineInfos.Add(info);
    }

    public void DeleteLine(GameObject line)
    {
        LineInfo? match = userDrawnLineInfos.Find(info => info.lineObject == line);

        if (match.HasValue)
        {
            LineInfo info = match.Value;

            if (info.scrollEntry != null)
                Destroy(info.scrollEntry);

            userDrawnLineInfos.Remove(info);
        }

        Destroy(line);
    }


    public void DeleteAllUserLines()
    {
        foreach (var info in userDrawnLineInfos)
        {
            if (info.lineObject != null)
                Destroy(info.lineObject);

            if (info.scrollEntry != null)
                Destroy(info.scrollEntry);
        }

        userDrawnLineInfos.Clear();
    }


    public void ClearUserLines() => DeleteAllUserLines();

    public void FinishDrawing()
    {
        currentDrawType = null;
        drawingPoints.Clear();
    }
    public List<LineInfo> GetLinesByType(string type)
    {
        return userDrawnLineInfos.FindAll(info => info.routeType == type);
    }

    private GameObject AddLineToScrollView(string routeType, Vector2 start, Vector2 end)
    {
        GameObject entry = Instantiate(lineInfoTextPrefab, scrollViewContent);
        TextMeshProUGUI text = entry.GetComponent<TextMeshProUGUI>();

        if (text != null)
        {
            text.text = $"[{routeType.ToUpper()}] From ({start.x:0.00}, {start.y:0.00}) " +
                        $"to ({end.x:0.00}, {end.y:0.00})";
            //text.color = Color.white;
        }

        return entry;
    }



}
public struct LineInfo
{
    public GameObject lineObject;
    public GameObject scrollEntry;
    public string routeType;
    public Vector2 start;
    public Vector2 end;
}

