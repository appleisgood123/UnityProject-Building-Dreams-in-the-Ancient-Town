using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;
    public static GameManager Instance => _instance;

    [Header("数据表引用")]
    public EmployeeTable employeeTable;

    [SerializeField] private int startCurrency = 1000;
    private int currentCurrency;
    public int CurrentCurrency => currentCurrency;

    public UnityAction<int> OnCurrencyChanged;

    private EmployeeData currentCandidate;
    private List<EmployeeData> employeeList = new List<EmployeeData>();
    private int currentIndex = -1;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        if (transform.parent != null) transform.SetParent(null);
        DontDestroyOnLoad(gameObject);
        currentCurrency = startCurrency;
    }

    private void Start() => RefreshRecruitCandidate();

    public EmployeeTable GetEmployeeTable()
    {
        if (employeeTable == null)
            Debug.LogError("GameManager: employeeTable 未在 Inspector 中赋值！");
        return employeeTable;
    }

    public void AddCurrency(int amount)
    {
        if (amount < 0) return;
        currentCurrency += amount;
        OnCurrencyChanged?.Invoke(currentCurrency);
    }

    public bool SpendCurrency(int amount)
    {
        if (amount < 0 || currentCurrency < amount) return false;
        currentCurrency -= amount;
        OnCurrencyChanged?.Invoke(currentCurrency);
        return true;
    }

    public EmployeeData RefreshRecruitCandidate()
    {
        EmployeeTable table = GetEmployeeTable();
        if (table == null) return null;

        var list = table.DataList;
        if (list == null || list.Count == 0) return null;

        currentIndex++;
        if (currentIndex >= list.Count) currentIndex = 0;

        var item = list[currentIndex];
        currentCandidate = new EmployeeData()
        {
            uid = Guid.NewGuid().ToString(),
            id = item.id,
            employeeName = item.employeeName,
            avatarSprite = item.avatarSprite,
            cost = item.cost,
            jobType = item.jobType   // 从表中读取职业
        };
        return currentCandidate;
    }

    public EmployeeData GetCurrentCandidate() => currentCandidate;

    public bool RecruitCurrentCandidate()
    {
        if (currentCandidate == null) return false;
        if (!SpendCurrency(currentCandidate.cost)) return false;

        employeeList.Add(new EmployeeData()
        {
            uid = currentCandidate.uid,
            id = currentCandidate.id,
            employeeName = currentCandidate.employeeName,
            avatarSprite = currentCandidate.avatarSprite,
            cost = currentCandidate.cost,
            jobType = currentCandidate.jobType
        });
        RefreshRecruitCandidate();
        return true;
    }

    public List<EmployeeData> GetEmployeeList() => employeeList;

    public void FireEmployees(List<string> uidList)
    {
        if (uidList == null || uidList.Count == 0) return;
        employeeList.RemoveAll(x => uidList.Contains(x.uid));
    }

    public EmployeeData GetEmployeeByUID(string uid)
    {
        return employeeList.Find(emp => emp.uid == uid);
    }

    public List<EmployeeData> GetIdleEmployeesByJobType(EmployeeJobType jobType)
    {
        return employeeList.FindAll(emp => string.IsNullOrEmpty(emp.assignedBuildingUID) && emp.jobType == jobType);
    }
}