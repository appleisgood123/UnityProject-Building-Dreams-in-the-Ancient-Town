using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class BuildingEncyclopedia : MonoBehaviour
{
    [Header("主面板")]
    public GameObject encyclopediaPanel;          // 图鉴主面板
    public Transform iconContainer;               // 图标父物体（ScrollView 的 Content）

    [Header("图标预制体")]
    public GameObject buildingIconPrefab;         // 建筑图标预制体（Button + Image + Text）

    [Header("详情弹窗")]
    public GameObject detailPanel;                // 详情弹窗面板
    public Image detailImage;
    public TextMeshProUGUI detailName;
    public TextMeshProUGUI detailDesc;
    public TextMeshProUGUI detailCost;
    public TextMeshProUGUI detailIncome;
    public Button closeDetailButton;

    [Header("数据")]
    public List<BuildingDataSO> allBuildings;     // 所有建筑数据（拖入）

    private void Start()
    {
        encyclopediaPanel.SetActive(false);
        detailPanel.SetActive(false);
        closeDetailButton.onClick.AddListener(() => detailPanel.SetActive(false));
        RefreshIconList();
    }

    public void OpenEncyclopedia()
    {
        encyclopediaPanel.SetActive(true);
    }

    public void CloseEncyclopedia()
    {
        encyclopediaPanel.SetActive(false);
        detailPanel.SetActive(false);
    }

    private void RefreshIconList()
    {
        // 清空旧图标
        foreach (Transform child in iconContainer)
            Destroy(child.gameObject);

        foreach (var building in allBuildings)
        {
            GameObject iconObj = Instantiate(buildingIconPrefab, iconContainer);
            Image iconImg = iconObj.GetComponent<Image>();
            if (iconImg != null && building.iconSprite != null)
                iconImg.sprite = building.iconSprite;

            TextMeshProUGUI nameText = iconObj.GetComponentInChildren<TextMeshProUGUI>();
            if (nameText != null) nameText.text = building.buildingName;

            Button btn = iconObj.GetComponent<Button>();
            if (btn != null)
            {
                BuildingDataSO captured = building;
                btn.onClick.AddListener(() => ShowDetail(captured));
            }
        }
    }

    private void ShowDetail(BuildingDataSO building)
    {
        if (detailImage != null && building.displayImage != null)
            detailImage.sprite = building.displayImage;
        else if (detailImage != null && building.iconSprite != null)
            detailImage.sprite = building.iconSprite;

        detailName.text = building.buildingName;
        detailDesc.text = building.description;

        string cost = $"消耗：银两{building.costSilver}  木材{building.costWood}  砖石{building.costStone}";
        detailCost.text = cost;

        string income = "每月收益：";
        if (building.monthlySilver > 0) income += $"银两+{building.monthlySilver} ";
        if (building.monthlyWood > 0) income += $"木材+{building.monthlyWood} ";
        if (building.monthlyStone > 0) income += $"砖石+{building.monthlyStone} ";
        if (income == "每月收益：") income += "无";
        detailIncome.text = income;

        detailPanel.SetActive(true);
    }
}