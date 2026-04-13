using UnityEngine;
using TMPro;

public class BuildingInteraction : MonoBehaviour
{
    [Header("建筑数据（由BuildingManager自动赋值）")]
    public BuildingDataSO buildingData;

    [Header("交互提示UI")]
    public GameObject interactPrompt;
    public TextMeshProUGUI promptText;

    private bool playerInRange = false;

    private void Start()
    {
        if (interactPrompt != null)
            interactPrompt.SetActive(false);

        if (BuildingInfoPanel.Instance != null)
            BuildingInfoPanel.Instance.OnPanelClosed += OnPanelClosedHandler;
    }

    private void OnDestroy()
    {
        if (BuildingInfoPanel.Instance != null)
            BuildingInfoPanel.Instance.OnPanelClosed -= OnPanelClosedHandler;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            UpdatePromptVisibility();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            interactPrompt.SetActive(false);
        }
    }

    private void Update()
    {
        // 只有玩家在范围内，才响应 F 键
        if (!playerInRange) return;

        if (Input.GetKeyDown(KeyCode.F))
        {
            HandleInteraction();
        }
    }

    private void HandleInteraction()
    {
        if (BuildingInfoPanel.Instance == null) return;

        // 面板已打开 → 关闭
        if (BuildingInfoPanel.Instance.IsVisible)
        {
            BuildingInfoPanel.Instance.Close();
        }
        // 面板关闭 → 打开当前建筑
        else
        {
            OpenThisBuildingPanel();
        }
    }

    private void OpenThisBuildingPanel()
    {
        if (buildingData == null)
        {
            Debug.LogWarning($"{gameObject.name} 缺少建筑数据");
            return;
        }

        BuildingInstance instance = GetComponent<BuildingInstance>();
        if (instance == null)
        {
            Debug.LogWarning($"{gameObject.name} 缺少 BuildingInstance");
            return;
        }

        BuildingInfoPanel.Instance.Show(buildingData, instance);
        HidePrompt();
    }

    private void UpdatePromptVisibility()
    {
        if (interactPrompt == null) return;

        bool shouldShow = playerInRange && !BuildingInfoPanel.Instance.IsVisible;
        interactPrompt.SetActive(shouldShow);

        if (shouldShow && promptText != null)
        {
            promptText.text = "按 F 查看信息";
        }
    }

    private void HidePrompt()
    {
        if (interactPrompt != null)
            interactPrompt.SetActive(false);
    }

    private void OnPanelClosedHandler()
    {
        UpdatePromptVisibility();
    }
}