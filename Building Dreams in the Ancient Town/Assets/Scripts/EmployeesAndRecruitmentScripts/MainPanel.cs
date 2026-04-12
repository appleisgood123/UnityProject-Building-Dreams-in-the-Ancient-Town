using UnityEngine;
using UnityEngine.UI;

public class MainPanel : MonoBehaviour
{
    public GameObject recruitPanel;
    public GameObject employeePanel;
    public GameObject techTreePanel;      // 科技树面板（拖拽赋值）

    private void Awake()
    {
        Transform recruitBtn = transform.Find("RecruitBtn");
        Transform employeeBtn = transform.Find("EmployeeBtn");

        if (recruitBtn != null) recruitBtn.GetComponent<Button>().onClick.AddListener(OnClickRecruit);
        if (employeeBtn != null) employeeBtn.GetComponent<Button>().onClick.AddListener(OnClickEmployee);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Y)) TogglePanel(recruitPanel);
        else if (Input.GetKeyDown(KeyCode.U)) TogglePanel(employeePanel);
        else if (Input.GetKeyDown(KeyCode.I)) ToggleTechTree();
    }

    private void TogglePanel(GameObject targetPanel)
    {
        if (targetPanel.activeSelf)
        {
            targetPanel.SetActive(false);
            CheckAndResumeGame();
        }
        else
        {
            CloseAllPanels();
            targetPanel.SetActive(true);
            PauseGameAndShowCursor();
        }
    }

    public void ToggleTechTree()  // 改为 public，方便外部调用（如科技树内部关闭按钮）
    {
        if (techTreePanel == null)
        {
            UnityEngine.Debug.LogError("techTreePanel 未赋值！");
            return;
        }
        if (techTreePanel.activeSelf)
        {
            techTreePanel.SetActive(false);
            CheckAndResumeGame();
        }
        else
        {
            CloseAllPanels();
            techTreePanel.SetActive(true);
            PauseGameAndShowCursor();
        }
    }

    private void CloseAllPanels()
    {
        recruitPanel.SetActive(false);
        employeePanel.SetActive(false);
        if (techTreePanel != null) techTreePanel.SetActive(false);
    }

    private bool IsAnyPanelOpen()
    {
        return recruitPanel.activeSelf || employeePanel.activeSelf || (techTreePanel != null && techTreePanel.activeSelf);
    }

    private void CheckAndResumeGame()
    {
        if (!IsAnyPanelOpen())
        {
            if (GamePauseManager.Instance != null)
            {
                GamePauseManager.Instance.RequestResume();
                // 安全保护：如果 Time.timeScale 仍为 0，强制恢复
                if (Time.timeScale == 0f)
                {
                    UnityEngine.Debug.LogWarning("强制恢复游戏（timeScale 仍为 0）");
                    GamePauseManager.Instance.ForceResume();
                }
            }
            if (MouseManager.Instance != null)
                MouseManager.Instance.SetCursorVisible(false);
        }
    }

    private void PauseGameAndShowCursor()
    {
        if (GamePauseManager.Instance != null)
            GamePauseManager.Instance.RequestPause();
        if (MouseManager.Instance != null)
            MouseManager.Instance.SetCursorVisible(true);
    }

    private void OnClickRecruit() => TogglePanel(recruitPanel);
    private void OnClickEmployee() => TogglePanel(employeePanel);
}