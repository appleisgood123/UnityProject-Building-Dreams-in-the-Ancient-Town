using UnityEngine;

public class TechTreeToggle : MonoBehaviour
{
    public GameObject techTreePanel;            // 科技树面板对象
    private TechDetailPanel techDetailPanel;

    private void Start()
    {
        if (techTreePanel != null)
            techDetailPanel = techTreePanel.GetComponentInChildren<TechDetailPanel>(true);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            ToggleTechTree();
        }
    }

    public void ToggleTechTree()
    {
        if (techTreePanel == null) return;
        SetPanelState(!techTreePanel.activeSelf);
    }

    public void ClosePanel()
    {
        SetPanelState(false);
    }

    private void SetPanelState(bool isOpen)
    {
        techTreePanel.SetActive(isOpen);

        if (isOpen)
        {
            if (GamePauseManager.Instance != null)
                GamePauseManager.Instance.RequestPause();
            if (MouseManager.Instance != null)
                MouseManager.Instance.SetCursorVisible(true);
        }
        else
        {
            if (GamePauseManager.Instance != null)
                GamePauseManager.Instance.RequestResume();
            if (MouseManager.Instance != null)
                MouseManager.Instance.SetCursorVisible(false);

            // 关闭科技树时同时隐藏详情面板
            if (techDetailPanel != null)
                techDetailPanel.Hide();
        }
    }
}