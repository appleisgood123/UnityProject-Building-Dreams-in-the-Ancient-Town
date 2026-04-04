using UnityEngine;
using UnityEngine.UI;

public class TechNodeUI : MonoBehaviour
{
    public TechNodeData techData;
    public Image background;
    public Image icon;
    public Button button;

    private void Awake()
    {
        if (button == null) button = GetComponent<Button>();
        if (button != null) button.onClick.AddListener(OnClick);
    }

    private void Start()
    {
        Refresh();
        if (TechManager.Instance != null)
            TechManager.Instance.OnTechUnlocked += OnTechUnlocked;
    }

    private void OnDestroy()
    {
        if (TechManager.Instance != null)
            TechManager.Instance.OnTechUnlocked -= OnTechUnlocked;
    }

    private void OnTechUnlocked(TechNodeData tech)
    {
        Refresh();
    }

    public void Refresh()
    {
        if (techData == null) return;

        if (icon != null && techData.icon != null)
            icon.sprite = techData.icon;

        if (background == null) return;

        if (TechManager.Instance.IsUnlocked(techData))
        {
            background.color = Color.green;
            if (button != null) button.interactable = false;   // 已解锁不可点击
        }
        else if (TechManager.Instance.CanUnlock(techData))
        {
            background.color = Color.white;
            if (button != null) button.interactable = true;
        }
        else
        {
            background.color = Color.gray;
            if (button != null) button.interactable = true;    // 条件不足仍可点击查看详情
        }
    }

    private void OnClick()
    {
        if (techData == null) return;
        if (button != null && !button.interactable) return;    // 已解锁不响应

        TechDetailPanel detailPanel = FindObjectOfType<TechDetailPanel>(true);
        if (detailPanel != null)
            detailPanel.Show(techData, this);
        else
            Debug.LogError("未找到 TechDetailPanel，请确保其在场景中。");
    }
}