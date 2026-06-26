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

    [Header("��������")]
    public int costSilver;
    public int costWood;
    public int costStone;

    [Header("��������")]
    public int requiredHappiness;
    public BuildingDataSO requiredBuilding;

    [Header("�Ƿ���Ҫ�Ƽ�����")]
    public bool requireTechUnlock;

    [Header("���棨������Ч��")]
    public int incomeHappiness;
    public int populationCapIncrease;

    [Header("���棨ÿ�£�")]
    public int monthlySilver;
    public int monthlyWood;
    public int monthlyStone;

    [Header("��Դ�������ӣ����ֿ��ཨ����Ч��")]
    public int woodCapIncrease;
    public int stoneCapIncrease;

    [Header("Ա������")]
    public EmployeeJobType requiredEmployeeType;
    public int requiredEmployeeCount = 1;
    public bool requiresEmployeeToWork = true;

    [Header("��������")]
    public bool ignoreGroundCheck = false;   // �ŵ����⽨���ɺ��Ե�������
}