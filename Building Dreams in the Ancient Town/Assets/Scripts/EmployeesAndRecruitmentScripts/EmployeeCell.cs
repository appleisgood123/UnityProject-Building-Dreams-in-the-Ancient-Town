using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EmployeeCell : MonoBehaviour, IPointerClickHandler
{
    private Image UIAvatar;
    private TextMeshProUGUI UINameText;          // 改为 TMP
    private TextMeshProUGUI UIWorkPlaceText;     // 新增工作地
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
        UINameText = transform.Find("Bottom/bg/NameText")?.GetComponent<TextMeshProUGUI>();
        UIWorkPlaceText = transform.Find("Bottom/bg/WorkPlaceText")?.GetComponent<TextMeshProUGUI>();
        UISelectMark = transform.Find("SelectMark")?.gameObject;

        if (UISelectMark != null) UISelectMark.SetActive(false);
    }

    public void Refresh(EmployeeData data, EmployeePanel parent)
    {
        employeeData = data;
        uiParent = parent;

        if (UINameText != null)
            UINameText.text = data.employeeName;
        else
            Debug.LogWarning("EmployeeCell: UINameText 未找到");

        if (UIAvatar != null && data.avatarSprite != null)
            UIAvatar.sprite = data.avatarSprite;

        // 显示工作地
        if (UIWorkPlaceText != null)
        {
            if (string.IsNullOrEmpty(data.assignedBuildingUID))
            {
                UIWorkPlaceText.text = "空闲中";
            }
            else
            {
                BuildingInstance building = BuildingManager.Instance.GetBuildingInstanceByUID(data.assignedBuildingUID);
                if (building != null && building.data != null)
                    UIWorkPlaceText.text = building.data.buildingName;
                else
                    UIWorkPlaceText.text = "未知建筑";
            }
        }

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