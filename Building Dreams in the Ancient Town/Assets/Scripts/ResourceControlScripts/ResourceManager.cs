using UnityEngine;
using UnityEngine.Events;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance;

    [Header("初始资源")]
    public int startSilver = 500;
    public int startWood = 100;
    public int startStone = 0;
    public int startHappiness = 20;
    public int startPopulationCap = 0;
    public int startTechPoints = 0; // 初始科技点

    [Header("初始资源上限")]
    public int startWoodCap = 100;
    public int startStoneCap = 0;

    // 当前资源
    public int Silver { get; private set; }
    public int Wood { get; private set; }
    public int Stone { get; private set; }
    public int Happiness { get; private set; }
    public int PopulationCap { get; private set; }
    public int TechPoints { get; private set; }

    // 当前资源上限
    public int WoodCap { get; private set; }
    public int StoneCap { get; private set; }

    // 收入乘数（用于科技加成）
    public float silverIncomeMultiplier = 1.0f;
    public float woodIncomeMultiplier = 1.0f;
    public float stoneIncomeMultiplier = 1.0f;

    public UnityAction OnResourcesChanged;

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
            return;
        }

        Silver = startSilver;
        Wood = startWood;
        Stone = startStone;
        Happiness = startHappiness;
        PopulationCap = startPopulationCap;
        TechPoints = startTechPoints;
        WoodCap = startWoodCap;
        StoneCap = startStoneCap;
    }

    public bool CanAfford(int silver, int wood, int stone, int techPoints = 0)
    {
        return Silver >= silver && Wood >= wood && Stone >= stone && TechPoints >= techPoints;
    }

    public bool SpendResources(int silver, int wood, int stone, int techPoints = 0)
    {
        if (!CanAfford(silver, wood, stone, techPoints)) return false;
        Silver -= silver;
        Wood -= wood;
        Stone -= stone;
        TechPoints -= techPoints;
        OnResourcesChanged?.Invoke();
        return true;
    }

    public void AddResources(int silver, int wood, int stone)
    {
        Silver += silver;
        Wood = Mathf.Min(Wood + wood, WoodCap);
        Stone = Mathf.Min(Stone + stone, StoneCap);
        OnResourcesChanged?.Invoke();
    }

    public void AddHappiness(int amount)
    {
        Happiness += amount;
        OnResourcesChanged?.Invoke();
    }

    public void IncreasePopulationCap(int amount)
    {
        PopulationCap += amount;
        OnResourcesChanged?.Invoke();
    }

    public void IncreaseWoodCap(int amount)
    {
        WoodCap += amount;
        if (Wood > WoodCap) Wood = WoodCap;
        OnResourcesChanged?.Invoke();
    }

    public void IncreaseStoneCap(int amount)
    {
        StoneCap += amount;
        if (Stone > StoneCap) Stone = StoneCap;
        OnResourcesChanged?.Invoke();
    }

    // 任务系统调用此方法增加科技点
    public void AddTechPoints(int amount)
    {
        TechPoints += amount;
        OnResourcesChanged?.Invoke();
    }
}