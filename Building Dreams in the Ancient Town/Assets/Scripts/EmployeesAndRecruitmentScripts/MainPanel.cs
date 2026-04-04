using UnityEngine;
using UnityEngine.UI;

public class MainPanel : MonoBehaviour
{
    public GameObject recruitPanel;
    public GameObject employeePanel;

    private void Awake()
    {
        Transform recruitBtn = transform.Find("RecruitBtn");
        Transform employeeBtn = transform.Find("EmployeeBtn");

        if (recruitBtn != null) recruitBtn.GetComponent<Button>().onClick.AddListener(OnClickRecruit);
        if (employeeBtn != null) employeeBtn.GetComponent<Button>().onClick.AddListener(OnClickEmployee);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Y))
        {
            ToggleRecruitPanel();
        }
        else if (Input.GetKeyDown(KeyCode.U))
        {
            ToggleEmployeePanel();
        }
    }

    private void ToggleRecruitPanel()
    {
        if (recruitPanel.activeSelf)
        {
            recruitPanel.SetActive(false);
            if (GamePauseManager.Instance != null)
                GamePauseManager.Instance.RequestResume();
            if (MouseManager.Instance != null)
                MouseManager.Instance.SetCursorVisible(false);
        }
        else
        {
            recruitPanel.SetActive(true);
            employeePanel.SetActive(false);
            if (GamePauseManager.Instance != null)
                GamePauseManager.Instance.RequestPause();
            if (MouseManager.Instance != null)
                MouseManager.Instance.SetCursorVisible(true);
        }
    }

    private void ToggleEmployeePanel()
    {
        if (employeePanel.activeSelf)
        {
            employeePanel.SetActive(false);
            if (GamePauseManager.Instance != null)
                GamePauseManager.Instance.RequestResume();
            if (MouseManager.Instance != null)
                MouseManager.Instance.SetCursorVisible(false);
        }
        else
        {
            employeePanel.SetActive(true);
            recruitPanel.SetActive(false);
            if (GamePauseManager.Instance != null)
                GamePauseManager.Instance.RequestPause();
            if (MouseManager.Instance != null)
                MouseManager.Instance.SetCursorVisible(true);
        }
    }

    private void OnClickRecruit()
    {
        recruitPanel.SetActive(true);
        employeePanel.SetActive(false);
        if (GamePauseManager.Instance != null)
            GamePauseManager.Instance.RequestPause();
        if (MouseManager.Instance != null)
            MouseManager.Instance.SetCursorVisible(true);
    }

    private void OnClickEmployee()
    {
        recruitPanel.SetActive(false);
        employeePanel.SetActive(true);
        if (GamePauseManager.Instance != null)
            GamePauseManager.Instance.RequestPause();
        if (MouseManager.Instance != null)
            MouseManager.Instance.SetCursorVisible(true);
    }
}