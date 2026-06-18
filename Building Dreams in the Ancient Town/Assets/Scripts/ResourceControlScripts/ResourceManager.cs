using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance;

    [Header("��ʼ��Դ")]
    public int startSilver = 500;
    public int startWood = 100;
    public int startStone = 0;
    public int startHappiness = 20;
    public int startPopulationCap = 0;
    public int startTechPoints = 0; // ��ʼ�Ƽ���

    [Header("��ʼ��Դ����")]
    public int startWoodCap = 100;
    public int startStoneCap = 0;

    // ��ǰ��Դ
    public int Silver { get; private set; }
    public int Wood { get; private set; }
    public int Stone { get; private set; }
    public int Happiness { get; private set; }
    public int PopulationCap { get; private set; }
    public int TechPoints { get; private set; }

    // ��ǰ��Դ����
    public int WoodCap { get; private set; }
    public int StoneCap { get; private set; }

    // ������������ڿƼ��ӳɣ�
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

        // 挂载音效管理器
        if (GetComponent<AudioManager>() == null)
            gameObject.AddComponent<AudioManager>();

        // 挂载设置面板（ESC 键）
        if (GetComponent<GameSettings>() == null)
            gameObject.AddComponent<GameSettings>();

        // 设置背景音乐循环播放
        SetupBGM();
    }

    private IEnumerator Start()
    {
        // 如果有待加载的存档数据，等一帧让其他Manager初始化后恢复
        if (SaveLoadManager.PendingLoadData != null)
        {
            yield return null;
            SaveLoadManager.ApplySaveData(SaveLoadManager.PendingLoadData);
            SaveLoadManager.PendingLoadData = null;
        }
    }

    private void SetupBGM()
    {
        AudioClip bgmClip = Resources.Load<AudioClip>("Audio/《古筝山水间的悠然长调》笑眯眯的巴旦木");
        if (bgmClip == null)
        {
            Debug.LogWarning("未找到背景音乐: Resources/Audio/《古筝山水间的悠然长调》笑眯眯的巴旦木.mp3");
            return;
        }

        AudioSource audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = bgmClip;
        audioSource.loop = true;
        audioSource.volume = PlayerPrefs.GetFloat("BGM_Volume", 0.5f);
        audioSource.playOnAwake = false;
        audioSource.Play();
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

    // ����ϵͳ���ô˷������ӿƼ���
    public void LoadFromSaveData(SaveData data)
    {
        Silver = data.silver;
        Wood = data.wood;
        Stone = data.stone;
        Happiness = data.happiness;
        PopulationCap = data.populationCap;
        TechPoints = data.techPoints;
        WoodCap = data.woodCap;
        StoneCap = data.stoneCap;
        silverIncomeMultiplier = data.silverIncomeMultiplier;
        woodIncomeMultiplier = data.woodIncomeMultiplier;
        stoneIncomeMultiplier = data.stoneIncomeMultiplier;
        OnResourcesChanged?.Invoke();
    }

    public void AddTechPoints(int amount)
    {
        TechPoints += amount;
        OnResourcesChanged?.Invoke();
    }
}