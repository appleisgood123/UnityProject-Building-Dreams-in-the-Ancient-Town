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
    private float lastToggleTime = 0f;
    private const float TOGGLE_COOLDOWN = 0.2f; // 防止快速切换

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
            if (BuildingInfoPanel.Instance != null && !BuildingInfoPanel.Instance.contentPanel.activeSelf)
            {
                interactPrompt.SetActive(true);
                if (promptText != null) promptText.text = "按 F 查看信息";
            }
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
        if (Input.GetKeyDown(KeyCode.F) && Time.time > lastToggleTime + TOGGLE_COOLDOWN)
        {
            lastToggleTime = Time.time;
            if (BuildingInfoPanel.Instance != null)
            {
                if (BuildingInfoPanel.Instance.contentPanel.activeSelf)
                {
                    BuildingInfoPanel.Instance.Close();
                }
                else if (playerInRange)
                {
                    Interact();
                }
            }
        }
    }

    public void Interact()
    {
        if (!playerInRange) return;
        if (buildingData == null)
        {
            Debug.LogWarning($"Building {gameObject.name} has no buildingData assigned.");
            return;
        }

        BuildingInstance instance = GetComponent<BuildingInstance>();
        if (instance == null)
        {
            Debug.LogWarning($"Building {gameObject.name} has no BuildingInstance component.");
            return;
        }

        BuildingInfoPanel.Instance.Show(buildingData, instance);
        HidePrompt();
    }

    public void HidePrompt()
    {
        if (interactPrompt != null)
            interactPrompt.SetActive(false);
    }

    private void OnInfoPanelClosed()
    {
        if (playerInRange)
        {
            interactPrompt.SetActive(true);
            if (promptText != null) promptText.text = "按 F 查看信息";
        }
    }
}