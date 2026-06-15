using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class BuildingPageManager : MonoBehaviour
{
    [Header("页面设置")]
    public List<GameObject> slots;
    public List<BuildingDataSO> allBuildings;

    [Header("翻页按钮")]
    public Button prevButton;
    public Button nextButton;

    [Header("工具提示")]
    public GameObject tooltipPanel;
    public TextMeshProUGUI tooltipNameText;
    public TextMeshProUGUI tooltipCostText;
    public TextMeshProUGUI tooltipHappinessText;
    public TextMeshProUGUI tooltipRequirementText;

    private int currentPage = 0;
    private int buildingsPerPage => slots.Count;

    private List<Image> slotImages;
    private List<DraggableIcon> slotDraggables;
    private Coroutine hideTooltipCoroutine;

    void Start()
    {
        slotImages = new List<Image>();
        slotDraggables = new List<DraggableIcon>();

        for (int i = 0; i < slots.Count; i++)
        {
            var slot = slots[i];
            if (slot == null) continue;

            Image img = slot.GetComponent<Image>();
            DraggableIcon drag = slot.GetComponent<DraggableIcon>();
            slotImages.Add(img);
            slotDraggables.Add(drag);

            // 添加 EventTrigger
            EventTrigger trigger = slot.GetComponent<EventTrigger>();
            if (trigger == null) trigger = slot.AddComponent<EventTrigger>();
            trigger.triggers.Clear();

            int idx = i;
            EventTrigger.Entry enter = new EventTrigger.Entry();
            enter.eventID = EventTriggerType.PointerEnter;
            enter.callback.AddListener((data) => OnSlotPointerEnter(idx));
            trigger.triggers.Add(enter);

            EventTrigger.Entry exit = new EventTrigger.Entry();
            exit.eventID = EventTriggerType.PointerExit;
            exit.callback.AddListener((data) => OnSlotPointerExit());
            trigger.triggers.Add(exit);
        }

        prevButton.onClick.AddListener(PrevPage);
        nextButton.onClick.AddListener(NextPage);
        RefreshPage();

        if (tooltipPanel != null) tooltipPanel.SetActive(false);
    }

    void OnDestroy()
    {
        prevButton.onClick.RemoveListener(PrevPage);
        nextButton.onClick.RemoveListener(NextPage);
    }

    void OnSlotPointerEnter(int slotIndex)
    {
        if (hideTooltipCoroutine != null)
        {
            StopCoroutine(hideTooltipCoroutine);
            hideTooltipCoroutine = null;
        }

        int dataIndex = currentPage * buildingsPerPage + slotIndex;
        if (dataIndex >= allBuildings.Count) return;
        BuildingDataSO data = allBuildings[dataIndex];
        if (data == null) return;

        // 更新工具提示内容
        tooltipNameText.text = data.buildingName;
        tooltipCostText.text = $"花费: 银两{data.costSilver} 木材{data.costWood} 砖石{data.costStone}";
        tooltipHappinessText.text = $"需求幸福度: {data.requiredHappiness}";
        string requirement = "";
        if (data.requiredBuilding != null)
            requirement = $"前置建筑: {data.requiredBuilding.buildingName}";
        else if (data.requireTechUnlock)
            requirement = "需要科技解锁";
        else
            requirement = "无条件";
        tooltipRequirementText.text = requirement;

        // 显示并跟随鼠标
        tooltipPanel.SetActive(true);
        Vector2 mousePos = Input.mousePosition;
        tooltipPanel.transform.position = mousePos + new Vector2(20, -20);
    }

    void OnSlotPointerExit()
    {
        if (hideTooltipCoroutine != null) StopCoroutine(hideTooltipCoroutine);
        hideTooltipCoroutine = StartCoroutine(HideTooltipAfterDelay());
    }

    IEnumerator HideTooltipAfterDelay()
    {
        yield return new WaitForSeconds(0.05f);
        if (tooltipPanel != null) tooltipPanel.SetActive(false);
        hideTooltipCoroutine = null;
    }

    void RefreshPage()
    {
        int startIndex = currentPage * buildingsPerPage;
        for (int i = 0; i < slots.Count; i++)
        {
            int dataIndex = startIndex + i;
            if (dataIndex < allBuildings.Count)
            {
                BuildingDataSO data = allBuildings[dataIndex];
                if (slotImages[i] != null)
                    slotImages[i].sprite = data.iconSprite;
                if (slotDraggables[i] != null)
                    slotDraggables[i].buildingData = data;

                slots[i].SetActive(true);

                bool canBuild = CanBuildBuilding(data);
                slotImages[i].color = canBuild ? Color.white : Color.gray;
                if (slotDraggables[i] != null)
                    slotDraggables[i].enabled = canBuild;
            }
            else
            {
                slots[i].SetActive(false);
            }
        }
        prevButton.interactable = (currentPage > 0);
        nextButton.interactable = ((currentPage + 1) * buildingsPerPage < allBuildings.Count);
    }

    private bool CanBuildBuilding(BuildingDataSO data)
    {
        if (!ResourceManager.Instance.CanAfford(data.costSilver, data.costWood, data.costStone)) return false;
        if (ResourceManager.Instance.Happiness < data.requiredHappiness) return false;
        if (data.requiredBuilding != null && !BuildingManager.Instance.GetConstructedBuildings().Contains(data.requiredBuilding)) return false;
        if (data.requireTechUnlock && TechManager.Instance != null && !TechManager.Instance.IsBuildingUnlocked(data)) return false;
        return true;
    }

    void PrevPage()
    {
        if (currentPage > 0)
        {
            currentPage--;
            RefreshPage();
        }
    }

    void NextPage()
    {
        if ((currentPage + 1) * buildingsPerPage < allBuildings.Count)
        {
            currentPage++;
            RefreshPage();
        }
    }
}