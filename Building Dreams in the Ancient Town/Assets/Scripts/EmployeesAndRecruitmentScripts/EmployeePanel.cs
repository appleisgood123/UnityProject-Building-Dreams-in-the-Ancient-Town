using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum EmployeePanelMode
{
    Normal,
    Fire
}

public class EmployeePanel : MonoBehaviour
{
    private ScrollRect UIScrollView;
    private Button UIFireBtn;
    private Button UIConfirmFireBtn;
    private Button UICancelFireBtn;
    private Button UICloseBtn;

    public GameObject employeeCellPrefab;

    public EmployeePanelMode curMode = EmployeePanelMode.Normal;
    public List<string> selectedUidList = new List<string>();

    private void Awake()
    {
        InitUI();
    }

    private void OnEnable()
    {
        RefreshList();
        RefreshModeUI();
    }

    private void InitUI()
    {
        UIScrollView = transform.Find("Center/Scroll View").GetComponent<ScrollRect>();
        UIFireBtn = transform.Find("Bottom/BottomMenus/FireBtn").GetComponent<Button>();
        UIConfirmFireBtn = transform.Find("Bottom/DelectPanel/ConfirmFireBtn").GetComponent<Button>();
        UICancelFireBtn = transform.Find("Bottom/DelectPanel/CancelFireBtn").GetComponent<Button>();
        UICloseBtn = transform.Find("RightTop/CloseBtn").GetComponent<Button>();

        UIFireBtn.onClick.AddListener(OnClickFireMode);
        UIConfirmFireBtn.onClick.AddListener(OnClickConfirmFire);
        UICancelFireBtn.onClick.AddListener(OnClickCancelFire);
        UICloseBtn.onClick.AddListener(OnClickClose);
    }

    public void RefreshList()
    {
        RectTransform content = UIScrollView.content;

        for (int i = content.childCount - 1; i >= 0; i--)
        {
            Destroy(content.GetChild(i).gameObject);
        }

        List<EmployeeData> list = GameManager.Instance.GetEmployeeList();
        foreach (EmployeeData data in list)
        {
            Transform cellTran = Instantiate(employeeCellPrefab.transform, content);
            EmployeeCell cell = cellTran.GetComponent<EmployeeCell>();
            cell.Refresh(data, this);
        }
    }

    public void ToggleSelectUid(string uid)
    {
        if (selectedUidList.Contains(uid))
            selectedUidList.Remove(uid);
        else
            selectedUidList.Add(uid);
    }

    public bool IsSelected(string uid) => selectedUidList.Contains(uid);

    private void RefreshModeUI()
    {
        bool isFireMode = curMode == EmployeePanelMode.Fire;
        UIFireBtn.gameObject.SetActive(!isFireMode);
        UIConfirmFireBtn.gameObject.SetActive(isFireMode);
        UICancelFireBtn.gameObject.SetActive(isFireMode);
    }

    private void OnClickFireMode()
    {
        curMode = EmployeePanelMode.Fire;
        selectedUidList.Clear();
        RefreshModeUI();
        RefreshList();
    }

    private void OnClickConfirmFire()
    {
        GameManager.Instance.FireEmployees(selectedUidList);
        selectedUidList.Clear();
        curMode = EmployeePanelMode.Normal;
        RefreshModeUI();
        RefreshList();
    }

    private void OnClickCancelFire()
    {
        selectedUidList.Clear();
        curMode = EmployeePanelMode.Normal;
        RefreshModeUI();
        RefreshList();
    }

    private void OnClickClose()
    {
        gameObject.SetActive(false);
        if (GamePauseManager.Instance != null)
            GamePauseManager.Instance.RequestResume();
        if (MouseManager.Instance != null)
            MouseManager.Instance.SetCursorVisible(false);
    }
}