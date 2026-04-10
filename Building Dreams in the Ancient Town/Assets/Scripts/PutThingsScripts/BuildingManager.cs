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
            GameObject newBuildingObj = Instantiate(building.finalPrefab, position, rotation);
            BuildingInstance instance = newBuildingObj.GetComponent<BuildingInstance>();
            if (instance == null)
                instance = newBuildingObj.AddComponent<BuildingInstance>();
            instance.data = building;
            allBuildingInstances.Add(instance);
        }

        constructedBuildings.Add(building);

        ResourceManager.Instance.IncreasePopulationCap(building.populationCapIncrease);
        ResourceManager.Instance.AddHappiness(building.incomeHappiness);

        if (building.woodCapIncrease > 0)
            ResourceManager.Instance.IncreaseWoodCap(building.woodCapIncrease);
        if (building.stoneCapIncrease > 0)
            ResourceManager.Instance.IncreaseStoneCap(building.stoneCapIncrease);

        if (TaskManager.Instance != null)
            TaskManager.Instance.CheckTaskProgress();

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

    public List<BuildingDataSO> GetConstructedBuildings() => constructedBuildings;
    public int GetBuiltCount(string buildingName) => constructedBuildings.Count(b => b.buildingName == buildingName);
    public BuildingInstance GetBuildingInstanceByUID(string uid) => allBuildingInstances.Find(inst => inst.uid == uid);
    public List<BuildingInstance> GetAllBuildingInstances() => allBuildingInstances;

    public bool AssignEmployeeToBuilding(string employeeUID, BuildingInstance building)
    {
        if (building == null) return false;
        if (building.AssignEmployee(employeeUID))
        {
            EmployeeData emp = GameManager.Instance.GetEmployeeByUID(employeeUID);
            if (emp != null) emp.assignedBuildingUID = building.uid;
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
            return true;
        }
        return false;
    }

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