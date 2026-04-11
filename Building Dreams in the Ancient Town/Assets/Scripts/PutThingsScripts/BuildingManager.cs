using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class BuildingManager : MonoBehaviour
{
    public static BuildingManager Instance;

    private List<BuildingDataSO> constructedBuildings = new List<BuildingDataSO>();
    private float accumulatedMonths = 0f;

    // 科技带来的建筑幸福度加成
    private Dictionary<BuildingDataSO, int> happinessBonusFromTech = new Dictionary<BuildingDataSO, int>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
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

    // 检查是否可建造（资源 + 可选科技解锁 + 前置 + 幸福度）
    public bool CanBuild(BuildingDataSO building)
    {
        if (building == null) return false;

        // 资源检查
        if (!ResourceManager.Instance.CanAfford(building.costSilver, building.costWood, building.costStone))
            return false;

        // 幸福度检查
        if (ResourceManager.Instance.Happiness < building.requiredHappiness)
            return false;

        // 科技解锁检查（如果需要）
        if (building.requireTechUnlock)
        {
            if (TechManager.Instance == null) return false;
            if (!TechManager.Instance.IsBuildingUnlocked(building)) return false;
        }

        // 前置建筑检查
        if (building.requiredBuilding != null && !constructedBuildings.Contains(building.requiredBuilding))
            return false;

        return true;
    }

    // 修改：增加旋转参数
    public bool ConstructBuilding(BuildingDataSO building, Vector3 position, Quaternion rotation)
    {
        if (!CanBuild(building)) return false;

        ResourceManager.Instance.SpendResources(building.costSilver, building.costWood, building.costStone);

        if (building.finalPrefab != null)
        {
            GameObject newBuilding = Instantiate(building.finalPrefab, position, rotation);
            BuildingInteraction interaction = newBuilding.GetComponent<BuildingInteraction>();
            if (interaction != null)
                interaction.buildingData = building;
        }

        constructedBuildings.Add(building);

        // 立即应用收益
        ResourceManager.Instance.IncreasePopulationCap(building.populationCapIncrease);
        ResourceManager.Instance.AddHappiness(building.incomeHappiness);

        if (building.woodCapIncrease > 0)
            ResourceManager.Instance.IncreaseWoodCap(building.woodCapIncrease);
        if (building.stoneCapIncrease > 0)
            ResourceManager.Instance.IncreaseStoneCap(building.stoneCapIncrease);

        // 触发任务检查
        if (TaskManager.Instance != null)
            TaskManager.Instance.CheckTaskProgress();

        return true;
    }

    public void ApplyMonthlyIncome()
    {
        int totalSilver = 0, totalWood = 0, totalStone = 0;
        foreach (var building in constructedBuildings)
        {
            totalSilver += Mathf.RoundToInt(building.monthlySilver * ResourceManager.Instance.silverIncomeMultiplier);
            totalWood += Mathf.RoundToInt(building.monthlyWood * ResourceManager.Instance.woodIncomeMultiplier);
            totalStone += Mathf.RoundToInt(building.monthlyStone * ResourceManager.Instance.stoneIncomeMultiplier);
        }
        ResourceManager.Instance.AddResources(totalSilver, totalWood, totalStone);
    }

    public List<BuildingDataSO> GetConstructedBuildings() => constructedBuildings;

    public int GetBuiltCount(string buildingName)
    {
        return constructedBuildings.Count(b => b.buildingName == buildingName);
    }

    // 科技带来的幸福度加成
    public void AddHappinessBonus(BuildingDataSO building, int bonus)
    {
        if (building == null) return;
        if (!happinessBonusFromTech.ContainsKey(building))
            happinessBonusFromTech[building] = 0;
        happinessBonusFromTech[building] += bonus;
    }

    public int GetTotalBuildingHappiness(BuildingDataSO building)
    {
        if (building == null) return 0;
        int bonus = happinessBonusFromTech.ContainsKey(building) ? happinessBonusFromTech[building] : 0;
        return building.incomeHappiness + bonus;
    }
}