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
    public Sprite avatarSprite;   // Ö±½ÓÍÏ×§Í¼Æ¬
    public int cost;
}