using UnityEngine;
using System.Collections.Generic;

public class BuildingInstance : MonoBehaviour
{
    public string uid;
    public BuildingDataSO data;
    public List<string> assignedEmployeeUIDs = new List<string>();

    private void Awake()
    {
        if (string.IsNullOrEmpty(uid))
            uid = System.Guid.NewGuid().ToString();
    }

    // 分配员工
    public bool AssignEmployee(string employeeUID)
    {
        if (assignedEmployeeUIDs.Count >= data.requiredEmployeeCount)
            return false;
        if (!assignedEmployeeUIDs.Contains(employeeUID))
        {
            assignedEmployeeUIDs.Add(employeeUID);
            return true;
        }
        return false;
    }

    // 移除员工
    public bool RemoveEmployee(string employeeUID)
    {
        return assignedEmployeeUIDs.Remove(employeeUID);
    }

    // 是否满足员工需求（能正常工作）
    public bool IsFullyStaffed()
    {
        return assignedEmployeeUIDs.Count >= data.requiredEmployeeCount;
    }

    // 获取当前分配的员工数量
    public int GetStaffCount()
    {
        return assignedEmployeeUIDs.Count;
    }
}