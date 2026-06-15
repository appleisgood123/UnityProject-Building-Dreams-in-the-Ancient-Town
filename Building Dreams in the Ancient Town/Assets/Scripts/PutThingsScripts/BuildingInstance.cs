using UnityEngine;
using System.Collections.Generic;

public class BuildingInstance : MonoBehaviour
{
    public string uid;
    public BuildingDataSO data;
    public List<string> assignedEmployeeUIDs = new List<string>();
    public Transform npcSpawnPoint;      // 在建筑预制体中手动放置的空物体作为生成点
    public GameObject currentNPC;        // 当前生成的NPC实例

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