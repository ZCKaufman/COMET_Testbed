using UnityEngine;

public class MatchParentSize : MonoBehaviour
{
    void LateUpdate()
    {
        RectTransform rt = GetComponent<RectTransform>();
        RectTransform parentRT = transform.parent as RectTransform;
        if (rt != null && parentRT != null)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }
    }
}

