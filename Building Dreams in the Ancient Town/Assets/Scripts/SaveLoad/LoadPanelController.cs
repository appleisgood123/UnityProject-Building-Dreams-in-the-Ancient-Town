using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LoadPanelController : MonoBehaviour
{
    private GameObject loadOverlay;
    private bool isSetup = false;
    private int expandedSlot = -1;

    void Start()
    {
        // 延迟初始化，由 StartMenu.Show() 触发
    }

    public void Show()
    {
        if (!isSetup) SetupLoadPanel();
        if (loadOverlay == null) return;
        RefreshAllSlots();
        loadOverlay.SetActive(true);
    }

    public void Hide()
    {
        if (loadOverlay != null) loadOverlay.SetActive(false);

        if (expandedSlot >= 0 && loadOverlay != null)
        {
            Transform slotT = loadOverlay.transform.Find($"Slot{expandedSlot}");
            if (slotT != null)
            {
                Transform bar = slotT.Find("ActionBar");
                if (bar != null) bar.gameObject.SetActive(false);
            }
            expandedSlot = -1;
        }
    }

    private void SetupLoadPanel()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null) return;

        TMP_FontAsset font = Resources.Load<TMP_FontAsset>("Fonts/朱雀仿宋 SDF");
        Sprite btnSprite = Resources.Load<Sprite>("Pictures/UI/button_ready_off");

        // 全屏遮罩（独立于 Overlay，直接挂在 Canvas 下）
        loadOverlay = NewGO("LoadOverlay", canvas.transform);
        loadOverlay.transform.SetAsLastSibling();
        SetFull(loadOverlay.GetComponent<RectTransform>());
        Image overlayImg = loadOverlay.AddComponent<Image>();
        overlayImg.color = new Color(0, 0, 0, 0.85f);

        // 标题
        MkText("Title", loadOverlay.transform, "读取存档", 42, font, Color.white,
            new Vector2(0, 180), new Vector2(200, 50), true);

        // 3 个槽位
        for (int i = 0; i < SaveLoadManager.MAX_SLOTS; i++)
            CreateSlot(loadOverlay.transform, i, font, btnSprite);

        // 关闭按钮（底部）
        GameObject closeBtn = NewGO("CloseButton", loadOverlay.transform);
        RectTransform cr = closeBtn.GetComponent<RectTransform>();
        cr.anchorMin = cr.anchorMax = new Vector2(0.5f, 0.5f);
        cr.sizeDelta = new Vector2(180, 55);
        cr.anchoredPosition = new Vector2(0, -160);
        Image ci = closeBtn.AddComponent<Image>();
        if (btnSprite != null) ci.sprite = btnSprite; else ci.color = new Color(0.35f, 0.35f, 0.4f);
        Button cbtn = closeBtn.AddComponent<Button>();
        cbtn.targetGraphic = ci;
        cbtn.onClick.AddListener(Hide);

        GameObject closeText = NewGO("Text", closeBtn.transform);
        TextMeshProUGUI ct = closeText.AddComponent<TextMeshProUGUI>();
        ct.text = "关闭"; ct.fontSize = 24; ct.alignment = TextAlignmentOptions.Center;
        ct.color = Color.white; ct.raycastTarget = false;
        if (font != null) ct.font = font;
        RectTransform ctr = ct.GetComponent<RectTransform>();
        ctr.anchorMin = Vector2.zero; ctr.anchorMax = Vector2.one;
        ctr.sizeDelta = Vector2.zero;

        loadOverlay.SetActive(false);
        isSetup = true;
    }

    private void CreateSlot(Transform parent, int slot, TMP_FontAsset font, Sprite btnSprite)
    {
        float yPos = 90 - slot * 90;

        GameObject slotGo = NewGO($"Slot{slot}", parent);
        RectTransform slotR = slotGo.GetComponent<RectTransform>();
        slotR.anchorMin = slotR.anchorMax = new Vector2(0.5f, 0.5f);
        slotR.sizeDelta = new Vector2(420, 60);
        slotR.anchoredPosition = new Vector2(0, yPos);
        Image slotImg = slotGo.AddComponent<Image>();
        slotImg.color = new Color(0.25f, 0.22f, 0.18f, 0.9f);
        Button slotBtn = slotGo.AddComponent<Button>();
        slotBtn.targetGraphic = slotImg;

        // 标签
        GameObject labelGo = NewGO("Label", slotGo.transform);
        TextMeshProUGUI label = labelGo.AddComponent<TextMeshProUGUI>();
        label.fontSize = 24; label.alignment = TextAlignmentOptions.Center; label.color = Color.white;
        label.raycastTarget = false;
        if (font != null) label.font = font;
        RectTransform labelR = label.GetComponent<RectTransform>();
        labelR.anchorMin = Vector2.zero; labelR.anchorMax = Vector2.one;
        labelR.sizeDelta = Vector2.zero;
        UpdateSlotLabel(slot, label);

        int capturedSlot = slot;
        slotBtn.onClick.AddListener(() => OnSlotClicked(capturedSlot));

        // 操作栏（初始隐藏）
        GameObject actionBar = NewGO("ActionBar", slotGo.transform);
        RectTransform abR = actionBar.GetComponent<RectTransform>();
        abR.anchorMin = new Vector2(0, 0); abR.anchorMax = new Vector2(1, 0);
        abR.pivot = new Vector2(0.5f, 0);
        abR.sizeDelta = new Vector2(0, 55);
        abR.anchoredPosition = new Vector2(0, -60);
        actionBar.SetActive(false);

        // 读取存档按钮（居中）
        CreateSmallButton(actionBar.transform, "LoadBtn", "读取存档", Vector2.zero, font, btnSprite,
            () =>
            {
                if (!SaveLoadManager.HasSave(capturedSlot)) return;
                SaveLoadManager.PendingLoadData = SaveLoadManager.LoadSave(capturedSlot);
                UnityEngine.SceneManagement.SceneManager.LoadScene("Scene1");
            });

        // 删除存档按钮
        CreateSmallButton(actionBar.transform, "DeleteBtn", "删除存档", new Vector2(140, 0), font, btnSprite,
            () =>
            {
                SaveLoadManager.DeleteSave(capturedSlot);
                UpdateSlotLabel(capturedSlot, label);
                OnSlotClicked(capturedSlot);
            });
    }

    private void CreateSmallButton(Transform parent, string name, string text, Vector2 pos,
        TMP_FontAsset font, Sprite btnSprite, UnityEngine.Events.UnityAction onClick)
    {
        GameObject go = NewGO(name, parent);
        RectTransform r = go.GetComponent<RectTransform>();
        r.anchorMin = r.anchorMax = new Vector2(0.5f, 0.5f);
        r.sizeDelta = new Vector2(130, 40);
        r.anchoredPosition = pos;
        Image img = go.AddComponent<Image>();
        if (btnSprite != null) img.sprite = btnSprite; else img.color = new Color(0.4f, 0.35f, 0.3f);
        Button btn = go.AddComponent<Button>();
        btn.targetGraphic = img;
        btn.onClick.AddListener(onClick);

        GameObject txtGo = NewGO("Text", go.transform);
        TextMeshProUGUI tmp = txtGo.AddComponent<TextMeshProUGUI>();
        tmp.text = text; tmp.fontSize = 16; tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white; tmp.raycastTarget = false;
        if (font != null) tmp.font = font;
        RectTransform tr = tmp.GetComponent<RectTransform>();
        tr.anchorMin = Vector2.zero; tr.anchorMax = Vector2.one;
        tr.sizeDelta = Vector2.zero;
    }

    private void UpdateSlotLabel(int slot, TextMeshProUGUI label)
    {
        if (SaveLoadManager.HasSave(slot))
        {
            SaveData data = SaveLoadManager.LoadSave(slot);
            label.text = $"档位 {slot + 1}  -  {data.saveTime}";
        }
        else
        {
            label.text = $"档位 {slot + 1}  -  空";
        }
    }

    private void RefreshAllSlots()
    {
        for (int i = 0; i < SaveLoadManager.MAX_SLOTS; i++)
        {
            Transform slotT = loadOverlay.transform.Find($"Slot{i}");
            if (slotT != null)
            {
                Transform labelT = slotT.Find("Label");
                if (labelT != null)
                {
                    TextMeshProUGUI tmp = labelT.GetComponent<TextMeshProUGUI>();
                    if (tmp != null) UpdateSlotLabel(i, tmp);
                }
                Transform bar = slotT.Find("ActionBar");
                if (bar != null) bar.gameObject.SetActive(false);
            }
        }
        expandedSlot = -1;
    }

    private void OnSlotClicked(int slot)
    {
        if (loadOverlay == null) return;

        // 空槽位不展开
        if (!SaveLoadManager.HasSave(slot)) return;

        // 收起之前展开的槽位
        if (expandedSlot >= 0 && expandedSlot != slot)
        {
            Transform oldSlot = loadOverlay.transform.Find($"Slot{expandedSlot}");
            if (oldSlot != null)
            {
                Transform oldBar = oldSlot.Find("ActionBar");
                if (oldBar != null) oldBar.gameObject.SetActive(false);
            }
        }

        Transform slotT = loadOverlay.transform.Find($"Slot{slot}");
        if (slotT == null) return;
        Transform bar = slotT.Find("ActionBar");
        if (bar == null) return;

        bool isNowExpanded = !bar.gameObject.activeSelf;
        bar.gameObject.SetActive(isNowExpanded);
        expandedSlot = isNowExpanded ? slot : -1;
    }

    // ====== 工具方法 ======
    private static GameObject NewGO(string n, Transform p)
    {
        GameObject go = new GameObject(n, typeof(RectTransform));
        go.transform.SetParent(p, false);
        go.layer = p.gameObject.layer;
        return go;
    }
    private static void SetFull(RectTransform r)
    {
        r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one;
        r.sizeDelta = Vector2.zero;
    }
    private static void MkText(string n, Transform p, string t, int s, TMP_FontAsset f,
        Color c, Vector2 pos, Vector2 size, bool full = false)
    {
        GameObject go = NewGO(n, p);
        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = t; tmp.fontSize = s; tmp.alignment = TextAlignmentOptions.Center; tmp.color = c;
        tmp.raycastTarget = false;
        if (f != null) tmp.font = f;
        RectTransform r = go.GetComponent<RectTransform>();
        r.anchorMin = r.anchorMax = new Vector2(0.5f, 0.5f);
        if (full) SetFull(r); else { r.sizeDelta = size; r.anchoredPosition = pos; }
    }
}
