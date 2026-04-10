using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class BuildingInfoPanel : MonoBehaviour
{
    public static BuildingInfoPanel Instance;

    [Header("主内容面板（必须拖拽）")]
    public GameObject contentPanel;              // 包含所有UI内容的子物体

    [Header("UI组件")]
    public TextMeshProUGUI nameText;
    public Image displayImage;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI incomeText;
    public TextMeshProUGUI immediateText;
    public Button closeButton;

    [Header("员工管理")]
    public TextMeshProUGUI employeeStatusText;
    public GameObject employeeSelectionPanel;
    public TMP_FontAsset employeeButtonFont;

    private BuildingInstance currentBuilding;
    public System.Action OnPanelClosed;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        // 初始隐藏内容面板
        if (contentPanel != null)
            contentPanel.SetActive(false);
        else
            Debug.LogError("BuildingInfoPanel: contentPanel 未赋值！");

        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(Close);
        }

        if (employeeStatusText != null)
        {
            Button statusBtn = employeeStatusText.GetComponent<Button>();
            if (statusBtn == null)
                statusBtn = employeeStatusText.gameObject.AddComponent<Button>();
            statusBtn.onClick.RemoveAllListeners();
            statusBtn.onClick.AddListener(OnEmployeeStatusClicked);
        }

        if (employeeSelectionPanel != null)
            employeeSelectionPanel.SetActive(false);
    }

    public void Close()
    {
        if (contentPanel == null || !contentPanel.activeSelf) return;
        contentPanel.SetActive(false);
        OnPanelClosed?.Invoke();
        if (MouseManager.Instance != null)
            MouseManager.Instance.SetCursorVisible(false);
        if (GamePauseManager.Instance != null)
            GamePauseManager.Instance.RequestResume();
        Debug.Log("建筑信息面板已关闭");
    }

    public void Show(BuildingDataSO data, BuildingInstance buildingInstance)
    {
        if (data == null) return;
        currentBuilding = buildingInstance;

        if (MouseManager.Instance != null)
            MouseManager.Instance.SetCursorVisible(true);
        if (GamePauseManager.Instance != null)
            GamePauseManager.Instance.RequestPause();

        nameText.text = data.buildingName;
        if (displayImage != null)
        {
            if (data.displayImage != null)
                displayImage.sprite = data.displayImage;
            else
                displayImage.gameObject.SetActive(false);
        }
        descriptionText.text = data.description;

        string monthly = "";
        if (data.monthlySilver > 0) monthly += $"银两+{data.monthlySilver} ";
        if (data.monthlyWood > 0) monthly += $"木材+{data.monthlyWood} ";
        if (data.monthlyStone > 0) monthly += $"砖石+{data.monthlyStone} ";
        if (string.IsNullOrEmpty(monthly)) monthly = "无";
        incomeText.text = $"每月收益：{monthly}";

        string immediate = "";
        if (data.incomeHappiness > 0) immediate += $"幸福度+{data.incomeHappiness} ";
        if (data.populationCapIncrease > 0) immediate += $"人口上限+{data.populationCapIncrease} ";
        if (string.IsNullOrEmpty(immediate)) immediate = "无";
        immediateText.text = $"立即收益：{immediate}";

        RefreshEmployeeStatus();
        contentPanel.SetActive(true);
        Debug.Log("建筑信息面板已显示");
    }

    // 其余方法保持不变...
    private void RefreshEmployeeStatus() { /* 原代码 */ }
    private void OnEmployeeStatusClicked() { /* 原代码 */ }
    private void OnSelectEmployee(string employeeUID) { /* 原代码 */ }
}