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

    public bool AssignEmployee(string employeeUID)
    {
        if (assignedEmployeeUIDs.Contains(employeeUID)) return false;
        assignedEmployeeUIDs.Add(employeeUID);
        return true;
    }

    public bool RemoveEmployee(string employeeUID)
    {
        return assignedEmployeeUIDs.Remove(employeeUID);
    }

    public bool IsFullyStaffed()
    {
        return assignedEmployeeUIDs.Count >= (data != null ? data.requiredEmployeeCount : 1);
    }
}