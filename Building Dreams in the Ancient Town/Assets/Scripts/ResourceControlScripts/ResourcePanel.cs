using TMPro;
using UnityEngine;

public class ResourcePanel : MonoBehaviour
{
    public TextMeshProUGUI silverText;
    public TextMeshProUGUI woodText;
    public TextMeshProUGUI stoneText;
    public TextMeshProUGUI happinessText;
    public TextMeshProUGUI populationText;
    public TextMeshProUGUI techPointsText; // 显示科技点

    private void Start()
    {
        if (ResourceManager.Instance != null)
            ResourceManager.Instance.OnResourcesChanged += UpdateUI;
        UpdateUI();
    }

    private void OnDestroy()
    {
        if (ResourceManager.Instance != null)
            ResourceManager.Instance.OnResourcesChanged -= UpdateUI;
    }

    private void UpdateUI()
    {
        if (ResourceManager.Instance == null) return;

        silverText.text = ResourceManager.Instance.Silver.ToString();
        woodText.text = $"{ResourceManager.Instance.Wood}/{ResourceManager.Instance.WoodCap}";
        stoneText.text = $"{ResourceManager.Instance.Stone}/{ResourceManager.Instance.StoneCap}";
        happinessText.text = ResourceManager.Instance.Happiness.ToString();

        int currentPopulation = GameManager.Instance != null ? GameManager.Instance.GetEmployeeList().Count : 0;
        populationText.text = $"{currentPopulation}/{ResourceManager.Instance.PopulationCap}";

        if (techPointsText != null)
            techPointsText.text = ResourceManager.Instance.TechPoints.ToString();
    }
}