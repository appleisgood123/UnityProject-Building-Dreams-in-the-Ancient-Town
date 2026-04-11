using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EmployeeCell : MonoBehaviour, IPointerClickHandler
{
    private Image UIAvatar;
    private Text UINameText;
    private GameObject UISelectMark;

    private EmployeeData employeeData;
    private EmployeePanel uiParent;

    private void Awake()
    {
        InitUI();
    }

    private void InitUI()
    {
        UIAvatar = transform.Find("Top/EmployeeBackground/Avatar")?.GetComponent<Image>();
        UINameText = transform.Find("Bottom/bg/NameText")?.GetComponent<Text>();
        UISelectMark = transform.Find("SelectMark")?.gameObject;

        if (UISelectMark != null) UISelectMark.SetActive(false);
    }

    public void Refresh(EmployeeData data, EmployeePanel parent)
    {
        employeeData = data;
        uiParent = parent;

        if (UINameText != null) UINameText.text = data.employeeName;
        if (UIAvatar != null && data.avatarSprite != null)
            UIAvatar.sprite = data.avatarSprite;

        RefreshSelectState();
    }

    public void RefreshSelectState()
    {
        if (UISelectMark == null) return;
        if (uiParent.curMode == EmployeePanelMode.Fire)
            UISelectMark.SetActive(uiParent.IsSelected(employeeData.uid));
        else
            UISelectMark.SetActive(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (uiParent.curMode != EmployeePanelMode.Fire) return;
        uiParent.ToggleSelectUid(employeeData.uid);
        RefreshSelectState();
    }
}