using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class BuildingInfoPanel : MonoBehaviour
{
    public static BuildingInfoPanel Instance;

    [Header("面板内容根物体")]
    public GameObject panel;

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
    public BuildingInstance CurrentBuilding => currentBuilding;

    public bool IsVisible => panel != null && panel.activeSelf;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        // 自动查找面板
        if (panel == null)
            panel = transform.Find("PanelContent")?.gameObject;

        if (panel != null) panel.SetActive(false);
        else Debug.LogError("未找到 PanelContent");

        if (closeButton != null)
            closeButton.onClick.AddListener(Close);

        // 为员工状态文本添加点击事件
        if (employeeStatusText != null)
        {
            Button statusBtn = employeeStatusText.GetComponent<Button>();
            if (statusBtn == null)
                statusBtn = employeeStatusText.gameObject.AddComponent<Button>();
            statusBtn.onClick.AddListener(OnEmployeeStatusClicked);
        }

        if (employeeSelectionPanel != null)
            employeeSelectionPanel.SetActive(false);
    }

    public void Close()
    {
        if (!IsVisible) return;

        panel.SetActive(false);
        OnPanelClosed?.Invoke();

        if (MouseManager.Instance != null)
            MouseManager.Instance.SetCursorVisible(false);

        if (GamePauseManager.Instance != null)
            GamePauseManager.Instance.RequestResume();
    }

    public void Show(BuildingDataSO data, BuildingInstance buildingInstance)
    {
        if (data == null || buildingInstance == null) return;

        // 强制先关闭，避免重复打开
        Close();

        currentBuilding = buildingInstance;

        if (MouseManager.Instance != null)
            MouseManager.Instance.SetCursorVisible(true);

        if (GamePauseManager.Instance != null)
            GamePauseManager.Instance.RequestPause();

        // 填充UI
        nameText.text = data.buildingName;

        if (displayImage != null)
        {
            displayImage.gameObject.SetActive(data.displayImage != null);
            if (data.displayImage != null) displayImage.sprite = data.displayImage;
        }

        if (descriptionText != null)
            descriptionText.text = data.description;

        // 每月收益
        string monthly = "无";
        if (data.monthlySilver > 0) monthly += $"银两+{data.monthlySilver} ";
        if (data.monthlyWood > 0) monthly += $"木材+{data.monthlyWood} ";
        if (data.monthlyStone > 0) monthly += $"砖石+{data.monthlyStone} ";
        if (incomeText != null) incomeText.text = $"每月收益：{monthly.Trim()}";

        // 立即收益
        string immediate = "无";
        if (data.incomeHappiness > 0) immediate += $"幸福度+{data.incomeHappiness} ";
        if (data.populationCapIncrease > 0) immediate += $"人口上限+{data.populationCapIncrease} ";
        if (immediateText != null) immediateText.text = $"立即收益：{immediate.Trim()}";

        RefreshEmployeeStatus();
        panel.SetActive(true);
    }

    private void RefreshEmployeeStatus()
    {
        if (employeeStatusText == null || currentBuilding == null) return;

        if (currentBuilding.assignedEmployeeUIDs.Count == 0)
        {
            employeeStatusText.text = "员工：无";
            return;
        }

        string uid = currentBuilding.assignedEmployeeUIDs[0];
        EmployeeData emp = GameManager.Instance.GetEmployeeByUID(uid);
        employeeStatusText.text = emp != null ? $"员工：{emp.employeeName}" : "员工：未知";
    }

    private void OnEmployeeStatusClicked()
    {
        if (currentBuilding == null || employeeSelectionPanel == null) return;

        // 清空旧按钮
        foreach (Transform child in employeeSelectionPanel.transform)
            Destroy(child.gameObject);

        EmployeeJobType required = currentBuilding.data.requiredEmployeeType;
        List<EmployeeData> idleList = GameManager.Instance.GetIdleEmployeesByJobType(required);

        foreach (var emp in idleList)
        {
            GameObject btn = new GameObject("EmpBtn", typeof(Button));
            btn.transform.SetParent(employeeSelectionPanel.transform);

            TextMeshProUGUI tmp = btn.AddComponent<TextMeshProUGUI>();
            tmp.text = emp.employeeName;
            tmp.fontSize = 24;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            if (employeeButtonFont != null) tmp.font = employeeButtonFont;

            btn.GetComponent<Button>().onClick.AddListener(() => SelectEmployee(emp.uid));
        }

        employeeSelectionPanel.SetActive(true);
    }

    private void SelectEmployee(string uid)
    {
        if (currentBuilding == null) return;

        foreach (string id in currentBuilding.assignedEmployeeUIDs.ToArray())
            BuildingManager.Instance.RemoveEmployeeFromBuilding(id, currentBuilding);

        BuildingManager.Instance.AssignEmployeeToBuilding(uid, currentBuilding);
        RefreshEmployeeStatus();
        employeeSelectionPanel.SetActive(false);
    }
}