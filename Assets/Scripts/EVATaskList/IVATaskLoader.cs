using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IVATaskLoader : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button craterButton;
    [SerializeField] private Button lavaTubeButton;
    [SerializeField] private Button psrButton;          // Permanently Shadowed Region
    [SerializeField] private Button sampleButton;       // Soil Sample

    [Header("Task Text Output")]
    [SerializeField] private TMP_Text taskOutput;       // single list, numbered

    private Button currentHighlightedButton;

    /* ---------- public button handlers ---------- */
    public void LoadCrater()      => LoadSample("Crater Exploration", craterButton);
    public void LoadLavaTube()    => LoadSample("Lava Tube Exploration", lavaTubeButton);
    public void LoadPSR()         => LoadSample("Permanently Shadowed Region", psrButton);
    public void LoadSampleCol()   => LoadSample("Soil Sample", sampleButton);

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
            taskOutput.text = FormatNumberedList(entry.EVA);
        }
        else
        {
            taskOutput.text = "No sample found for \"" + title + "\".";
        }

        HighlightButton(clickedButton);
    }

    private string FormatNumberedList(System.Collections.Generic.List<string> steps)
    {
        if (steps == null || steps.Count == 0)
            return "No steps available.";

        System.Text.StringBuilder sb = new();
        for (int i = 0; i < steps.Count; i++)
            sb.AppendLine($"{i + 1}. {steps[i]}");
        return sb.ToString();
    }

    private void HighlightButton(Button buttonToHighlight)
    {
        if (currentHighlightedButton != null)
        {
            Image prevImage = currentHighlightedButton.GetComponent<Image>();
            if (prevImage != null) prevImage.color = Color.white;
        }

        if (buttonToHighlight != null)
        {
            Image newImage = buttonToHighlight.GetComponent<Image>();
            if (newImage != null) newImage.color = new Color(0.8f, 0.9f, 1f);
            currentHighlightedButton = buttonToHighlight;
        }
    }
}
