using UnityEngine;
using UnityEngine.UI;

public class RecruitPanel : MonoBehaviour
{
    private Transform UICandidateRoot;
    private Button UIRecruitBtn;
    private Button UIRefreshBtn;
    private Button UICloseBtn;

    public GameObject recruitEmployeeCellPrefab;

    private void Awake()
    {
        InitUI();
    }

    private void OnEnable()
    {
        RefreshCandidateUI();
    }

    private void InitUI()
    {
        UICandidateRoot = transform.Find("CandidateRoot");
        UIRecruitBtn = transform.Find("RecruitBtn")?.GetComponent<Button>();
        UIRefreshBtn = transform.Find("RefreshBtn")?.GetComponent<Button>();
        UICloseBtn = transform.Find("CloseBtn")?.GetComponent<Button>();

        if (UIRecruitBtn != null) UIRecruitBtn.onClick.AddListener(OnClickRecruit);
        if (UIRefreshBtn != null) UIRefreshBtn.onClick.AddListener(OnClickRefresh);
        if (UICloseBtn != null) UICloseBtn.onClick.AddListener(OnClickClose);
    }

    private void RefreshCandidateUI()
    {
        if (GameManager.Instance.GetCurrentCandidate() == null)
            GameManager.Instance.RefreshRecruitCandidate();

        if (UICandidateRoot == null) return;
        foreach (Transform child in UICandidateRoot)
            Destroy(child.gameObject);

        EmployeeData data = GameManager.Instance.GetCurrentCandidate();
        if (data == null) return;

        GameObject cellObj = Instantiate(recruitEmployeeCellPrefab, UICandidateRoot);
        RecruitEmployeeCell cell = cellObj.GetComponent<RecruitEmployeeCell>();
        if (cell != null) cell.Refresh(data);
    }

    private void OnClickRecruit()
    {
        if (GameManager.Instance.RecruitCurrentCandidate())
            RefreshCandidateUI();
        else
            Debug.LogWarning("招募失败：货币不足或没有候选员工");
    }

    private void OnClickRefresh()
    {
        GameManager.Instance.RefreshRecruitCandidate();
        RefreshCandidateUI();
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