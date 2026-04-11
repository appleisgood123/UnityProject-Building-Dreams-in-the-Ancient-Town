using System;
using UnityEngine;

[Serializable]
public class EmployeeData
{
    public string uid;
    public int id;
    public string employeeName;
    public Sprite avatarSprite;   // 直接引用图片
    public int cost;
}