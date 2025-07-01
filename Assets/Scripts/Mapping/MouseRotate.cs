using UnityEngine;

public class MouseRotate : MonoBehaviour
{
    private RectTransform rt;
    private bool isRotating = false;
    private Vector3 lastMousePosition;

    void Start()
    {
        rt = GetComponent<RectTransform>();
        if (rt == null)
            rt = GetComponent<Transform>() as RectTransform;
    }

    void Update()
    {
       // RIGHT CLICK — ROTATE
        if (Input.GetMouseButtonDown(0))
        {
            isRotating = true;
            lastMousePosition = Input.mousePosition;
        }

        if (Input.GetMouseButton(0) && isRotating)
        {
            float deltaX = Input.mousePosition.x - lastMousePosition.x;
            rt.Rotate(0f, 0f, -deltaX * 0.5f);  // negative = clockwise
            lastMousePosition = Input.mousePosition;
        }

        if (Input.GetMouseButtonUp(0))
        {
            isRotating = false;
        }
    }
}
