using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;

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
    public bool IsSpriteReady => imageToDisplay != null;
    public System.Action OnSpriteReady;


    private void Awake()
    {
        button = GetComponent<Button>();
        image = GetComponent<Image>();
        highlight = transform.Find("Highlight")?.gameObject;

        if (button != null)
            button.onClick.AddListener(OnClick);

        SetSelected(false);
    }

    private IEnumerator Start()
    {
        while (ConfigLoader.EVAMapConfig == null || ConfigLoader.EVAMapConfig.EVAMapping == null)
            yield return null;

        var config = ConfigLoader.EVAMapConfig;
        var match = config.EVAMapping.Maps.Find(entry => entry.key == imageKey);
        if (match == null)
        {
            Debug.LogError($"No entry found in config for key: {imageKey}");
            yield break;
        }

        imageToDisplay = Resources.Load<Sprite>(match.path);
        if (imageToDisplay == null)
        {
            Debug.LogError($"Failed to load sprite at: {match.path}");
            yield break;
        }

        OnSpriteReady?.Invoke();
    }


    private void OnClick()
    {
        if (group != null && imageToDisplay != null)
        {
            group.SelectButton(this);

            object role;
            PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("Role", out role);
            string playerRole = role as string;

            if (imageKey == "POIs" && poiManager != null)
            {
                if (playerRole == "IVA")
                    poiManager.ShowPOIs();
                else
                    Debug.Log("Only IVA users can view POIs.");
            }
            else if (poiManager != null)
            {
                poiManager.ClearPOIs();
            }
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
