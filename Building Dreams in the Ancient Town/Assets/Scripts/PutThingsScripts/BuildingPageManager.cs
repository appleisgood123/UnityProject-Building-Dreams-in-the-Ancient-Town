using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class BuildingPageManager : MonoBehaviour
{
    [Header("页面设置")]
    public List<GameObject> slots;                    // 手动拖入6个占位符
    public List<BuildingDataSO> allBuildings;         // 所有建筑数据（ScriptableObject）

    [Header("翻页按钮")]
    public Button prevButton;
    public Button nextButton;

    private int currentPage = 0;
    private int buildingsPerPage => slots.Count;

    private List<Image> slotImages;
    private List<DraggableIcon> slotDraggables;

    void Start()
    {
        slotImages = new List<Image>();
        slotDraggables = new List<DraggableIcon>();

        foreach (var slot in slots)
        {
            if (slot == null)
            {
                Debug.LogError("slots 列表中存在空引用！");
                continue;
            }
            Image img = slot.GetComponent<Image>();
            DraggableIcon drag = slot.GetComponent<DraggableIcon>();

            if (img == null)
                Debug.LogError($"Slot {slot.name} 缺少 Image 组件！");
            if (drag == null)
                Debug.LogError($"Slot {slot.name} 缺少 DraggableIcon 组件！");

            slotImages.Add(img);
            slotDraggables.Add(drag);
        }

        prevButton.onClick.AddListener(PrevPage);
        nextButton.onClick.AddListener(NextPage);

        RefreshPage();
    }

    void OnDestroy()
    {
        prevButton.onClick.RemoveListener(PrevPage);
        nextButton.onClick.RemoveListener(NextPage);
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
                   slotImages[i].sprite = data.iconSprite; // 如果你有 icon 字段，否则可注释掉
                if (slotDraggables[i] != null)
                    slotDraggables[i].buildingData = data;   // 传递建筑数据
                slots[i].SetActive(true);
            }
            else
            {
                slots[i].SetActive(false);
            }
        }

        prevButton.interactable = (currentPage > 0);
        nextButton.interactable = ((currentPage + 1) * buildingsPerPage < allBuildings.Count);
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