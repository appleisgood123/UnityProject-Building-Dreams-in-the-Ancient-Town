using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuildingInfoPanel : MonoBehaviour
{
    public static BuildingInfoPanel Instance;

    public GameObject panel;
    public TextMeshProUGUI nameText;
    public Image displayImage;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI incomeText;
    public TextMeshProUGUI immediateText;
    public Button closeButton;

    public System.Action OnPanelClosed;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        if (panel != null)
            panel.SetActive(false);

        if (closeButton != null)
            closeButton.onClick.AddListener(OnCloseButtonClicked);
    }

    private void OnCloseButtonClicked()
    {
        Close();
    }

    public void Close()
    {
        if (!panel.activeSelf) return;

        panel.SetActive(false);
        OnPanelClosed?.Invoke();

        if (MouseManager.Instance != null)
            MouseManager.Instance.SetCursorVisible(false);
        if (GamePauseManager.Instance != null)
            GamePauseManager.Instance.RequestResume();
    }

    public void Show(BuildingDataSO data)
    {
        if (data == null) return;

        if (MouseManager.Instance != null)
            MouseManager.Instance.SetCursorVisible(true);
        if (GamePauseManager.Instance != null)
            GamePauseManager.Instance.RequestPause();

        if (nameText != null)
            nameText.text = data.buildingName;

        // 处理展示图片
        if (displayImage != null)
        {
            displayImage.gameObject.SetActive(true); // 先激活，确保上次隐藏的不影响
            if (data.displayImage != null)
            {
                displayImage.sprite = data.displayImage;
                Debug.Log($"设置图片：{data.displayImage.name}");
            }
            else
            {
                displayImage.gameObject.SetActive(false); // 无图片则隐藏
                Debug.LogWarning($"建筑 {data.buildingName} 未设置 displayImage");
            }
        }

        if (descriptionText != null)
            descriptionText.text = data.description;

        // 收益显示
        string monthly = "";
        if (data.monthlySilver > 0) monthly += $"银两+{data.monthlySilver} ";
        if (data.monthlyWood > 0) monthly += $"木材+{data.monthlyWood} ";
        if (data.monthlyStone > 0) monthly += $"砖石+{data.monthlyStone} ";
        if (string.IsNullOrEmpty(monthly)) monthly = "无";
        if (incomeText != null)
            incomeText.text = $"每月收益：{monthly}";

        string immediate = "";
        if (data.incomeHappiness > 0) immediate += $"幸福度+{data.incomeHappiness} ";
        if (data.populationCapIncrease > 0) immediate += $"人口上限+{data.populationCapIncrease} ";
        if (string.IsNullOrEmpty(immediate)) immediate = "无";
        if (immediateText != null)
            immediateText.text = $"立即收益：{immediate}";

        panel.SetActive(true);
    }
}