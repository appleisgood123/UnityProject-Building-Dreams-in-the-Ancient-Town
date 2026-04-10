using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class TechManager : MonoBehaviour
{
    public static TechManager Instance;

    [SerializeField] private List<TechNodeData> allTechNodes;

    private HashSet<TechNodeData> unlockedTechs = new HashSet<TechNodeData>();

    public System.Action<TechNodeData> OnTechUnlocked;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public bool IsUnlocked(TechNodeData tech)
    {
        return unlockedTechs.Contains(tech);
    }

    public bool CanUnlock(TechNodeData tech)
    {
        if (IsUnlocked(tech)) return false;

        foreach (var pre in tech.prerequisites)
            if (!IsUnlocked(pre)) return false;

        if (ResourceManager.Instance.Happiness < tech.requiredHappiness)
            return false;

        return ResourceManager.Instance.CanAfford(tech.requiredSilver, tech.requiredWood, tech.requiredStone, tech.requiredTechPoints);
    }

    public bool UnlockTech(TechNodeData tech)
    {
        if (!CanUnlock(tech)) return false;

        ResourceManager.Instance.SpendResources(tech.requiredSilver, tech.requiredWood, tech.requiredStone, tech.requiredTechPoints);

        unlockedTechs.Add(tech);
        ApplyTechEffect(tech);
        OnTechUnlocked?.Invoke(tech);

        if (TaskManager.Instance != null)
            TaskManager.Instance.CheckTaskProgress();

        return true;
    }

    private void ApplyTechEffect(TechNodeData tech)
    {
        switch (tech.effectType)
        {
            case TechEffectType.IncreaseSilverIncome:
                ResourceManager.Instance.silverIncomeMultiplier += tech.effectValue / 100f;
                break;
            case TechEffectType.IncreaseWoodIncome:
                ResourceManager.Instance.woodIncomeMultiplier += tech.effectValue / 100f;
                break;
            case TechEffectType.IncreaseStoneIncome:
                ResourceManager.Instance.stoneIncomeMultiplier += tech.effectValue / 100f;
                break;
            case TechEffectType.IncreaseBuildingHappiness:
                foreach (var building in tech.targetBuildings)
                    BuildingManager.Instance.AddHappinessBonus(building, (int)tech.effectValue);
                break;
            case TechEffectType.IncreasePopulationCap:
                ResourceManager.Instance.IncreasePopulationCap((int)tech.effectValue);
                break;
        }
    }

    public List<TechNodeData> GetAllTechNodes() => allTechNodes;

    public List<BuildingDataSO> GetUnlockedBuildings()
    {
        List<BuildingDataSO> result = new List<BuildingDataSO>();
        foreach (var tech in unlockedTechs)
            if (tech.unlockedBuildings != null)
                result.AddRange(tech.unlockedBuildings);
        return result;
    }

    public bool IsBuildingUnlocked(BuildingDataSO building)
    {
        foreach (var tech in unlockedTechs)
        {
            if (tech.unlockedBuildings != null && tech.unlockedBuildings.Contains(building))
                return true;
        }
        return false;
    }

    public bool IsTechUnlocked(string techName)
    {
        foreach (var tech in unlockedTechs)
        {
            if (tech.nodeName == techName)
                return true;
        }
        return false;
    }
}