using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;




[System.Serializable]
public class LandmarkPrefabEntry
{
    public string type;
    public GameObject prefab;
}


public class POIManager : MonoBehaviour
{
    public RectTransform poiContainer;
    public GameObject poiPrefab;
    public GameObject uiLinePrefab;


    private List<GameObject> spawnedPOIs = new List<GameObject>();
    public List<LandmarkPrefabEntry> landmarkPrefabs;
    private Dictionary<string, GameObject> landmarkPrefabMap;


    private List<GameObject> predefinedLines = new List<GameObject>();


    private void Start()
    {
        StartCoroutine(InitializeAfterConfig());
    }

    private IEnumerator InitializeAfterConfig()
    {
        // wait until the config is ready 
        while (ConfigLoader.EVAMapConfig == null || ConfigLoader.EVAMapConfig.EVAMapping == null)
        {
            yield return null;
        }

        landmarkPrefabMap = new Dictionary<string, GameObject>();
        foreach (var entry in landmarkPrefabs)
        {
            if (!landmarkPrefabMap.ContainsKey(entry.type))
            {
                landmarkPrefabMap.Add(entry.type, entry.prefab);
            }
            else
            {
                Debug.LogWarning($"Duplicate landmark type: {entry.type}");
            }
        }

        ShowLandmarks();
    }

    public void ShowPOIs()
    {
        // Check if the player is IVA
        object role;
        PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("Role", out role);
        string playerRole = role as string;

        if (playerRole != "IVA")
        {
            Debug.Log("Only IVA users can view POIs.");
            return;
        }

        ClearPOIs();

        var config = ConfigLoader.EVAMapConfig;
        if (config?.EVAMapping?.POIs == null || poiPrefab == null || poiContainer == null)
        {
            Debug.LogError("Missing config, POIs, prefab, or container.");
            return;
        }

        foreach (var poi in config.EVAMapping.POIs)
        {
            GameObject instance = Instantiate(poiPrefab, poiContainer);
            instance.name = poi.description;

            RectTransform rect = instance.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(poi.x, poi.y);
            rect.anchoredPosition = Vector2.zero;

            POIDisplay display = instance.GetComponent<POIDisplay>();
            if (display != null)
            {
                display.SetDescription(poi.description);
            }

            spawnedPOIs.Add(instance);
        }
    }

    public void ShowLandmarks()
    {
        var config = ConfigLoader.EVAMapConfig;
        if (config?.EVAMapping?.Landmarks == null || poiContainer == null)
        {
            Debug.LogError("Missing config, Landmarks, prefab, or container.");
            return;
        }


        foreach (var landmark in config.EVAMapping.Landmarks)
        {
            if (!landmarkPrefabMap.ContainsKey(landmark.type))
            {
                Debug.LogWarning($"No prefab assigned for landmark type: {landmark.type}");
                continue;
            }


            GameObject prefab = landmarkPrefabMap[landmark.type];
            GameObject instance = Instantiate(prefab, poiContainer);
            instance.name = landmark.description;


            RectTransform rect = instance.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(landmark.x, landmark.y);
            rect.anchoredPosition = Vector2.zero;


            POIDisplay display = instance.GetComponent<POIDisplay>();
            if (display != null)
                display.SetDescription(landmark.description);
        }
    }

    public void ShowRoutes()
    {
        // Check if the player is IVA
        object role;
        PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("Role", out role);
        string playerRole = role as string;

        if (playerRole != "IVA")
        {
            Debug.Log("Only IVA users can view routes.");
            return;
        }

        ClearRoutes();

        var config = ConfigLoader.EVAMapConfig;
        if (config?.EVAMapping?.PredefinedRoutes == null || uiLinePrefab == null || poiContainer == null)
        {
            Debug.LogError("Missing config, prefab, or container.");
            return;
        }

        foreach (var route in config.EVAMapping.PredefinedRoutes)
        {
            for (int i = 0; i < route.points.Count - 1; i++)
            {
                Vector2 start = GetAnchoredPosition(new Vector2(route.points[i].x, route.points[i].y));
                Vector2 end = GetAnchoredPosition(new Vector2(route.points[i + 1].x, route.points[i + 1].y));

                Color color = route.type == "walk"
                    ? new Color(0.2f, 0.5f, 0.2f)
                    : new Color(0.6f, 0.1f, 0.1f);

                bool dashed = route.type == "walk";
                DrawUILine(start, end, color, dashed);
            }
        }
    }

    private void DrawUILine(Vector2 start, Vector2 end, Color color, bool dashed = false)
    {
        if (!dashed)
        {
            CreateLineSegment(start, end, color);
            return;
        }


        float totalLength = Vector2.Distance(start, end);
        Vector2 dir = (end - start).normalized;
        float dashLength = 10f;
        float gapLength = 5f;


        float currentLength = 0f;
        while (currentLength < totalLength)
        {
            float segmentStart = currentLength;
            float segmentEnd = Mathf.Min(currentLength + dashLength, totalLength);


            Vector2 segStart = start + dir * segmentStart;
            Vector2 segEnd = start + dir * segmentEnd;


            CreateLineSegment(segStart, segEnd, color);


            currentLength += dashLength + gapLength;
        }
    }


    private void CreateLineSegment(Vector2 start, Vector2 end, Color color)
    {
        GameObject lineGO = Instantiate(uiLinePrefab, poiContainer);
        lineGO.name = "PredefinedLine";
        predefinedLines.Add(lineGO);


        Image image = lineGO.GetComponent<Image>();
        image.color = color;


        RectTransform rt = lineGO.GetComponent<RectTransform>();
        Vector2 dir = (end - start).normalized;
        float distance = Vector2.Distance(start, end);


        rt.sizeDelta = new Vector2(distance, 4f);
        rt.anchorMin = rt.anchorMax = new Vector2(0, 0);
        rt.pivot = new Vector2(0, 0.5f);
        rt.anchoredPosition = start;
        rt.localEulerAngles = new Vector3(0, 0, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);
    }


    public void ClearPOIs()
    {
        foreach (var obj in spawnedPOIs)
        {
            if (obj != null)
                Destroy(obj);
        }
        spawnedPOIs.Clear();
    }


    public void ClearRoutes()
    {
        foreach (var line in predefinedLines)
        {
            if (line != null)
                Destroy(line);
        }
        predefinedLines.Clear();
    }


    public Vector2 GetAnchoredPosition(Vector2 norm)
    {
        Vector2 containerSize = poiContainer.rect.size;
        return new Vector2(norm.x * containerSize.x, norm.y * containerSize.y);
    }
}
