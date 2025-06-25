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
            // Delay selection until sprite is ready
            if (defaultButton.IsSpriteReady)
            {
                SelectButton(defaultButton);
            }
            else
            {
                defaultButton.OnSpriteReady += () =>
                {
                    SelectButton(defaultButton);
                };
            }
        }
    }



    public void SelectButton(ImageSelectorButton selected)
    {
        if (currentlySelected != null)
        {
            currentlySelected.SetSelected(false);
            if (currentlySelected.imageKey == "POIs" && currentlySelected.poiManager != null)
            {
                currentlySelected.poiManager.ClearPOIs();
            }


            if (currentlySelected.imageKey == "Routes" && currentlySelected.poiManager != null)
            {
                currentlySelected.poiManager.ClearRoutes();
            }
        }


        selected.SetSelected(true);
        currentlySelected = selected;


        if (targetImage != null)
        {
            targetImage.sprite = selected.GetSprite();
        }


        // Show POIs if the newly selected button is "POIs"
        if (selected.imageKey == "POIs" && selected.poiManager != null)
        {
            selected.poiManager.ShowPOIs();
        }


        if (selected.imageKey == "Routes" && selected.poiManager != null)
        {
            Debug.Log("Showing routes for selected button.");
            selected.poiManager.ShowRoutes();
        }
    }
}
