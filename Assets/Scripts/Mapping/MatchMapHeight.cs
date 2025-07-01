using UnityEngine;

public class MatchMapHeight : MonoBehaviour
{
    public RectTransform mapContainer;
    public RectTransform numberGridBar; 

    void Update()
    {
        RectTransform rt = GetComponent<RectTransform>();
        if (rt != null && mapContainer != null && numberGridBar != null)
        {
            Vector3 mapBottom = mapContainer.position - new Vector3(0, mapContainer.rect.height * mapContainer.pivot.y * mapContainer.lossyScale.y, 0);
            Vector3 barBottom = numberGridBar.position - new Vector3(0, numberGridBar.rect.height * numberGridBar.pivot.y * numberGridBar.lossyScale.y, 0);

            float worldHeight = Mathf.Abs(barBottom.y - mapBottom.y);
            Vector2 size = rt.sizeDelta;
            size.y = worldHeight / rt.lossyScale.y;
            rt.sizeDelta = size;

            Vector3 barTop = numberGridBar.position + new Vector3(0, numberGridBar.rect.height * (1 - numberGridBar.pivot.y) * numberGridBar.lossyScale.y, 0);
            Vector3 localPos = rt.parent.InverseTransformPoint(barTop);
            rt.localPosition = new Vector3(rt.localPosition.x,localPos.y - numberGridBar.rect.height,rt.localPosition.z);

        }
    }
}
