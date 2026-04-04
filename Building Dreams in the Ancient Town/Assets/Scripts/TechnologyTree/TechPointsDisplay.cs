using UnityEngine;
using TMPro;

public class TechPointsDisplay : MonoBehaviour
{
    public TextMeshProUGUI techPointsText; // ÔÚInspectorÖÐÍÏ×§¸³Öµ

    private void OnEnable()
    {
        if (ResourceManager.Instance != null)
        {
            ResourceManager.Instance.OnResourcesChanged += UpdateDisplay;
            UpdateDisplay();
        }
    }

    private void OnDisable()
    {
        if (ResourceManager.Instance != null)
            ResourceManager.Instance.OnResourcesChanged -= UpdateDisplay;
    }

    private void UpdateDisplay()
    {
        if (techPointsText != null && ResourceManager.Instance != null)
            techPointsText.text = ResourceManager.Instance.TechPoints.ToString();
    }
}