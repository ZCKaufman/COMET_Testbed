using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class IVATaskLoader : MonoBehaviour
{
    [Header("UI Outputs")]
    [Header("Buttons")]
    [SerializeField] private Button craterButton;
    [SerializeField] private Button lavaTubeButton;
    [SerializeField] private Button solarPanelButton;

    private Button currentHighlightedButton;

    [SerializeField] private TMP_Text ev1Output;
    [SerializeField] private TMP_Text ev2Output;

    /* ---------- public button handlers ---------- */
    public void LoadCrater() => LoadSample("Crater Exploration", craterButton);
    public void LoadLavaTube() => LoadSample("Lava Tube Exploration", lavaTubeButton);
    public void LoadSolarPanel() => LoadSample("Solar Panel Repair", solarPanelButton);


    /* ---------- core ---------- */
    private void LoadSample(string title, Button clickedButton)
    {
        if (!ConfigLoader.IsLoaded || ConfigLoader.IVASampleTasks == null)
        {
            Debug.LogWarning("Config not loaded yet.");
            return;
        }

        if (ConfigLoader.IVASampleTasks.Samples.TryGetValue(title, out var entry))
        {
            ev1Output.text = FormatNumberedList(entry.EV1);
            ev2Output.text = FormatNumberedList(entry.EV2);
        }
        else
        {
            ev1Output.text = "No sample found.";
            ev2Output.text = "No sample found.";
        }

        HighlightButton(clickedButton);
    }

    private string FormatNumberedList(System.Collections.Generic.List<string> steps)
    {
        if (steps == null || steps.Count == 0)
            return "No steps available.";

        System.Text.StringBuilder sb = new();
        for (int i = 0; i < steps.Count; i++)
        {
            sb.AppendLine($"{i + 1}. {steps[i]}");
        }
        return sb.ToString();
    }

    private void HighlightButton(Button buttonToHighlight)
    {
        if (currentHighlightedButton != null)
        {
            // Reset the previous button's background color
            Image prevImage = currentHighlightedButton.GetComponent<Image>();
            if (prevImage != null)
                prevImage.color = Color.white;
        }

        if (buttonToHighlight != null)
        {
            // Set the new button's background color
            Image newImage = buttonToHighlight.GetComponent<Image>();
            if (newImage != null)
                newImage.color = new Color(0.8f, 0.9f, 1f); // soft blue

            currentHighlightedButton = buttonToHighlight;
        }
    }


}
