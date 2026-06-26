using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

/// <summary>Scene0 / Scene1 打开时自动生成设置面板</summary>
[InitializeOnLoad]
public static class AutoCreateSettingsUI
{
    private static string DoneKey => $"AutoCreateSettingsUI_Done_{SceneManager.GetActiveScene().name}";

    static AutoCreateSettingsUI()
    {
        EditorSceneManager.sceneOpened += OnSceneOpened;
    }

    private static void OnSceneOpened(Scene scene, OpenSceneMode mode)
    {
        if (scene.name != "Scene0") return;
        string key = $"AutoCreateSettingsUI_Done_{scene.name}";
        if (EditorPrefs.GetBool(key, false)) return;

        EditorApplication.delayCall += () =>
        {
            CreateSettingsUI();
            EditorPrefs.SetBool(key, true);
        };
    }

    private static void CreateSettingsUI()
    {
        Canvas canvas = GameObject.Find("Canvas")?.GetComponent<Canvas>();
        if (canvas == null)
        {
            Debug.LogWarning("AutoCreateSettingsUI: 未找到 Canvas");
            return;
        }

        // 检查是否已存在
        if (canvas.transform.Find("Overlay") != null)
        {
            Debug.Log("AutoCreateSettingsUI: Overlay 已存在，跳过");
            return;
        }

        Undo.IncrementCurrentGroup();
        int undoGroup = Undo.GetCurrentGroup();

        TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/朱雀仿宋 SDF.asset");
        Sprite bgSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/Pictures/UI/c6aa96a348de16d46fc9542aa8264d62.png");
        Sprite btnSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/Pictures/UI/button_agree.png");

        // === Overlay ===
        GameObject overlay = CreateGO("Overlay", canvas.transform);
        Undo.RegisterCreatedObjectUndo(overlay, "Create Settings UI");
        SetFullStretch(overlay.GetComponent<RectTransform>());
        // Overlay 自身不加 Button，避免拦截子元素的拖拽事件

        // 背景遮罩层（放最底层，点空白处关闭）
        GameObject background = CreateGO("Background", overlay.transform);
        background.transform.SetAsFirstSibling();
        Undo.RegisterCreatedObjectUndo(background, "Create Settings UI");
        SetFullStretch(background.GetComponent<RectTransform>());
        Image bgImg = background.AddComponent<Image>();
        bgImg.color = new Color(0, 0, 0, 0.5f);
        Button bgBtn = background.AddComponent<Button>();
        bgBtn.onClick.AddListener(() => overlay.SetActive(false));

        // === SettingsPanel ===
        GameObject panel = CreateGO("SettingsPanel", overlay.transform);
        Undo.RegisterCreatedObjectUndo(panel, "Create Settings UI");
        RectTransform pr = panel.GetComponent<RectTransform>();
        pr.anchorMin = pr.anchorMax = new Vector2(0.5f, 0.5f);
        pr.sizeDelta = new Vector2(650, 500);
        pr.anchoredPosition = Vector2.zero;
        Image pImg = panel.AddComponent<Image>();
        if (bgSprite != null) { pImg.sprite = bgSprite; pImg.type = Image.Type.Simple; pImg.preserveAspect = true; }
        else pImg.color = new Color(0.15f, 0.15f, 0.18f, 0.95f);

        // 标题
        CreateTMP("Title", panel.transform, "设  置", 40, font, new Vector2(0, 190), new Vector2(200, 50));
        // 分隔线
        {
            GameObject d = CreateGO("Divider", panel.transform);
            var dr = d.GetComponent<RectTransform>();
            dr.anchorMin = dr.anchorMax = new Vector2(0.5f, 0.5f);
            dr.sizeDelta = new Vector2(500, 2);
            dr.anchoredPosition = new Vector2(0, 155);
            d.AddComponent<Image>().color = new Color(0.5f, 0.4f, 0.3f, 0.4f);
            Undo.RegisterCreatedObjectUndo(d, "Create Settings UI");
        }
        // BGM行
        CreateTMP("BGMLabel", panel.transform, "背景音乐", 24, font, new Vector2(-150, 80), new Vector2(140, 36));
        CreateSlider("BGMSlider", panel.transform, new Vector2(60, 80), 220);
        CreateTMP("BGMValue", panel.transform, "50", 20, font, new Vector2(210, 80), new Vector2(50, 30));
        // SFX行
        CreateTMP("SFXLabel", panel.transform, "音    效", 24, font, new Vector2(-150, 15), new Vector2(140, 36));
        CreateSlider("SFXSlider", panel.transform, new Vector2(60, 15), 220);
        CreateTMP("SFXValue", panel.transform, "100", 20, font, new Vector2(210, 15), new Vector2(50, 30));
        // 关闭按钮
        {
            GameObject cb = CreateGO("CloseButton", panel.transform);
            RectTransform cr = cb.GetComponent<RectTransform>();
            cr.anchorMin = cr.anchorMax = new Vector2(0.5f, 0.5f);
            cr.sizeDelta = new Vector2(140, 50);
            cr.anchoredPosition = new Vector2(0, -60);
            Image ci = cb.AddComponent<Image>();
            if (btnSprite != null) ci.sprite = btnSprite; else ci.color = new Color(0.35f, 0.35f, 0.4f);
            Button cbtn = cb.AddComponent<Button>();
            cbtn.targetGraphic = ci;
            cbtn.onClick.AddListener(() => overlay.SetActive(false));
            Undo.RegisterCreatedObjectUndo(cb, "Create Settings UI");
            CreateTMP("Text", cb.transform, "关闭", 22, font, Vector2.zero, Vector2.zero, true);
        }

        // 初始隐藏
        overlay.SetActive(false);

        Undo.CollapseUndoOperations(undoGroup);
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Debug.Log("✅ 设置面板已自动生成在 Canvas/Overlay 下。如需重新生成：先删掉 Overlay，再点击菜单 Tools/重置设置面板");
    }

    // ============ Helper ============
    private static GameObject CreateGO(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        go.layer = parent.gameObject.layer;
        return go;
    }
    private static void SetFullStretch(RectTransform r)
    {
        r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one;
        r.sizeDelta = Vector2.zero; r.anchoredPosition = Vector2.zero;
    }
    private static void CreateTMP(string name, Transform parent, string text, int size, TMP_FontAsset font, Vector2 pos, Vector2 s, bool full = false)
    {
        GameObject go = CreateGO(name, parent);
        Undo.RegisterCreatedObjectUndo(go, "Create Settings UI");
        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text; tmp.fontSize = size; tmp.alignment = TextAlignmentOptions.Center; tmp.color = Color.black;
        if (font != null) tmp.font = font;
        RectTransform r = go.GetComponent<RectTransform>();
        r.anchorMin = r.anchorMax = new Vector2(0.5f, 0.5f);
        if (full) SetFullStretch(r); else { r.sizeDelta = s; r.anchoredPosition = pos; }
    }
    private static void CreateSlider(string name, Transform parent, Vector2 pos, float w)
    {
        GameObject bg = CreateGO(name, parent);
        Undo.RegisterCreatedObjectUndo(bg, "Create Settings UI");
        RectTransform br = bg.GetComponent<RectTransform>();
        br.anchorMin = br.anchorMax = new Vector2(0.5f, 0.5f);
        br.sizeDelta = new Vector2(w, 12); br.anchoredPosition = pos;
        bg.AddComponent<Image>().color = new Color(0.7f, 0.65f, 0.55f, 0.6f);

        GameObject fill = CreateGO("Fill", bg.transform);
        Undo.RegisterCreatedObjectUndo(fill, "Create Settings UI");
        RectTransform fr = fill.GetComponent<RectTransform>();
        fr.anchorMin = Vector2.zero; fr.anchorMax = Vector2.one;
        fr.pivot = new Vector2(0, 0.5f); fr.sizeDelta = Vector2.zero;
        fill.AddComponent<Image>().color = new Color(0.7f, 0.4f, 0.2f, 0.8f);

        GameObject handle = CreateGO("Handle", bg.transform);
        Undo.RegisterCreatedObjectUndo(handle, "Create Settings UI");
        RectTransform hr = handle.GetComponent<RectTransform>();
        hr.anchorMin = hr.anchorMax = new Vector2(0, 0.5f);
        hr.sizeDelta = new Vector2(18, 26); hr.anchoredPosition = new Vector2(w * 0.5f, 0);
        handle.AddComponent<Image>().color = new Color(0.55f, 0.3f, 0.15f);
    }

    // 强制生成（忽略标记）
    [MenuItem("Tools/生成设置面板(强制)")]
    private static void ForceCreate()
    {
        string key = $"AutoCreateSettingsUI_Done_{SceneManager.GetActiveScene().name}";
        EditorPrefs.DeleteKey(key);
        CreateSettingsUI();
    }

    // 重置菜单
    [MenuItem("Tools/重置设置面板生成标记")]
    private static void ResetFlag()
    {
        string key = $"AutoCreateSettingsUI_Done_{SceneManager.GetActiveScene().name}";
        EditorPrefs.DeleteKey(key);
        Debug.Log($"已重置 {SceneManager.GetActiveScene().name} 的标记。重新打开场景将重新生成。");
    }
}
