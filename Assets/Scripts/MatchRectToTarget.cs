using UnityEngine;

public class MatchRectToTarget : MonoBehaviour
{
    [SerializeField] private RectTransform targetRect;
    [SerializeField] private RectTransform referenceParent; 

    void LateUpdate()
    {
        if (targetRect != null && referenceParent != null)
        {
            targetRect.anchorMin = Vector2.zero;
            targetRect.anchorMax = Vector2.one;
            targetRect.offsetMin = Vector2.zero;
            targetRect.offsetMax = Vector2.zero;

            if (targetRect.parent != referenceParent)
            {
                targetRect.SetParent(referenceParent, worldPositionStays: false);
            }
        }
    }
}
