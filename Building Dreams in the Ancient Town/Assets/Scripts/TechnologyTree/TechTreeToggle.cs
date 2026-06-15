using UnityEngine;

public class TechTreeToggle : MonoBehaviour
{
    public GameObject techTreePanel;            // �Ƽ���������
    private TechDetailPanel techDetailPanel;

    private void Start()
    {
        if (techTreePanel != null)
            techDetailPanel = techTreePanel.GetComponentInChildren<TechDetailPanel>(true);
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

            // �رտƼ���ʱͬʱ�����������
            if (techDetailPanel != null)
                techDetailPanel.Hide();
        }
    }
}