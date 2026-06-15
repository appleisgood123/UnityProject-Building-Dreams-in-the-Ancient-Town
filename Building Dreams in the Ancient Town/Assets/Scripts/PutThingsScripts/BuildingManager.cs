using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class BuildingManager : MonoBehaviour
{
    public static BuildingManager Instance;

    private List<BuildingDataSO> constructedBuildings = new List<BuildingDataSO>();
    private List<BuildingInstance> allBuildingInstances = new List<BuildingInstance>();
    private float accumulatedMonths = 0f;
    private Dictionary<BuildingDataSO, int> happinessBonusFromTech = new Dictionary<BuildingDataSO, int>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void OnEnable()
    {
        if (GameMonthManager.Instance != null)
            GameMonthManager.Instance.OnMonthUpdated += OnMonthUpdated;
    }

    private void OnDisable()
    {
        if (GameMonthManager.Instance != null)
            GameMonthManager.Instance.OnMonthUpdated -= OnMonthUpdated;
    }

    private void OnMonthUpdated(float deltaMonths)
    {
        accumulatedMonths += deltaMonths;
        while (accumulatedMonths >= 1f)
        {
            accumulatedMonths -= 1f;
            ApplyMonthlyIncome();
        }
    }

    public bool CanBuild(BuildingDataSO building)
    {
        if (building == null) return false;
        if (!ResourceManager.Instance.CanAfford(building.costSilver, building.costWood, building.costStone))
            return false;
        if (ResourceManager.Instance.Happiness < building.requiredHappiness)
            return false;
        if (building.requireTechUnlock)
        {
            if (TechManager.Instance == null) return false;
            if (!TechManager.Instance.IsBuildingUnlocked(building)) return false;
        }
        if (building.requiredBuilding != null && !constructedBuildings.Contains(building.requiredBuilding))
            return false;
        return true;
    }

    public bool ConstructBuilding(BuildingDataSO building, Vector3 position, Quaternion rotation)
    {
        if (!CanBuild(building)) return false;

        ResourceManager.Instance.SpendResources(building.costSilver, building.costWood, building.costStone);

        if (building.finalPrefab != null)
        {
            GameObject newBuilding = Instantiate(building.finalPrefab, position, rotation);
            BuildingInstance instance = newBuilding.GetComponent<BuildingInstance>();
            if (instance == null) instance = newBuilding.AddComponent<BuildingInstance>();
            instance.data = building;
            allBuildingInstances.Add(instance);
        }

        constructedBuildings.Add(building);

        ResourceManager.Instance.IncreasePopulationCap(building.populationCapIncrease);
        ResourceManager.Instance.AddHappiness(building.incomeHappiness);
        // 应用科技幸福感加成
        if (happinessBonusFromTech.TryGetValue(building, out int techBonus))
            ResourceManager.Instance.AddHappiness(techBonus);
        if (building.woodCapIncrease > 0) ResourceManager.Instance.IncreaseWoodCap(building.woodCapIncrease);
        if (building.stoneCapIncrease > 0) ResourceManager.Instance.IncreaseStoneCap(building.stoneCapIncrease);
        if (TaskManager.Instance != null) TaskManager.Instance.CheckTaskProgress();

        return true;
    }

    public void ApplyMonthlyIncome()
    {
        int totalSilver = 0, totalWood = 0, totalStone = 0;
        foreach (var instance in allBuildingInstances)
        {
            var building = instance.data;
            if (building.requiresEmployeeToWork && !instance.IsFullyStaffed())
                continue;
            totalSilver += Mathf.RoundToInt(building.monthlySilver * ResourceManager.Instance.silverIncomeMultiplier);
            totalWood += Mathf.RoundToInt(building.monthlyWood * ResourceManager.Instance.woodIncomeMultiplier);
            totalStone += Mathf.RoundToInt(building.monthlyStone * ResourceManager.Instance.stoneIncomeMultiplier);
        }
        ResourceManager.Instance.AddResources(totalSilver, totalWood, totalStone);
    }

    public BuildingInstance GetBuildingInstanceByUID(string uid)
    {
        return allBuildingInstances.Find(inst => inst.uid == uid);
    }

    // ---------- NPC ˢ�º����߼� ----------
    private void RefreshNPCForBuilding(BuildingInstance building)
    {
        if (building == null) return;
        // ���پ�NPC
        if (building.currentNPC != null)
            Destroy(building.currentNPC);
        building.currentNPC = null;

        // �����Ա�������ɵ�һ��Ա����NPC
        if (building.assignedEmployeeUIDs.Count > 0)
        {
            string firstUID = building.assignedEmployeeUIDs[0];
            EmployeeData emp = GameManager.Instance.GetEmployeeByUID(firstUID);
            if (emp != null && emp.npcPrefab != null && building.npcSpawnPoint != null)
            {
                building.currentNPC = Instantiate(emp.npcPrefab, building.npcSpawnPoint.position, building.npcSpawnPoint.rotation);
                building.currentNPC.transform.SetParent(building.transform);
            }
        }
    }

    public bool AssignEmployeeToBuilding(string employeeUID, BuildingInstance building)
    {
        if (building == null) return false;
        if (building.AssignEmployee(employeeUID))
        {
            EmployeeData emp = GameManager.Instance.GetEmployeeByUID(employeeUID);
            if (emp != null) emp.assignedBuildingUID = building.uid;
            RefreshNPCForBuilding(building);
            return true;
        }
        return false;
    }

    public bool RemoveEmployeeFromBuilding(string employeeUID, BuildingInstance building)
    {
        if (building == null) return false;
        if (building.RemoveEmployee(employeeUID))
        {
            EmployeeData emp = GameManager.Instance.GetEmployeeByUID(employeeUID);
            if (emp != null) emp.assignedBuildingUID = "";
            RefreshNPCForBuilding(building);
            return true;
        }
        return false;
    }

    // ---------- ������� ----------
    public void DemolishBuilding(BuildingInstance buildingInstance)
    {
        if (buildingInstance == null) return;

        // 1. �ͷ����з����Ա��
        foreach (string empUID in buildingInstance.assignedEmployeeUIDs.ToArray())
        {
            RemoveEmployeeFromBuilding(empUID, buildingInstance);
        }

        // 2. �ӽ���ʵ���б����Ƴ�
        allBuildingInstances.Remove(buildingInstance);
        constructedBuildings.Remove(buildingInstance.data);

        // 3. ����һ����Դ
        BuildingDataSO data = buildingInstance.data;
        int refundSilver = data.costSilver / 2;
        int refundWood = data.costWood / 2;
        int refundStone = data.costStone / 2;
        ResourceManager.Instance.AddResources(refundSilver, refundWood, refundStone);

        // 4. �Ƴ���������������
        ResourceManager.Instance.IncreasePopulationCap(-data.populationCapIncrease);
        ResourceManager.Instance.AddHappiness(-data.incomeHappiness);
        // 移除科技幸福感加成
        if (happinessBonusFromTech.TryGetValue(data, out int techBonus))
            ResourceManager.Instance.AddHappiness(-techBonus);
        if (data.woodCapIncrease > 0)
            ResourceManager.Instance.IncreaseWoodCap(-data.woodCapIncrease);
        if (data.stoneCapIncrease > 0)
            ResourceManager.Instance.IncreaseStoneCap(-data.stoneCapIncrease);

        // 5. ����NPC����ѡ����Ϊ��������ʱ������Ҳ�����٣������������ã�
        if (buildingInstance.currentNPC != null)
            Destroy(buildingInstance.currentNPC);

        // 6. ���ٽ�������
        Destroy(buildingInstance.gameObject);
    }

    public List<BuildingDataSO> GetConstructedBuildings() => constructedBuildings;
    public int GetBuiltCount(string buildingName) => constructedBuildings.Count(b => b.buildingName == buildingName);

    public void AddHappinessBonus(BuildingDataSO building, int bonus)
    {
        if (building == null) return;

        int oldBonus = 0;
        happinessBonusFromTech.TryGetValue(building, out oldBonus);
        int delta = bonus - oldBonus;
        happinessBonusFromTech[building] = bonus;

        // 对已建造的该类型建筑，应用幸福度差值
        foreach (var instance in allBuildingInstances)
        {
            if (instance.data == building)
            {
                ResourceManager.Instance.AddHappiness(delta);
            }
        }
    }

    public int GetTotalBuildingHappiness(BuildingDataSO building)
    {
        if (building == null) return 0;
        happinessBonusFromTech.TryGetValue(building, out int bonus);
        return bonus;
    }
}