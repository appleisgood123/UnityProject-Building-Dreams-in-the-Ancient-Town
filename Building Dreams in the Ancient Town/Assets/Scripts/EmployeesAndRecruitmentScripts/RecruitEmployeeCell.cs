using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RecruitEmployeeCell : MonoBehaviour
{
    private Image UIAvatar;
    private TextMeshProUGUI UINameText;
    private TextMeshProUGUI UICostText;

    private void Awake()
    {
        InitUI();
    }

    private void InitUI()
    {
        Transform avatarTrans = transform.Find("Avatar");
        if (avatarTrans != null)
            UIAvatar = avatarTrans.GetComponent<Image>();
        else
            Debug.LogError("RecruitEmployeeCell: 找不到 Avatar");

        Transform nameTrans = transform.Find("NameText");
        if (nameTrans != null)
            UINameText = nameTrans.GetComponent<TextMeshProUGUI>();
        else
            Debug.LogError("RecruitEmployeeCell: 找不到 NameText");

        Transform costTrans = transform.Find("CostText");
        if (costTrans != null)
            UICostText = costTrans.GetComponent<TextMeshProUGUI>();
        else
            Debug.LogError("RecruitEmployeeCell: 找不到 CostText");
    }

    public void Refresh(EmployeeData data)
    {
        if (data == null) return;
        if (UINameText != null)
            UINameText.text = data.employeeName;
        if (UICostText != null)
            UICostText.text = "招募花费: " + data.cost;
        if (UIAvatar != null && data.avatarSprite != null)
            UIAvatar.sprite = data.avatarSprite;
    }
}