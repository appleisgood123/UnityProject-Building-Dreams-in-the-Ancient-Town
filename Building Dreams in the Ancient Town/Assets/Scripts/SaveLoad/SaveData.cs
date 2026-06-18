using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SaveData
{
    public int slotIndex;
    public string saveTime;

    // Player
    public float playerPosX;
    public float playerPosY;
    public float playerPosZ;
    public float playerRotY;

    // Resources
    public int silver;
    public int wood;
    public int stone;
    public int happiness;
    public int populationCap;
    public int techPoints;
    public int woodCap;
    public int stoneCap;
    public float silverIncomeMultiplier;
    public float woodIncomeMultiplier;
    public float stoneIncomeMultiplier;

    // Time
    public float totalMonths;

    // Tech
    public List<string> unlockedTechNames = new List<string>();

    // Buildings
    public List<BuildingSaveEntry> buildings = new List<BuildingSaveEntry>();

    // Employees
    public List<EmployeeSaveEntry> employees = new List<EmployeeSaveEntry>();

    // Tasks
    public List<string> completedTaskNames = new List<string>();
    public List<string> activeTaskNames = new List<string>();
}

[Serializable]
public class BuildingSaveEntry
{
    public string buildingName;
    public float posX;
    public float posY;
    public float posZ;
    public float rotX;
    public float rotY;
    public float rotZ;
    public float rotW;
    public List<string> assignedEmployeeUIDs = new List<string>();
}

[Serializable]
public class EmployeeSaveEntry
{
    public string uid;
    public int id;
    public string employeeName;
    public int jobType;
    public string assignedBuildingUID;
}
