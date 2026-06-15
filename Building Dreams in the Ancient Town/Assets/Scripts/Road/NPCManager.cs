using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class Route
{
    public Transform startPoint;
    public Transform endPoint;
    [Tooltip("需要达到的最低幸福度，0表示无条件")]
    public int requiredHappiness = 0;
}

public class NPCManager : MonoBehaviour
{
    [Header("NPC 预制体列表")]
    public List<GameObject> npcPrefabs;

    [Header("所有道路列表（包括需要解锁的）")]
    public List<Route> allRoutes;   // 预先配置所有道路，每条可设置 requiredHappiness

    [Header("其他设置")]
    public float arriveDistance = 0.5f;
    public float moveSpeed = 2f;    // NPC 移动速度（可统一，也可让预制体各自设置）

    private List<GameObject> activeNPCs = new List<GameObject>();
    private List<Route> availableRoutes = new List<Route>();

    private void Start()
    {
        if (ResourceManager.Instance != null)
            ResourceManager.Instance.OnResourcesChanged += OnResourcesChanged;
        UpdateAvailableRoutes();
        UpdateNPCs();
    }

    private void OnDestroy()
    {
        if (ResourceManager.Instance != null)
            ResourceManager.Instance.OnResourcesChanged -= OnResourcesChanged;
    }

    private void OnResourcesChanged()
    {
        UpdateAvailableRoutes();   // 幸福度变化时，重新筛选可用道路
        UpdateNPCs();              // 人口上限变化也会触发，同时也会影响NPC数量
    }

    // 根据当前幸福度筛选可用道路
    private void UpdateAvailableRoutes()
    {
        availableRoutes.Clear();
        if (ResourceManager.Instance == null) return;
        int currentHappiness = ResourceManager.Instance.Happiness;

        foreach (var route in allRoutes)
        {
            if (route.requiredHappiness <= currentHappiness)
                availableRoutes.Add(route);
        }

        if (availableRoutes.Count == 0)
            Debug.LogWarning("当前没有可用的道路，请检查配置或幸福度");
    }

    private void UpdateNPCs()
    {
        if (ResourceManager.Instance == null) return;
        int targetCount = ResourceManager.Instance.PopulationCap;

        while (activeNPCs.Count < targetCount)
            SpawnNPC();
        while (activeNPCs.Count > targetCount && activeNPCs.Count > 0)
            RemoveLastNPC();
    }

    private void SpawnNPC()
    {
        if (npcPrefabs == null || npcPrefabs.Count == 0 || availableRoutes.Count == 0)
        {
            Debug.LogWarning("NPCManager: 缺少预制体或可用道路");
            return;
        }

        // 随机选择 NPC 预制体
        GameObject selectedPrefab = npcPrefabs[Random.Range(0, npcPrefabs.Count)];
        // 随机选择一条可用道路
        Route selectedRoute = availableRoutes[Random.Range(0, availableRoutes.Count)];

        GameObject npc = Instantiate(selectedPrefab);
        npc.transform.position = selectedRoute.startPoint.position;

        SimpleWalker walker = npc.GetComponent<SimpleWalker>();
        if (walker != null)
        {
            walker.startPoint = selectedRoute.startPoint;
            walker.endPoint = selectedRoute.endPoint;
            walker.arriveDistance = arriveDistance;
            walker.moveSpeed = moveSpeed;   // 可选：使用统一速度
        }
        else
        {
            Debug.LogError($"NPC 预制体 {selectedPrefab.name} 缺少 SimpleWalker 组件！");
            Destroy(npc);
            return;
        }

        activeNPCs.Add(npc);
    }

    private void RemoveLastNPC()
    {
        if (activeNPCs.Count == 0) return;
        GameObject last = activeNPCs[activeNPCs.Count - 1];
        activeNPCs.RemoveAt(activeNPCs.Count - 1);
        Destroy(last);
    }
}