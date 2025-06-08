using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;


public class ImageSelectorButton : MonoBehaviour
{
    public string imageKey;
    public ImageSelectorGroup group;
    public List<GameObject> objectsToToggle;


    private Button button;
    private Image image;
    private GameObject highlight;
    private Sprite imageToDisplay;
    public POIManager poiManager;


    private void Start()
    {
        var config = ConfigLoader.LoadedConfig;
        if (config == null || config.EVAMapping == null || config.EVAMapping.Maps == null)
        {
            Debug.LogError("Config or Maps list is null.");
            return;
        }


        var match = config.EVAMapping.Maps.Find(entry => entry.key == imageKey);
        if (match == null)
        {
            Debug.LogError($"No entry found in config for key: {imageKey}");
            return;
        }


        imageToDisplay = Resources.Load<Sprite>(match.path);


        if (imageToDisplay == null)
        {
            Debug.LogError($"Failed to load sprite at: {match.path}");
        }
    }

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
            if (imageKey == "POIs" && poiManager != null)
                poiManager.ShowPOIs();
            else if (poiManager != null)
                poiManager.ClearPOIs();
        }
    }


    public void SetSelected(bool selected)
    {
        if (image != null)
            image.color = selected ? new Color(186f / 255f, 215f / 255f, 255f / 255f) : Color.white;


        if (highlight != null)
            highlight.SetActive(selected);


        if (objectsToToggle != null)
        {
            foreach (var obj in objectsToToggle)
                if (obj != null) obj.SetActive(selected);
        }
    }


    public Sprite GetSprite()
    {
        return imageToDisplay;
    }
}


