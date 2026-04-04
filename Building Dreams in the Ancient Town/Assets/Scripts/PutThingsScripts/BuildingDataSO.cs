using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewBuilding", menuName = "Game/Building Data")]
public class BuildingDataSO : ScriptableObject
{
    public string buildingName;
    public Sprite iconSprite;
    public Sprite displayImage;
    [TextArea(3, 5)]
    public string description;

    public GameObject previewPrefab;
    public GameObject finalPrefab;

    [Header("建造消耗")]
    public int costSilver;
    public int costWood;
    public int costStone;

    [Header("解锁条件")]
    public int requiredHappiness;
    public BuildingDataSO requiredBuilding;

    [Header("是否需要科技解锁")]
    public bool requireTechUnlock;

    [Header("收益（立即生效）")]
    public int incomeHappiness;
    public int populationCapIncrease;

    [Header("收益（每月）")]
    public int monthlySilver;
    public int monthlyWood;
    public int monthlyStone;

    [Header("资源上限增加（仅仓库类建筑有效）")]
    public int woodCapIncrease;
    public int stoneCapIncrease;
}