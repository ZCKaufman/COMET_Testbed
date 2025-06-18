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
                ? new Color(0.2f, 0.4f, 0.8f)  // muted blue
                : new Color(0.9f, 0.7f, 0.2f); // warm gold/yellow


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

        GameObject scrollEntry = AddLineToScrollView(currentDrawType, start, end, dist);

        LineInfo info = new LineInfo
        {
            lineObject = go,
            scrollEntry = scrollEntry,
            routeType = currentDrawType,
            start = start,
            end = end,
            distance = dist
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
        RefreshScrollEntries();
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

    private GameObject AddLineToScrollView(string routeType, Vector2 start, Vector2 end, float distance)
    {
        var config = ConfigLoader.EVAMapConfig;
        GameObject entry = Instantiate(lineInfoTextPrefab, scrollViewContent);

        float scale = config.EVAMapping.mapScale;
        float scaledDistance = distance * scale;
        Vector2 scaledEnd = end * scale;

        TMP_Text numberText = entry.transform.Find("NumberColumn/NumberText")?.GetComponent<TMP_Text>();
        TMP_Text coordinatesText = entry.transform.Find("CoordinatesColumn/CoordinatesText")?.GetComponent<TMP_Text>();
        TMP_Text distanceText = entry.transform.Find("DistanceColumn/DistanceText")?.GetComponent<TMP_Text>();
        TMP_Text totalDistanceText = entry.transform.Find("TotalDistanceColumn/TotalDistanceText")?.GetComponent<TMP_Text>();

        if (numberText != null)
            numberText.text = $"{userDrawnLineInfos.Count + 1}";

        if (coordinatesText != null)
            coordinatesText.text = $"({scaledEnd.x:0.0}, {scaledEnd.y:0.0})";

        if (distanceText != null)
            distanceText.text = $"{scaledDistance:0.0} m";

        if (totalDistanceText != null)
        {
            float total = GetTotalDistance(routeType) + distance;
            totalDistanceText.text = $"{total * scale:0.0} m";
        }
        return entry;
    }

    private float GetTotalDistance(string routeType)
    {
        float total = 0f;
        foreach (var info in userDrawnLineInfos)
            if (info.routeType == routeType)
                total += info.distance;
        return total;
    }
    
    private void RefreshScrollEntries()
    {
        foreach (var info in userDrawnLineInfos)
        {
            if (info.scrollEntry != null)
                Destroy(info.scrollEntry);
        }

        float scale = ConfigLoader.EVAMapConfig.EVAMapping.mapScale;
        Dictionary<string, float> runningTotals = new();

        for (int i = 0; i < userDrawnLineInfos.Count; i++)
        {
            var info = userDrawnLineInfos[i];

            if (!runningTotals.ContainsKey(info.routeType))
                runningTotals[info.routeType] = 0f;

            runningTotals[info.routeType] += info.distance;
            float total = runningTotals[info.routeType];

            GameObject newEntry = Instantiate(lineInfoTextPrefab, scrollViewContent);

            TMP_Text numberText = newEntry.transform.Find("NumberColumn/NumberText")?.GetComponent<TMP_Text>();
            TMP_Text coordinatesText = newEntry.transform.Find("CoordinatesColumn/CoordinatesText")?.GetComponent<TMP_Text>();
            TMP_Text distanceText = newEntry.transform.Find("DistanceColumn/DistanceText")?.GetComponent<TMP_Text>();
            TMP_Text totalDistanceText = newEntry.transform.Find("TotalDistanceColumn/TotalDistanceText")?.GetComponent<TMP_Text>();

            if (numberText != null)
                numberText.text = $"{i + 1}";

            if (coordinatesText != null)
            {
                Vector2 scaledEnd = info.end * scale;
                coordinatesText.text = $"({scaledEnd.x:0.0}, {scaledEnd.y:0.0})";
            }

            if (distanceText != null)
                distanceText.text = $"{info.distance * scale:0.0} m";

            if (totalDistanceText != null)
                totalDistanceText.text = $"{total * scale:0.0} m";

            userDrawnLineInfos[i] = new LineInfo
            {
                lineObject = info.lineObject,
                scrollEntry = newEntry,
                routeType = info.routeType,
                start = info.start,
                end = info.end,
                distance = info.distance
            };
        }
    }

}
public struct LineInfo
{
    public GameObject lineObject;
    public GameObject scrollEntry;
    public string routeType;
    public Vector2 start;
    public Vector2 end;
    public float distance;
}


