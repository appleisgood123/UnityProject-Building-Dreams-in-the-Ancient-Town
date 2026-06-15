using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DraggableIcon : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("�������ݣ��� BuildingPageManager �Զ���ֵ��")]
    public BuildingDataSO buildingData;

    [Header("UI ��壨���ڼ������뿪������壩")]
    public RectTransform uiPanelRect;

    [Header("��������")]
    public LayerMask groundLayer = 1 << 0;
    public Material validPlacementMaterial;
    public Material invalidPlacementMaterial;

    [Header("��ײ���")]
    public LayerMask blockingLayers = ~0;

    [Header("��Ч")]
    public AudioClip placementSound;

    [Header("��Ч")]
    public GameObject placementEffect;
    public float effectDuration = 2f;

    [Header("ȷ��/ȡ��/��ת���")]
    public GameObject confirmPanel;
    public bool followModelPosition = true;
    public Vector2 panelScreenOffset = new Vector2(0, 100);

    [Header("��ת����")]
    public float rotateStep = 90f;

    private GameObject previewInstance;
    private Collider[] previewColliders;
    private Camera mainCamera;
    private bool isDragging = false;
    private bool previewActive = false;
    private bool canPlace = false;

    private Button confirmButton;
    private Button cancelButton;
    private Button rotateButton;

    void Start()
    {
        mainCamera = Camera.main;

        if (confirmPanel != null)
        {
            confirmButton = confirmPanel.transform.Find("ConfirmButton")?.GetComponent<Button>();
            cancelButton = confirmPanel.transform.Find("CancelButton")?.GetComponent<Button>();
            rotateButton = confirmPanel.transform.Find("RotateButton")?.GetComponent<Button>();

            if (confirmButton != null)
                confirmButton.onClick.AddListener(OnConfirmClicked);
            if (cancelButton != null)
                cancelButton.onClick.AddListener(OnCancelClicked);
            if (rotateButton != null)
                rotateButton.onClick.AddListener(OnRotateClicked);

            confirmPanel.SetActive(false);
        }
    }

    void OnDestroy()
    {
        if (confirmButton != null)
            confirmButton.onClick.RemoveListener(OnConfirmClicked);
        if (cancelButton != null)
            cancelButton.onClick.RemoveListener(OnCancelClicked);
        if (rotateButton != null)
            rotateButton.onClick.RemoveListener(OnRotateClicked);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // ����������ɽ��죬��ֹ��ק
        if (buildingData != null && !CanBuildBuilding())
        {
            return;
        }
        if (previewActive) EndDragCleanup();
        isDragging = true;
        previewActive = false;
        canPlace = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging) return;

        bool isMouseOverUI = RectTransformUtility.RectangleContainsScreenPoint(
            uiPanelRect, eventData.position, eventData.pressEventCamera);

        if (!isMouseOverUI)
        {
            if (!previewActive)
            {
                CreatePreview();
                previewActive = true;
                UpdatePreviewPositionAndCheckPlacement(eventData.position);
            }
            else
            {
                UpdatePreviewPositionAndCheckPlacement(eventData.position);
            }
        }
        else
        {
            if (previewActive) EndDragCleanup();
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (previewActive)
        {
            if (canPlace)
            {
                ShowConfirmPanel();
            }
            else
            {
                EndDragCleanup();
            }
        }
        isDragging = false;
    }

    private void CreatePreview()
    {
        if (buildingData == null || buildingData.previewPrefab == null) return;
        previewInstance = Instantiate(buildingData.previewPrefab);
        previewColliders = previewInstance.GetComponentsInChildren<Collider>();
        foreach (var col in previewColliders) col.enabled = false;
    }

    private void ShowConfirmPanel()
    {
        if (confirmPanel == null) return;
        confirmPanel.SetActive(true);
        if (followModelPosition && previewInstance != null)
        {
            Vector3 screenPos = mainCamera.WorldToScreenPoint(previewInstance.transform.position);
            RectTransform panelRect = confirmPanel.GetComponent<RectTransform>();
            if (panelRect != null)
            {
                Vector2 pos = new Vector2(screenPos.x + panelScreenOffset.x, screenPos.y + panelScreenOffset.y);
                float halfWidth = panelRect.rect.width / 2;
                float halfHeight = panelRect.rect.height / 2;
                pos.x = Mathf.Clamp(pos.x, halfWidth, Screen.width - halfWidth);
                pos.y = Mathf.Clamp(pos.y, halfHeight, Screen.height - halfHeight);
                panelRect.position = pos;
            }
        }
    }

    private void OnConfirmClicked()
    {
        if (previewInstance == null || !canPlace || buildingData == null) return;

        if (BuildingManager.Instance.ConstructBuilding(buildingData, previewInstance.transform.position, previewInstance.transform.rotation))
        {
            // ʹ�ý���Ԥ��ģ�͵İ�Χ�����Ƴ���ľ
            RemoveTreesByBounds(previewInstance);

            // 播放放置音效
            PlaySFX("放置音效");
            if (placementEffect != null)
            {
                GameObject effect = Instantiate(placementEffect, previewInstance.transform.position, Quaternion.identity);
                Destroy(effect, effectDuration);
            }
        }
        else
        {
            Debug.LogWarning("�����������㣡");
        }

        PlaySFX("确认点击");
        EndDragCleanup();
    }
    private void OnCancelClicked()
    {
        PlaySFX("取消点击");
        EndDragCleanup();
    }

    private void PlaySFX(string clipName)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(clipName);
    }

    private void OnRotateClicked()
    {
        if (previewInstance != null)
        {
            previewInstance.transform.Rotate(Vector3.up, rotateStep, Space.World);
        }
    }

    // �޸ĺ�ķ�����֧�� ignoreGroundCheck
    private void UpdatePreviewPositionAndCheckPlacement(Vector2 screenPos)
    {
        if (previewInstance == null) return;

        Ray ray = mainCamera.ScreenPointToRay(screenPos);
        RaycastHit hit;
        bool hitSomething = Physics.Raycast(ray, out hit, 1000f);

        if (hitSomething)
        {
            previewInstance.transform.position = hit.point;

            bool canBuild = BuildingManager.Instance.CanBuild(buildingData);

            if (buildingData.ignoreGroundCheck)
            {
                // �ŵ����⽨�������Ե�����ص����
                canPlace = canBuild;
            }
            else
            {
                bool onGround = ((1 << hit.collider.gameObject.layer) & groundLayer) != 0;
                bool overlapping = CheckOverlap();
                canPlace = onGround && canBuild && !overlapping;
            }

            ApplyMaterialToPreview(canPlace ? validPlacementMaterial : invalidPlacementMaterial);
        }
        else
        {
            previewInstance.transform.position = Vector3.one * 10000f;
            canPlace = false;
        }
    }

    private bool CheckOverlap()
    {
        if (previewInstance == null || previewColliders == null || previewColliders.Length == 0)
            return false;

        Bounds combinedBounds = new Bounds(previewInstance.transform.position, Vector3.zero);
        foreach (var col in previewColliders)
            combinedBounds.Encapsulate(col.bounds);

        Collider[] overlapped = Physics.OverlapBox(combinedBounds.center, combinedBounds.extents, previewInstance.transform.rotation, blockingLayers);

        foreach (var col in overlapped)
        {
            bool isSelf = false;
            foreach (var selfCol in previewColliders)
            {
                if (col == selfCol) { isSelf = true; break; }
            }
            if (!isSelf) return true;
        }
        return false;
    }

    private void EndDragCleanup()
    {
        if (previewInstance != null)
            Destroy(previewInstance);
        previewInstance = null;
        previewColliders = null;
        previewActive = false;
        isDragging = false;
        canPlace = false;

        if (confirmPanel != null)
            confirmPanel.SetActive(false);
    }

    private void ApplyMaterialToPreview(Material mat)
    {
        if (mat == null || previewInstance == null) return;
        Renderer[] renderers = previewInstance.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            Material[] mats = new Material[renderer.materials.Length];
            for (int i = 0; i < mats.Length; i++) mats[i] = mat;
            renderer.materials = mats;
        }
    }

    private void RemoveTreesOnTerrain(Vector3 position, float radius)
    {
        Terrain[] terrains = Terrain.activeTerrains;
        if (terrains == null || terrains.Length == 0)
        {
            Debug.Log("û���ҵ��κε���");
            return;
        }

        bool anyTreeRemoved = false;

        foreach (Terrain terrain in terrains)
        {
            TerrainData terrainData = terrain.terrainData;
            if (terrainData == null) continue;

            // ����������ת�������ξֲ�����
            Vector3 localPos = terrain.transform.InverseTransformPoint(position);
            // �����Ƿ��ڵ��η�Χ��
            if (localPos.x < 0 || localPos.x > terrainData.size.x ||
                localPos.z < 0 || localPos.z > terrainData.size.z)
            {
                Debug.Log($"�� {position} ���ڵ��� {terrain.name} ��Χ�ڣ�����");
                continue;
            }

            // ��һ������ (0~1)
            float xNorm = localPos.x / terrainData.size.x;
            float zNorm = localPos.z / terrainData.size.z;

            // �뾶��һ������Ϊ����λ���ǹ�һ�����꣩
            float radiusNorm = radius / Mathf.Max(terrainData.size.x, terrainData.size.z);
            Debug.Log($"���� {terrain.name} �뾶��һ��: {radiusNorm}");

            TreeInstance[] trees = terrainData.treeInstances;
            if (trees.Length == 0)
            {
                Debug.Log($"���� {terrain.name} ��û����");
                continue;
            }

            List<TreeInstance> newTrees = new List<TreeInstance>();
            int removedCount = 0;

            foreach (var tree in trees)
            {
                Vector3 treePos = new Vector3(tree.position.x, 0, tree.position.z);
                Vector3 targetPos = new Vector3(xNorm, 0, zNorm);
                float distance = Vector3.Distance(treePos, targetPos);
                if (distance > radiusNorm)
                {
                    newTrees.Add(tree);
                }
                else
                {
                    removedCount++;
                    // ������������������ڵ���
                    Vector3 worldTreePos = terrain.transform.TransformPoint(new Vector3(tree.position.x * terrainData.size.x, 0, tree.position.z * terrainData.size.z));
                    Debug.Log($"�Ƴ��� at {worldTreePos}");
                }
            }

            if (removedCount > 0)
            {
                terrainData.treeInstances = newTrees.ToArray();
                terrain.Flush();
                anyTreeRemoved = true;
                Debug.Log($"���� {terrain.name} �Ƴ��� {removedCount} ����");
            }
        }

        if (!anyTreeRemoved)
        {
            Debug.Log($"λ�� {position} �뾶 {radius} ��δ������ľ");
        }
    }

    private bool CanBuildBuilding()
    {
        if (buildingData == null) return false;
        if (!ResourceManager.Instance.CanAfford(buildingData.costSilver, buildingData.costWood, buildingData.costStone))
            return false;
        if (ResourceManager.Instance.Happiness < buildingData.requiredHappiness)
            return false;
        if (buildingData.requiredBuilding != null && !BuildingManager.Instance.GetConstructedBuildings().Contains(buildingData.requiredBuilding))
            return false;
        if (buildingData.requireTechUnlock && TechManager.Instance != null && !TechManager.Instance.IsBuildingUnlocked(buildingData))
            return false;
        return true;
    }
    private void RemoveTreesByBounds(GameObject buildingPreview)
    {
        // ��ȡ����Ԥ��ģ�͵������Χ�У��ϲ����� Renderer �� Collider��
        Bounds bounds = GetCombinedBounds(buildingPreview);
        if (bounds.size == Vector3.zero)
        {
            Debug.LogWarning("�޷���ȡ������Χ�У�ʹ��Ĭ�ϰ뾶");
            RemoveTreesOnTerrain(buildingPreview.transform.position, 2f);
            return;
        }

        Terrain[] terrains = Terrain.activeTerrains;
        if (terrains == null || terrains.Length == 0) return;

        bool anyRemoved = false;
        foreach (Terrain terrain in terrains)
        {
            TerrainData terrainData = terrain.terrainData;
            if (terrainData == null) continue;

            TreeInstance[] trees = terrainData.treeInstances;
            if (trees.Length == 0) continue;

            List<TreeInstance> newTrees = new List<TreeInstance>();
            int removedCount = 0;

            foreach (var tree in trees)
            {
                // ����������������
                Vector3 treeWorldPos = terrain.transform.TransformPoint(new Vector3(
                    tree.position.x * terrainData.size.x,
                    0,
                    tree.position.z * terrainData.size.z
                ));
                // �ж����Ƿ��ڽ�����Χ���ڣ�����Y����죬ֻ����XZƽ��ͶӰ��
                if (treeWorldPos.x >= bounds.min.x && treeWorldPos.x <= bounds.max.x &&
                    treeWorldPos.z >= bounds.min.z && treeWorldPos.z <= bounds.max.z)
                {
                    removedCount++;
                    // ��ѡ��־
                    // Debug.Log($"�Ƴ��� at {treeWorldPos}");
                }
                else
                {
                    newTrees.Add(tree);
                }
            }

            if (removedCount > 0)
            {
                terrainData.treeInstances = newTrees.ToArray();
                terrain.Flush();
                anyRemoved = true;
                Debug.Log($"���� {terrain.name} �Ƴ��� {removedCount} ����");
            }
        }

        if (!anyRemoved)
        {
            Debug.Log("δ������Ҫ�Ƴ�����ľ");
        }
    }

    private Bounds GetCombinedBounds(GameObject obj)
    {
        Bounds combinedBounds = new Bounds(obj.transform.position, Vector3.zero);
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
        {
            Collider[] colliders = obj.GetComponentsInChildren<Collider>();
            if (colliders.Length > 0)
            {
                foreach (var col in colliders)
                    combinedBounds.Encapsulate(col.bounds);
            }
            else
            {
                combinedBounds.size = Vector3.one; // Ĭ�ϴ�С
            }
        }
        else
        {
            foreach (Renderer renderer in renderers)
                combinedBounds.Encapsulate(renderer.bounds);
        }
        return combinedBounds;
    }
}