using UnityEngine;
using UnityEngine.UI;

public class SelectableButton : MonoBehaviour
{
    private Button button;
    private Image image;

    public ButtonGroupManager groupManager;

    private GameObject highlight;

    private void Awake()
    {
        button = GetComponent<Button>();
        image = GetComponent<Image>();
        highlight = transform.Find("Highlight")?.gameObject;

        if (button != null)
            button.onClick.AddListener(OnClick);

        if (highlight != null)
            highlight.SetActive(false);
    }

    public void OnClick()
    {
        groupManager.SelectButton(this);
    }

    public void SetSelected(bool selected)
    {
        Color selectedColor = new Color(186f / 255f, 215f / 255f, 255f / 255f);
        Color defaultColor = Color.white;

        image.color = selected ? selectedColor : defaultColor;

        if (highlight != null)
            highlight.SetActive(selected);
    }
}
