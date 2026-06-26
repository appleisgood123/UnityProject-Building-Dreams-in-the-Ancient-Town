using System;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;

    [Header("���ݱ�����")]
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
            Debug.LogError("GameManager: employeeTable δ�� Inspector �и�ֵ��");
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

        // ����˿�����
        int currentPopulation = employeeList.Count;
        if (currentPopulation >= ResourceManager.Instance.PopulationCap)
        {
            Debug.LogWarning($"�˿��Ѵ����� ({currentPopulation}/{ResourceManager.Instance.PopulationCap})���޷�������ļ");
            return false;
        }

        int cost = currentCandidate.cost;
        if (!ResourceManager.Instance.SpendResources(cost, 0, 0))
        {
            Debug.LogWarning($"�������㣬��Ҫ {cost}����ǰ {ResourceManager.Instance.Silver}");
            return false;
        }

        // ��ļ�߼�...
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
        // 从 employeeTable 恢复 avatarSprite、npcPrefab 和 cost
        if (employeeTable != null)
        {
            foreach (var item in employeeTable.DataList)
            {
                if (item.id == entry.id)
                {
                    emp.avatarSprite = item.avatarSprite;
                    emp.npcPrefab = item.npcPrefab;
                    emp.cost = item.cost;
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