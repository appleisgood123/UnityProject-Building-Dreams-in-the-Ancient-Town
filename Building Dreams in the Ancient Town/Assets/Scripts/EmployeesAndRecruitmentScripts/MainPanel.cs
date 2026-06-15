using UnityEngine;
using UnityEngine.UI;

public class MainPanel : MonoBehaviour
{
    public GameObject recruitPanel;
    public GameObject employeePanel;
    public GameObject techTreePanel;      // �Ƽ�����壨��ק��ֵ��

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

    public void ToggleTechTree()  // ��Ϊ public�������ⲿ���ã���Ƽ����ڲ��رհ�ť��
    {
        if (techTreePanel == null)
        {
            UnityEngine.Debug.LogError("techTreePanel δ��ֵ��");
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
                // ��ȫ��������� Time.timeScale ��Ϊ 0��ǿ�ƻָ�
                if (Time.timeScale == 0f)
                {
                    UnityEngine.Debug.LogWarning("ǿ�ƻָ���Ϸ��timeScale ��Ϊ 0��");
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

    private void OnClickRecruit()
    {
        PlayClick();
        TogglePanel(recruitPanel);
    }

    private void OnClickEmployee()
    {
        PlayClick();
        TogglePanel(employeePanel);
    }

    private void PlayClick()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX("普通点击");
    }
}