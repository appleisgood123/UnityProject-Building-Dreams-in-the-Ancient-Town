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
    public EmployeeJobType jobType;               // 职业
    public string assignedBuildingUID = "";       // 分配的建筑实例UID
}

public enum EmployeeJobType
{
    Woodcutter,   // 樵夫 → 伐木场
    Stonecutter,  // 石匠 → 采石场
    Merchant,     // 商贩 → 银两建筑
    Administrator // 管事
}