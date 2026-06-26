using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class Route
{
    public Transform startPoint;
    public Transform endPoint;
    [Tooltip("��Ҫ�ﵽ������Ҹ��ȣ�0��ʾ������")]
    public int requiredHappiness = 0;
}

public class NPCManager : MonoBehaviour
{
    [Header("NPC Ԥ�����б�")]
    public List<GameObject> npcPrefabs;

    [Header("���е�·�б���������Ҫ�����ģ�")]
    public List<Route> allRoutes;   // Ԥ���������е�·��ÿ�������� requiredHappiness

    [Header("��������")]
    public float arriveDistance = 0.5f;
    public float moveSpeed = 1f;    // NPC �ƶ��ٶȣ���ͳһ��Ҳ����Ԥ����������ã�

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
        UpdateAvailableRoutes();   // �Ҹ��ȱ仯ʱ������ɸѡ���õ�·
        UpdateNPCs();              // �˿����ޱ仯Ҳ�ᴥ����ͬʱҲ��Ӱ��NPC����
    }

    // ���ݵ�ǰ�Ҹ���ɸѡ���õ�·
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
            Debug.LogWarning("��ǰû�п��õĵ�·���������û��Ҹ���");
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
            Debug.LogWarning("NPCManager: ȱ��Ԥ�������õ�·");
            return;
        }

        // ���ѡ�� NPC Ԥ����
        GameObject selectedPrefab = npcPrefabs[Random.Range(0, npcPrefabs.Count)];
        // ���ѡ��һ�����õ�·
        Route selectedRoute = availableRoutes[Random.Range(0, availableRoutes.Count)];

        GameObject npc = Instantiate(selectedPrefab);
        npc.transform.position = selectedRoute.startPoint.position;

        SimpleWalker walker = npc.GetComponent<SimpleWalker>();
        if (walker != null)
        {
            walker.startPoint = selectedRoute.startPoint;
            walker.endPoint = selectedRoute.endPoint;
            walker.arriveDistance = arriveDistance;
            walker.moveSpeed = moveSpeed;   // ��ѡ��ʹ��ͳһ�ٶ�
        }
        else
        {
            Debug.LogError($"NPC Ԥ���� {selectedPrefab.name} ȱ�� SimpleWalker �����");
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