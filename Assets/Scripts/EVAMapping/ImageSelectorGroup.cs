using UnityEngine;
using UnityEngine.UI;

public class ImageSelectorGroup : MonoBehaviour
{
    public Image targetImage;

    private ImageSelectorButton currentlySelected;
    public ImageSelectorButton defaultButton;

    private void Start()
    {
        if (defaultButton != null)
        {
            SelectButton(defaultButton);
        }
    }


    public void SelectButton(ImageSelectorButton selected)
    {
        if (currentlySelected != null)
        {
            currentlySelected.SetSelected(false);
        }

        selected.SetSelected(true);
        currentlySelected = selected;

        if (targetImage != null)
        {
            targetImage.sprite = selected.imageToDisplay;
        }
    }
}
