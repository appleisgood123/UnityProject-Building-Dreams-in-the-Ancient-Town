using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EmployeeCell : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Image UIAvatar;
    [SerializeField] private TextMeshProUGUI UINameText;
    [SerializeField] private TextMeshProUGUI UIJobText;
    [SerializeField] private TextMeshProUGUI UIWorkPlaceText;
    [SerializeField] private GameObject UISelectMark;

    private EmployeeData employeeData;
    private EmployeePanel uiParent;

    private void Awake()
    {
        // 自动查找备用（如果未手动拖拽）
        if (UIAvatar == null) UIAvatar = transform.Find("Top/EmployeeBackground/Avatar")?.GetComponent<Image>();
        if (UINameText == null) UINameText = transform.Find("Bottom/bg/NameText")?.GetComponent<TextMeshProUGUI>();
        if (UIJobText == null) UIJobText = transform.Find("Bottom/bg/JobText")?.GetComponent<TextMeshProUGUI>();
        if (UIWorkPlaceText == null) UIWorkPlaceText = transform.Find("Bottom/bg/WorkPlaceText")?.GetComponent<TextMeshProUGUI>();
        if (UISelectMark == null) UISelectMark = transform.Find("SelectMark")?.gameObject;
        if (UISelectMark != null) UISelectMark.SetActive(false);
    }

    public void Refresh(EmployeeData data, EmployeePanel parent)
    {
        employeeData = data;
        uiParent = parent;

        if (UINameText != null) UINameText.text = data.employeeName;
        if (UIAvatar != null && data.avatarSprite != null) UIAvatar.sprite = data.avatarSprite;

        if (UIJobText != null)
        {
            string jobName = GetJobName(data.jobType);
            UIJobText.text = $"职业：{jobName}";
        }

        if (UIWorkPlaceText != null)
        {
            if (string.IsNullOrEmpty(data.assignedBuildingUID))
                UIWorkPlaceText.text = "空闲中";
            else
            {
                BuildingInstance building = BuildingManager.Instance.GetBuildingInstanceByUID(data.assignedBuildingUID);
                UIWorkPlaceText.text = building != null && building.data != null ? $"工作：{building.data.buildingName}" : "工作：未知";
            }
        }

        RefreshSelectState();
    }

    private string GetJobName(EmployeeJobType jobType)
    {
        switch (jobType)
        {
            case EmployeeJobType.Woodcutter: return "樵夫";
            case EmployeeJobType.Stonecutter: return "石匠";
            case EmployeeJobType.Merchant: return "商贩";
            case EmployeeJobType.Administrator: return "管事";
            default: return "未知";
        }
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