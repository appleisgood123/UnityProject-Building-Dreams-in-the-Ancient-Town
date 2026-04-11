using UnityEngine;
using System.Collections.Generic;

// 枚举定义（只在此文件中保留）
public enum TechEffectType
{
    None,
    IncreaseSilverIncome,
    IncreaseWoodIncome,
    IncreaseStoneIncome,
    IncreaseBuildingHappiness,
    IncreasePopulationCap
}

[CreateAssetMenu(fileName = "NewTechNode", menuName = "Game/Tech Node")]
public class TechNodeData : ScriptableObject
{
    public string nodeName;
    public string description;
    public Sprite icon;

    [Header("前置科技")]
    public List<TechNodeData> prerequisites;

    [Header("解锁消耗")]
    public int requiredSilver;
    public int requiredWood;
    public int requiredStone;
    public int requiredTechPoints;

    [Header("解锁条件（不消耗）")]
    public int requiredHappiness;

    [Header("科技效果")]
    public TechEffectType effectType;
    public float effectValue;

    [Header("效果目标建筑（多个）")]
    public List<BuildingDataSO> targetBuildings;

    [Header("解锁建筑（多个）")]
    public List<BuildingDataSO> unlockedBuildings;
}