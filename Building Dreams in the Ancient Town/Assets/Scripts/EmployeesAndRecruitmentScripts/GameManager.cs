using System;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;

    [Header("数据表引用")]
    public EmployeeTable employeeTable;

    private EmployeeData currentCandidate;
    private List<EmployeeData> employeeList = new List<EmployeeData>();
    private int currentIndex = -1;

    public static GameManager Instance
    {
        get { return _instance; }
    }

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
    }

    private void Start()
    {
        RefreshRecruitCandidate();
    }

    public EmployeeTable GetEmployeeTable()
    {
        if (employeeTable == null)
            Debug.LogError("GameManager: employeeTable 未在 Inspector 中赋值！");
        return employeeTable;
    }

    public EmployeeData RefreshRecruitCandidate()
    {
        EmployeeTable table = GetEmployeeTable();
        if (table == null) return null;

        List<EmployeeTableItem> list = table.DataList;
        if (list == null || list.Count == 0) return null;

        currentIndex++;
        if (currentIndex >= list.Count) currentIndex = 0;

        EmployeeTableItem item = list[currentIndex];

        currentCandidate = new EmployeeData()
        {
            uid = Guid.NewGuid().ToString(),
            id = item.id,
            employeeName = item.employeeName,
            avatarSprite = item.avatarSprite,
            cost = item.cost,
            jobType = item.jobType,
             npcPrefab = item.npcPrefab
        };
        return currentCandidate;
    }

    public bool RecruitCurrentCandidate()
    {
        if (currentCandidate == null) return false;

        // 检查人口上限
        int currentPopulation = employeeList.Count;
        if (currentPopulation >= ResourceManager.Instance.PopulationCap)
        {
            Debug.LogWarning($"人口已达上限 ({currentPopulation}/{ResourceManager.Instance.PopulationCap})，无法继续招募");
            return false;
        }

        int cost = currentCandidate.cost;
        if (!ResourceManager.Instance.SpendResources(cost, 0, 0))
        {
            Debug.LogWarning($"银两不足，需要 {cost}，当前 {ResourceManager.Instance.Silver}");
            return false;
        }

        // 招募逻辑...
        EmployeeData newEmployee = new EmployeeData()
        {
            uid = currentCandidate.uid,
            id = currentCandidate.id,
            employeeName = currentCandidate.employeeName,
            avatarSprite = currentCandidate.avatarSprite,
            cost = currentCandidate.cost,
            jobType = currentCandidate.jobType,
            npcPrefab = currentCandidate.npcPrefab
        };
        employeeList.Add(newEmployee);
        RefreshRecruitCandidate();
        ResourceManager.Instance.OnResourcesChanged?.Invoke();
        return true;
    }
    public List<EmployeeData> GetEmployeeList() => employeeList;

    public void FireEmployees(List<string> uidList)
    {
        if (uidList == null || uidList.Count == 0) return;
        employeeList.RemoveAll(x => uidList.Contains(x.uid));
        ResourceManager.Instance.OnResourcesChanged?.Invoke();
    }

    public EmployeeData GetEmployeeByUID(string uid)
    {
        return employeeList.Find(emp => emp.uid == uid);
    }

    public void ClearAllEmployees()
    {
        employeeList.Clear();
    }

    public void RestoreEmployee(EmployeeSaveEntry entry)
    {
        EmployeeData emp = new EmployeeData()
        {
            uid = entry.uid,
            id = entry.id,
            employeeName = entry.employeeName,
            avatarSprite = null,
            cost = 0,
            jobType = (EmployeeJobType)entry.jobType,
            assignedBuildingUID = entry.assignedBuildingUID
        };
        // Restore avatarSprite and npcPrefab from employeeTable
        if (employeeTable != null)
        {
            foreach (var item in employeeTable.DataList)
            {
                if (item.id == entry.id)
                {
                    emp.avatarSprite = item.avatarSprite;
                    emp.npcPrefab = item.npcPrefab;
                    break;
                }
            }
        }
        employeeList.Add(emp);
    }

    public EmployeeData GetCurrentCandidate() => currentCandidate;

    public List<EmployeeData> GetIdleEmployeesByJobType(EmployeeJobType jobType)
    {
        return employeeList.FindAll(emp => string.IsNullOrEmpty(emp.assignedBuildingUID) && emp.jobType == jobType);
    }
}