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
    public Button closeSelectionButton;
    public TMP_FontAsset employeeButtonFont;

    [Header("拆除")]
    public Button demolishButton;      // 新增拆除按钮（在 Inspector 中拖拽）

    private BuildingInstance currentBuilding;
    public System.Action OnPanelClosed;
    public BuildingInstance CurrentBuilding => currentBuilding;

    public bool IsVisible => panel != null && panel.activeSelf;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        if (panel == null)
            panel = transform.Find("PanelContent")?.gameObject;

        if (panel != null) panel.SetActive(false);
        else Debug.LogError("未找到 PanelContent");

        if (closeButton != null)
            closeButton.onClick.AddListener(Close);

        if (employeeStatusText != null)
        {
            Button statusBtn = employeeStatusText.GetComponent<Button>();
            if (statusBtn == null)
                statusBtn = employeeStatusText.gameObject.AddComponent<Button>();
            statusBtn.onClick.AddListener(OnEmployeeStatusClicked);
        }

        if (closeSelectionButton != null)
            closeSelectionButton.onClick.AddListener(CloseEmployeeSelection);

        // 新增拆除按钮监听
        if (demolishButton != null)
            demolishButton.onClick.AddListener(DemolishBuilding);

        if (employeeSelectionPanel != null)
            employeeSelectionPanel.SetActive(false);
        if (closeSelectionButton != null)
            closeSelectionButton.gameObject.SetActive(false);
    }

    private void CloseEmployeeSelection()
    {
        if (employeeSelectionPanel != null)
            employeeSelectionPanel.SetActive(false);
        if (closeSelectionButton != null)
            closeSelectionButton.gameObject.SetActive(false);
    }

    public void Close()
    {
        if (!IsVisible) return;
        PlaySFX("关闭点击");
        panel.SetActive(false);
        OnPanelClosed?.Invoke();
        CloseEmployeeSelection();

        if (MouseManager.Instance != null) MouseManager.Instance.SetCursorVisible(false);
        if (GamePauseManager.Instance != null) GamePauseManager.Instance.RequestResume();
    }

    public void Show(BuildingDataSO data, BuildingInstance buildingInstance)
    {
        if (data == null || buildingInstance == null) return;
        Close();
        currentBuilding = buildingInstance;

        if (MouseManager.Instance != null) MouseManager.Instance.SetCursorVisible(true);
        if (GamePauseManager.Instance != null) GamePauseManager.Instance.RequestPause();

        nameText.text = data.buildingName;
        if (displayImage != null)
        {
            displayImage.gameObject.SetActive(data.displayImage != null);
            if (data.displayImage != null) displayImage.sprite = data.displayImage;
        }
        if (descriptionText != null) descriptionText.text = data.description;

        string monthly = "无";
        if (data.monthlySilver > 0) monthly += $"银两+{data.monthlySilver} ";
        if (data.monthlyWood > 0) monthly += $"木材+{data.monthlyWood} ";
        if (data.monthlyStone > 0) monthly += $"砖石+{data.monthlyStone} ";
        if (incomeText != null) incomeText.text = $"每月收益：{monthly.Trim()}";

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
            employeeStatusText.text = "员工：无";
        else
        {
            string uid = currentBuilding.assignedEmployeeUIDs[0];
            EmployeeData emp = GameManager.Instance.GetEmployeeByUID(uid);
            employeeStatusText.text = emp != null ? $"员工：{emp.employeeName}" : "员工：未知";
        }
    }

    private void OnEmployeeStatusClicked()
    {
        if (currentBuilding == null || employeeSelectionPanel == null) return;

        foreach (Transform child in employeeSelectionPanel.transform)
            Destroy(child.gameObject);

        EmployeeJobType required = currentBuilding.data.requiredEmployeeType;
        List<EmployeeData> idleList = GameManager.Instance.GetIdleEmployeesByJobType(required);

        foreach (var emp in idleList)
        {
            // 创建按钮容器
            GameObject btnObj = new GameObject("EmpBtn", typeof(RectTransform), typeof(Button), typeof(Image));
            btnObj.transform.SetParent(employeeSelectionPanel.transform);
            btnObj.transform.localScale = Vector3.one;

            RectTransform btnRect = btnObj.GetComponent<RectTransform>();
            btnRect.sizeDelta = new Vector2(100, 120);

            // 设置头像图片
            Image btnImage = btnObj.GetComponent<Image>();
            if (emp.avatarSprite != null)
                btnImage.sprite = emp.avatarSprite;
            else
                btnImage.color = new Color(0.8f, 0.8f, 0.8f, 1f);

            // 头像下方显示名字
            GameObject nameObj = new GameObject("Name", typeof(RectTransform), typeof(TextMeshProUGUI));
            nameObj.transform.SetParent(btnObj.transform, false);
            nameObj.transform.localScale = Vector3.one;

            RectTransform nameRect = nameObj.GetComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0, 0);
            nameRect.anchorMax = new Vector2(1, 0);
            nameRect.pivot = new Vector2(0.5f, 1f);
            nameRect.sizeDelta = new Vector2(0, 28);
            nameRect.anchoredPosition = new Vector2(0, -6);

            TextMeshProUGUI nameText = nameObj.GetComponent<TextMeshProUGUI>();
            nameText.text = emp.employeeName;
            nameText.fontSize = 16;
            nameText.alignment = TextAlignmentOptions.Center;
            nameText.color = Color.black;
            if (employeeButtonFont != null) nameText.font = employeeButtonFont;

            btnObj.GetComponent<Button>().onClick.AddListener(() => SelectEmployee(emp.uid));
        }

        employeeSelectionPanel.SetActive(true);
        if (closeSelectionButton != null)
            closeSelectionButton.gameObject.SetActive(true);
    }

    private void SelectEmployee(string uid)
    {
        if (currentBuilding == null) return;
        PlaySFX("确认点击");
        foreach (string id in currentBuilding.assignedEmployeeUIDs.ToArray())
            BuildingManager.Instance.RemoveEmployeeFromBuilding(id, currentBuilding);
        BuildingManager.Instance.AssignEmployeeToBuilding(uid, currentBuilding);
        RefreshEmployeeStatus();
        CloseEmployeeSelection();
    }

    // 新增拆除方法
    private void DemolishBuilding()
    {
        if (currentBuilding == null) return;
        BuildingManager.Instance.DemolishBuilding(currentBuilding);
        Close(); // 拆除后关闭面板
    }

    private void PlaySFX(string clipName)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(clipName);
    }
}