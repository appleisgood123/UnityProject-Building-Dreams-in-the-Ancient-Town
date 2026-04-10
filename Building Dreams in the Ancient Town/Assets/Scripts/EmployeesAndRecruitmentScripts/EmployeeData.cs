using System;
using UnityEngine;

[Serializable]
public class EmployeeData
{
    public string uid;
    public int id;
    public string employeeName;
    public Sprite avatarSprite;
    public int cost;
    public EmployeeJobType jobType;          // 员工职业
    public string assignedBuildingUID = "";  // 分配的建筑实例UID
}

public enum EmployeeJobType
{
    Woodcutter,   // 樵夫
    Stonecutter,  // 石匠
    Merchant,     // 商贩
    Administrator // 管事
}