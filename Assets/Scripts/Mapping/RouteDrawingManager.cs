using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using Photon.Pun;
using System.Linq;


public class RouteDrawingManager : MonoBehaviour
{
    public RectTransform poiContainer;
    public GameObject uiLinePrefab;

    private string currentDrawType = null;
    private List<Vector2> drawingPoints = new List<Vector2>();
    private bool isDeleteMode = false;
    public Transform scrollViewContent;
    private PhotonView photonView;

    public GameObject lineInfoTextPrefab;
    private Dictionary<int, LineInfo> userDrawnLineInfos = new Dictionary<int, LineInfo>();



    public bool IsInDeleteMode() => isDeleteMode;

    void Awake()
    {
        photonView = GetComponent<PhotonView>();
    }


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
        object[] instData = new object[]
        {
            start,
            end,
            color.r,
            color.g,
            color.b,
            currentDrawType,
            Vector2.Distance(start, end)
        };

        GameObject go = PhotonNetwork.Instantiate("Prefabs/UILine", Vector3.zero, Quaternion.identity, 0, instData);
        go.name = "UserDrawnLine";

        // 🛠 Force into correct UI hierarchy
        RectTransform rt = go.GetComponent<RectTransform>();
        Transform mapImage = GameObject.Find("MapImage")?.transform;
        if (mapImage != null)
        {
            rt.SetParent(mapImage, false); // Important: false to preserve local scale
            rt.anchorMin = rt.anchorMax = new Vector2(0, 0);
            rt.pivot = new Vector2(0, 0.5f);

            // Reapply position/rotation now that parent is set
            Vector2 dir = (end - start).normalized;
            float dist = Vector2.Distance(start, end);

            rt.sizeDelta = new Vector2(dist, 4f);
            rt.anchoredPosition = start;
            rt.localEulerAngles = new Vector3(0, 0, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);
        }
        else
        {
            Debug.LogError("MapImage not found");
        }

        BoxCollider2D col = go.AddComponent<BoxCollider2D>();
        col.size = rt.sizeDelta;

        DeleteOnClick del = go.GetComponent<DeleteOnClick>();
        if (del != null)
        {
            del.manager = this;
        }
    }

    public void RegisterLine(int viewID, GameObject lineObj, GameObject scrollEntry, string routeType, Vector2 start, Vector2 end, float distance)
    {
        userDrawnLineInfos[viewID] = new LineInfo
        {
            lineObject = lineObj,
            scrollEntry = scrollEntry,
            routeType = routeType,
            start = start,
            end = end,
            distance = distance
        };
    }

    public void DeleteLine(GameObject line)
    {
        PhotonView pv = line.GetComponent<PhotonView>();
        if (pv != null)
        {
            photonView.RPC(nameof(RPC_DeleteLineByViewID), RpcTarget.AllBuffered, pv.ViewID);
        }
    }

    [PunRPC]
    private void RPC_DeleteLineByViewID(int viewID)
    {
        if (userDrawnLineInfos.TryGetValue(viewID, out LineInfo info))
        {
            if (info.scrollEntry != null)
                Destroy(info.scrollEntry);

            userDrawnLineInfos.Remove(viewID);
        }

        GameObject target = PhotonView.Find(viewID)?.gameObject;
        if (target != null)
            Destroy(target);

        RefreshScrollEntries(); 
    }

    public void DeleteAllUserLines()
    {
        photonView.RPC(nameof(RPC_RequestGlobalLineDelete), RpcTarget.AllBuffered);
    }

    [PunRPC]
    private void RPC_RequestGlobalLineDelete()
    {
        GameObject[] allLines = GameObject.FindGameObjectsWithTag("UserPath");

        foreach (GameObject line in allLines)
        {
            if (line == null) continue;

            PhotonView pv = line.GetComponent<PhotonView>();
            if (pv != null && pv.IsMine)
            {
                PhotonNetwork.Destroy(line);
            }
            else if (pv != null)
            {
                Destroy(line);
            }
        }

        foreach (var entry in userDrawnLineInfos.Values)
        {
            if (entry.scrollEntry != null)
                Destroy(entry.scrollEntry);
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
        return userDrawnLineInfos.Values
            .Where(info => info.routeType == type)
            .ToList();
    }

    public GameObject AddLineToScrollView(string routeType, Vector2 start, Vector2 end, float distance)
    {
        var config = ConfigLoader.MapConfig;
        GameObject entry = Instantiate(lineInfoTextPrefab, scrollViewContent);

        float scale = config.Mapping.mapScale;
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
        foreach (var info in userDrawnLineInfos.Values)
        {
            if (info.routeType == routeType)
                total += info.distance;
        }
        return total;
    }


    private void RefreshScrollEntries()
    {
        foreach (var info in userDrawnLineInfos.Values)
        {
            if (info.scrollEntry != null)
                Destroy(info.scrollEntry);
        }

        float scale = ConfigLoader.MapConfig.Mapping.mapScale;
        Dictionary<string, float> runningTotals = new();
        int counter = 1;

        Dictionary<int, LineInfo> updatedInfos = new();

        foreach (var kvp in userDrawnLineInfos)
        {
            int viewID = kvp.Key;
            LineInfo info = kvp.Value;

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
                numberText.text = $"{counter++}";

            if (coordinatesText != null)
            {
                Vector2 scaledEnd = info.end * scale;
                coordinatesText.text = $"({scaledEnd.x:0.0}, {scaledEnd.y:0.0})";
            }

            if (distanceText != null)
                distanceText.text = $"{info.distance * scale:0.0} m";

            if (totalDistanceText != null)
                totalDistanceText.text = $"{total * scale:0.0} m";

            updatedInfos[viewID] = new LineInfo
            {
                lineObject = info.lineObject,
                scrollEntry = newEntry,
                routeType = info.routeType,
                start = info.start,
                end = info.end,
                distance = info.distance
            };
        }
        userDrawnLineInfos = updatedInfos;
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
}


