using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TechDetailPanel : MonoBehaviour
{
    public GameObject panel;
    public TextMeshProUGUI techNameText;
    public TextMeshProUGUI descriptionText;
    public Transform costContainer;          // 用于动态生成消耗项
    public GameObject costItemPrefab;         // 消耗项预制体（需包含资源名和数值文本）
    public TextMeshProUGUI effectText;
    public TextMeshProUGUI requirementHappinessText; // 新增：显示幸福度要求
    public Button confirmButton;
    public Button cancelButton;

    private TechNodeData currentTech;
    private TechNodeUI currentTechNodeUI;

    private void Awake()
    {
        panel.SetActive(false);
        confirmButton.onClick.AddListener(OnConfirm);
        cancelButton.onClick.AddListener(OnCancel);
    }

    public void Show(TechNodeData tech, TechNodeUI nodeUI)
    {
        if (tech == null) return;

        currentTech = tech;
        currentTechNodeUI = nodeUI;

        techNameText.text = tech.nodeName;
        descriptionText.text = tech.description;

        // 清空消耗容器
        foreach (Transform child in costContainer)
            Destroy(child.gameObject);

        // 动态创建消耗项（仅数值 >0 的显示）
        if (tech.requiredSilver > 0) CreateCostItem("银两", tech.requiredSilver.ToString());
        if (tech.requiredWood > 0) CreateCostItem("木材", tech.requiredWood.ToString());
        if (tech.requiredStone > 0) CreateCostItem("砖石", tech.requiredStone.ToString());
        if (tech.requiredTechPoints > 0) CreateCostItem("科技点", tech.requiredTechPoints.ToString());

        // 显示幸福度要求
        if (requirementHappinessText != null)
        {
            if (tech.requiredHappiness > 0)
                requirementHappinessText.text = $"需求幸福度：{tech.requiredHappiness}";
            else
                requirementHappinessText.text = "需求幸福度：无";
        }

        effectText.text = GetEffectDescription(tech);

        // 确认按钮根据当前可解锁状态决定是否可用
        confirmButton.interactable = TechManager.Instance != null && TechManager.Instance.CanUnlock(tech);

        panel.SetActive(true);
    }

    private void CreateCostItem(string name, string value)
    {
        GameObject item = Instantiate(costItemPrefab, costContainer);
        TextMeshProUGUI[] texts = item.GetComponentsInChildren<TextMeshProUGUI>();
        if (texts.Length >= 2)
        {
            texts[0].text = name;
            texts[1].text = value;
        }
    }

    private string GetEffectDescription(TechNodeData tech)
    {
        switch (tech.effectType)
        {
            case TechEffectType.IncreaseSilverIncome:
                return $"银两收入 +{tech.effectValue}%";
            case TechEffectType.IncreaseWoodIncome:
                return $"木材收入 +{tech.effectValue}%";
            case TechEffectType.IncreaseStoneIncome:
                return $"砖石收入 +{tech.effectValue}%";
            case TechEffectType.IncreaseBuildingHappiness:
                return $"指定建筑幸福度 +{tech.effectValue}";
            case TechEffectType.IncreasePopulationCap:
                return $"人口上限 +{tech.effectValue}";
            default:
                return "无效果";
        }
    }

    private void OnConfirm()
    {
        if (currentTech != null && TechManager.Instance != null)
        {
            bool success = TechManager.Instance.UnlockTech(currentTech);
            if (success)
            {
                currentTechNodeUI?.Refresh();
                Hide();
            }
            else
            {
                Debug.LogWarning("科技解锁失败，可能条件不足");
            }
        }
    }

    private void OnCancel()
    {
        Hide();
    }

    public void Hide()
    {
        panel.SetActive(false);
        currentTech = null;
        currentTechNodeUI = null;
    }
}