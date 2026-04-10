using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RecruitEmployeeCell : MonoBehaviour
{
    private Image UIAvatar;
    private TextMeshProUGUI UINameText;
    private TextMeshProUGUI UICostText;
    private TextMeshProUGUI UIJobText;

    private void Awake()
    {
        InitUI();
    }

    private void InitUI()
    {
        Transform avatarTrans = transform.Find("Avatar");
        if (avatarTrans != null)
            UIAvatar = avatarTrans.GetComponent<Image>();

        Transform nameTrans = transform.Find("NameText");
        if (nameTrans != null)
            UINameText = nameTrans.GetComponent<TextMeshProUGUI>();

        Transform costTrans = transform.Find("CostText");
        if (costTrans != null)
            UICostText = costTrans.GetComponent<TextMeshProUGUI>();

        Transform jobTrans = transform.Find("JobText");
        if (jobTrans != null)
            UIJobText = jobTrans.GetComponent<TextMeshProUGUI>();
    }

    public void Refresh(EmployeeData data)
    {
        if (data == null) return;

        if (UINameText != null)
            UINameText.text = data.employeeName;
        else
            Debug.LogWarning("RecruitEmployeeCell: UINameText 未找到");

        if (UICostText != null)
            UICostText.text = "招募花费: " + data.cost;
        else
            Debug.LogWarning("RecruitEmployeeCell: UICostText 未找到");

        if (UIAvatar != null && data.avatarSprite != null)
            UIAvatar.sprite = data.avatarSprite;

        if (UIJobText != null)
        {
            string jobName = GetJobName(data.jobType);
            UIJobText.text = $"职业：{jobName}";
        }
        else
        {
            Debug.LogWarning("RecruitEmployeeCell: UIJobText 未找到");
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