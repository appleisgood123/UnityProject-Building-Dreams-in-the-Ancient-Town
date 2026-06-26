using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using System.Collections;
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

#if UNITY_EDITOR
        // 编辑器下自动清除旧的桥视频标记，确保每次测试都能看到
        PlayerPrefs.DeleteKey("FirstBridgeVideoPlayed");
#endif
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

        // 首次建桥播放中间视频
        CheckFirstBridgeVideo(building);

        return true;
    }

    private const string FIRST_BRIDGE_KEY = "FirstBridgeVideoPlayed";

    private void CheckFirstBridgeVideo(BuildingDataSO building)
    {
        // 只有桥类建筑触发
        if (building.buildingName != "惠爱桥" && building.buildingName != "赵州桥")
            return;
        if (PlayerPrefs.GetInt(FIRST_BRIDGE_KEY, 0) == 1)
            return;

        PlayerPrefs.SetInt(FIRST_BRIDGE_KEY, 1);
        PlayerPrefs.Save();
        StartCoroutine(PlayFirstBridgeVideo());
    }

    private IEnumerator PlayFirstBridgeVideo()
    {
        VideoClip clip = Resources.Load<VideoClip>("Video/中间视频");
        if (clip == null) yield break;

        // 暂停游戏
        if (GamePauseManager.Instance != null)
            GamePauseManager.Instance.RequestPause();

        // 渐黑
        Image fadeImage = CreateFullscreenOverlay();
        yield return Fade(fadeImage, 0f, 1f, 0.8f);

        // 播放视频（层级高于黑屏，直接可见）
        yield return PlayVideoOnOverlay(clip);

        // 渐亮恢复
        yield return Fade(fadeImage, 1f, 0f, 0.8f);
        Destroy(fadeImage.gameObject);

        // 恢复游戏
        if (GamePauseManager.Instance != null)
            GamePauseManager.Instance.RequestResume();
    }

    // ====== 视频播放工具（供外部也可用） ======

    private Image CreateFullscreenOverlay()
    {
        GameObject go = new GameObject("FadeCanvas", typeof(Canvas), typeof(Image));
        Canvas c = go.GetComponent<Canvas>();
        c.renderMode = RenderMode.ScreenSpaceOverlay;
        c.sortingOrder = 999;
        Image img = go.GetComponent<Image>();
        img.color = new Color(0, 0, 0, 0);
        img.raycastTarget = false;
        RectTransform r = go.GetComponent<RectTransform>();
        r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one;
        r.sizeDelta = Vector2.zero;
        return img;
    }

    private IEnumerator Fade(Image img, float from, float to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float a = Mathf.Lerp(from, to, elapsed / duration);
            img.color = new Color(0, 0, 0, a);
            yield return null;
        }
        img.color = new Color(0, 0, 0, to);
    }

    private IEnumerator PlayVideoOnOverlay(VideoClip clip)
    {
        GameObject canvasObj = new GameObject("VideoCanvas", typeof(Canvas));
        Canvas canvas = canvasObj.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;

        GameObject rawObj = new GameObject("RawImage", typeof(RectTransform), typeof(RawImage));
        rawObj.transform.SetParent(canvasObj.transform, false);
        RectTransform rt = rawObj.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.sizeDelta = Vector2.zero;
        RawImage rawImage = rawObj.GetComponent<RawImage>();

        VideoPlayer vp = canvasObj.AddComponent<VideoPlayer>();
        vp.source = VideoSource.VideoClip;
        vp.clip = clip;
        vp.renderMode = VideoRenderMode.RenderTexture;
        vp.audioOutputMode = VideoAudioOutputMode.Direct;

        RenderTexture renderTex = new RenderTexture(1920, 1080, 0);
        vp.targetTexture = renderTex;
        rawImage.texture = renderTex;

        bool finished = false;
        vp.loopPointReached += (source) => finished = true;
        vp.Play();
        yield return new WaitUntil(() => finished);

        vp.Stop();
        renderTex.Release();
        Destroy(renderTex);
        Destroy(canvasObj);
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
    public void ClearAllBuildings()
    {
        foreach (var inst in allBuildingInstances.ToArray())
        {
            if (inst.currentNPC != null)
                Destroy(inst.currentNPC);
            Destroy(inst.gameObject);
        }
        allBuildingInstances.Clear();
        constructedBuildings.Clear();
    }

    public BuildingInstance RestoreBuilding(BuildingDataSO buildingData, Vector3 position, Quaternion rotation, List<string> employeeUIDs)
    {
        if (buildingData == null || buildingData.finalPrefab == null) return null;
        GameObject newBuilding = Object.Instantiate(buildingData.finalPrefab, position, rotation);
        BuildingInstance instance = newBuilding.GetComponent<BuildingInstance>();
        if (instance == null) instance = newBuilding.AddComponent<BuildingInstance>();
        instance.data = buildingData;
        if (employeeUIDs != null)
            instance.assignedEmployeeUIDs = new List<string>(employeeUIDs);
        allBuildingInstances.Add(instance);
        constructedBuildings.Add(buildingData);

        // 应用建筑资源效果（和 ConstructBuilding 一致）
        ResourceManager.Instance.IncreasePopulationCap(buildingData.populationCapIncrease);
        ResourceManager.Instance.AddHappiness(buildingData.incomeHappiness);
        if (buildingData.woodCapIncrease > 0) ResourceManager.Instance.IncreaseWoodCap(buildingData.woodCapIncrease);
        if (buildingData.stoneCapIncrease > 0) ResourceManager.Instance.IncreaseStoneCap(buildingData.stoneCapIncrease);

        // 刷新建筑 NPC
        RefreshNPCForBuilding(instance);

        return instance;
    }

    /// <summary>对所有已建造建筑重新应用科技幸福感加成（读档后调用）</summary>
    public void ReapplyAllTechHappinessBonuses()
    {
        foreach (var kv in happinessBonusFromTech)
        {
            BuildingDataSO buildingType = kv.Key;
            int bonus = kv.Value;
            foreach (var instance in allBuildingInstances)
            {
                if (instance.data == buildingType)
                    ResourceManager.Instance.AddHappiness(bonus);
            }
        }
    }

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

    public List<BuildingInstance> AllBuildingInstances => allBuildingInstances;

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