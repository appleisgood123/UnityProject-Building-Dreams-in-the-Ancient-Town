using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "XiaoQi/EmployeeTable", fileName = "EmployeeTable")]
public class EmployeeTable : ScriptableObject
{
    public List<EmployeeTableItem> DataList = new List<EmployeeTableItem>();
}

[System.Serializable]
public class EmployeeTableItem
{
    public int id;
    public string employeeName;
    public Sprite avatarSprite;
    public int cost;
    public EmployeeJobType jobType;   // 新增职业字段
    public GameObject npcPrefab;
}