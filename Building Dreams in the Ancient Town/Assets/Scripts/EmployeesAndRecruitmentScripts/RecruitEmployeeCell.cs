using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RecruitEmployeeCell : MonoBehaviour
{
    [SerializeField] private Image UIAvatar;
    [SerializeField] private TextMeshProUGUI UINameText;
    [SerializeField] private TextMeshProUGUI UICostText;
    [SerializeField] private TextMeshProUGUI UIJobText;   // 暴露给 Inspector

    private void Awake()
    {
        // 如果未手动拖拽，尝试自动查找（备用）
        if (UIAvatar == null) UIAvatar = transform.Find("Avatar")?.GetComponent<Image>();
        if (UINameText == null) UINameText = transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();
        if (UICostText == null) UICostText = transform.Find("CostText")?.GetComponent<TextMeshProUGUI>();
        if (UIJobText == null) UIJobText = transform.Find("JobText")?.GetComponent<TextMeshProUGUI>();
    }

    public void Refresh(EmployeeData data)
    {
        if (UINameText != null) UINameText.text = data.employeeName;
        if (UICostText != null) UICostText.text = "招募花费: " + data.cost;
        if (UIAvatar != null && data.avatarSprite != null) UIAvatar.sprite = data.avatarSprite;
        if (UIJobText != null)
        {
            string jobName = GetJobName(data.jobType);
            UIJobText.text = $"职业：{jobName}";
        }
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
}