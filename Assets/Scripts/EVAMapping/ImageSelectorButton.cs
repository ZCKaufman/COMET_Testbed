using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ImageSelectorButton : MonoBehaviour
{
    public Sprite imageToDisplay;
    public ImageSelectorGroup group;
    public List<GameObject> objectsToToggle;

    private Button button;
    private Image image;
    private GameObject highlight;

    private void Awake()
    {
        button = GetComponent<Button>();
        image = GetComponent<Image>();
        highlight = transform.Find("Highlight")?.gameObject;

        if (button != null)
            button.onClick.AddListener(OnClick);

        SetSelected(false);
    }

    private void OnClick()
    {
        if (group != null && imageToDisplay != null)
        {
            group.SelectButton(this);
        }
    }

    public void SetSelected(bool selected)
    {
        if (image != null)
            image.color = selected ? new Color(186f / 255f, 215f / 255f, 255f / 255f) : Color.white;

        if (highlight != null)
            highlight.SetActive(selected);

        // 👇 Show/hide linked objects
        if (objectsToToggle != null)
        {
            foreach (var obj in objectsToToggle)
            {
                if (obj != null)
                    obj.SetActive(selected);
            }
        }
    }
}
