using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DraggableIcon : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("建筑数据（由 BuildingPageManager 自动赋值）")]
    public BuildingDataSO buildingData;

    [Header("UI 面板（用于检测鼠标离开建造面板）")]
    public RectTransform uiPanelRect;

    [Header("放置设置")]
    public LayerMask groundLayer = 1 << 8;
    public Material validPlacementMaterial;
    public Material invalidPlacementMaterial;

    [Header("碰撞检测")]
    public LayerMask blockingLayers = ~0;   // 阻挡层（请排除地面层）

    [Header("音效")]
    public AudioClip placementSound;

    [Header("特效")]
    public GameObject placementEffect;
    public float effectDuration = 2f;

    [Header("确认/取消/旋转面板")]
    public GameObject confirmPanel;
    public bool followModelPosition = true;
    public Vector2 panelScreenOffset = new Vector2(0, 100);

    [Header("旋转设置")]
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
        if (confirmPanel != null)
        {
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
    }

    private void OnConfirmClicked()
    {
        if (previewInstance == null || !canPlace || buildingData == null) return;

        // 传递位置和旋转角度
        if (BuildingManager.Instance.ConstructBuilding(buildingData, previewInstance.transform.position, previewInstance.transform.rotation))
        {
            if (placementSound != null)
                AudioSource.PlayClipAtPoint(placementSound, previewInstance.transform.position);
            if (placementEffect != null)
            {
                GameObject effect = Instantiate(placementEffect, previewInstance.transform.position, Quaternion.identity);
                Destroy(effect, effectDuration);
            }
        }
        else
        {
            Debug.LogWarning("建造条件不足！");
        }
        EndDragCleanup();
    }

    private void OnCancelClicked()
    {
        EndDragCleanup();
    }

    private void OnRotateClicked()
    {
        if (previewInstance != null)
        {
            previewInstance.transform.Rotate(Vector3.up, rotateStep, Space.World);
        }
    }

    private void UpdatePreviewPositionAndCheckPlacement(Vector2 screenPos)
    {
        if (previewInstance == null) return;

        Ray ray = mainCamera.ScreenPointToRay(screenPos);
        RaycastHit hit;
        bool hitSomething = Physics.Raycast(ray, out hit, 1000f);

        if (hitSomething)
        {
            previewInstance.transform.position = hit.point;

            // 对于忽略地面检查的建筑，不检查地面层和重叠
            if (buildingData.ignoreGroundCheck)
            {
                // 只检查建造条件（资源、科技等），位置任意
                canPlace = BuildingManager.Instance.CanBuild(buildingData);
            }
            else
            {
                bool onGround = ((1 << hit.collider.gameObject.layer) & groundLayer) != 0;
                bool canBuild = BuildingManager.Instance.CanBuild(buildingData);
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
}