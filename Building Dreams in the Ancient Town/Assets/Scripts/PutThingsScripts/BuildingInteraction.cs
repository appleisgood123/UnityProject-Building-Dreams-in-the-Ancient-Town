using UnityEngine;
using TMPro;

public class BuildingInteraction : MonoBehaviour
{
    public BuildingDataSO buildingData;
    public GameObject interactPrompt;
    public TextMeshProUGUI promptText;

    private bool playerInRange = false;

    private void Start()
    {
        if (interactPrompt != null)
            interactPrompt.SetActive(false);

        if (BuildingInfoPanel.Instance != null)
            BuildingInfoPanel.Instance.OnPanelClosed += OnInfoPanelClosed;
    }

    private void OnDestroy()
    {
        if (BuildingInfoPanel.Instance != null)
            BuildingInfoPanel.Instance.OnPanelClosed -= OnInfoPanelClosed;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            InteractionManager.Instance.SetCurrentInteractable(this);
            UpdatePrompt();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            HidePrompt();
            InteractionManager.Instance.ClearCurrentInteractable(this);
        }
    }

    private void UpdatePrompt()
    {
        if (BuildingInfoPanel.Instance != null && BuildingInfoPanel.Instance.panel.activeSelf)
            return; // 面板已打开，不显示提示

        if (playerInRange && InteractionManager.Instance.currentInteractable == this)
        {
            interactPrompt.SetActive(true);
            if (promptText != null) promptText.text = "按 F 查看信息";
        }
        else
        {
            interactPrompt.SetActive(false);
        }
    }

    public void HidePrompt()
    {
        if (interactPrompt != null)
            interactPrompt.SetActive(false);
    }

    public void Interact()
    {
        if (!playerInRange) return;

        if (BuildingInfoPanel.Instance != null && BuildingInfoPanel.Instance.panel.activeSelf)
        {
            // 如果面板已打开，则关闭
            BuildingInfoPanel.Instance.Close();
        }
        else
        {
            // 打开面板
            if (buildingData == null)
            {
                Debug.LogWarning("buildingData is null");
                return;
            }
            BuildingInfoPanel.Instance.Show(buildingData);
            HidePrompt(); // 打开面板后隐藏提示
        }
    }

    private void OnInfoPanelClosed()
    {
        // 面板关闭时，如果玩家仍在范围内，重新显示提示
        if (playerInRange && InteractionManager.Instance.currentInteractable == this)
        {
            interactPrompt.SetActive(true);
            if (promptText != null) promptText.text = "按 F 查看信息";
        }
    }

    // 此方法现在由 Update 调用改为由管理器调用，但为了保持灵活性，也可以保留 Update 但调用管理器
    private void Update()
    {
        // 如果当前建筑是玩家所在且是当前交互目标，按F键由管理器统一处理
        // 这样避免重复
        if (playerInRange && InteractionManager.Instance.currentInteractable == this)
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                InteractionManager.Instance.TryInteract();
            }
        }
    }
}