using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

/// <summary>Scene0 打开时自动在 Hierarchy 中生成读取存档面板</summary>
[InitializeOnLoad]
public static class AutoCreateLoadPanel
{
    private static string DoneKey => $"AutoCreateLoadPanel_Done_{SceneManager.GetActiveScene().name}";

    static AutoCreateLoadPanel()
    {
        EditorSceneManager.sceneOpened += OnSceneOpened;
    }

    private static void OnSceneOpened(Scene scene, OpenSceneMode mode)
    {
        if (scene.name != "Scene0") return;
        if (EditorPrefs.GetBool(DoneKey, false)) return;

        EditorApplication.delayCall += () =>
        {
            CreateLoadPanel();
            EditorPrefs.SetBool(DoneKey, true);
        };
    }

    private static void CreateLoadPanel()
    {
        Canvas canvas = GameObject.Find("Canvas")?.GetComponent<Canvas>();
        if (canvas == null) { Debug.LogWarning("未找到 Canvas"); return; }
        if (canvas.transform.Find("LoadOverlay") != null) { Debug.Log("LoadOverlay 已存在，跳过"); return; }

        Undo.IncrementCurrentGroup();
        int undoGroup = Undo.GetCurrentGroup();

        TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/朱雀仿宋 SDF.asset");
        Sprite panelFrame = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/Pictures/UI/c6aa96a348de16d46fc9542aa8264d62.png");
        Sprite btnSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/Pictures/UI/button_ready_off.png");
        Sprite slotBg = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/Pictures/UI/name_bar2.png");

        // === LoadOverlay ===
        GameObject overlay = MkGO("LoadOverlay", canvas.transform);
        Undo.RegisterCreatedObjectUndo(overlay, "Create LoadPanel");
        SetFull(overlay.GetComponent<RectTransform>());
        Image ovImg = overlay.AddComponent<Image>();
        ovImg.color = new Color(0, 0, 0, 0.6f);

        // === Frame ===
        GameObject frame = MkGO("Frame", overlay.transform);
        Undo.RegisterCreatedObjectUndo(frame, "Create LoadPanel");
        RectTransform fr = frame.GetComponent<RectTransform>();
        fr.anchorMin = fr.anchorMax = new Vector2(0.5f, 0.5f);
        fr.sizeDelta = new Vector2(600, 520);
        fr.anchoredPosition = Vector2.zero;
        Image frameImg = frame.AddComponent<Image>();
        if (panelFrame != null) { frameImg.sprite = panelFrame; frameImg.type = Image.Type.Simple; frameImg.preserveAspect = true; }
        else frameImg.color = new Color(0.15f, 0.12f, 0.1f, 0.95f);

        // 标题
        MkText("Title", frame.transform, "读取存档", 44, font, Color.black, new Vector2(0, 200), new Vector2(300, 60));

        // 分隔线
        {
            GameObject d = MkGO("Divider", frame.transform);
            Undo.RegisterCreatedObjectUndo(d, "Create LoadPanel");
            RectTransform dr = d.GetComponent<RectTransform>();
            dr.anchorMin = dr.anchorMax = new Vector2(0.5f, 0.5f);
            dr.sizeDelta = new Vector2(480, 2);
            dr.anchoredPosition = new Vector2(0, 155);
            d.AddComponent<Image>().color = new Color(0.4f, 0.3f, 0.2f, 0.35f);
        }

        // 3 个槽位
        for (int i = 0; i < 3; i++)
        {
            float yPos = 90 - i * 105;
            GameObject slot = MkGO($"Slot{i}", frame.transform);
            Undo.RegisterCreatedObjectUndo(slot, "Create LoadPanel");
            RectTransform sr = slot.GetComponent<RectTransform>();
            sr.anchorMin = sr.anchorMax = new Vector2(0.5f, 0.5f);
            sr.sizeDelta = new Vector2(460, 70);
            sr.anchoredPosition = new Vector2(0, yPos);
            Image si = slot.AddComponent<Image>();
            if (slotBg != null) si.sprite = slotBg;
            else si.color = new Color(0.25f, 0.22f, 0.18f, 0.9f);
            Button sb = slot.AddComponent<Button>();
            sb.targetGraphic = si;

            // 标签
            MkText("Label", slot.transform, $"档位 {i + 1}     空", 22, font, Color.black,
                new Vector2(0, 0), new Vector2(420, 40));

            // 操作栏
            GameObject bar = MkGO("ActionBar", slot.transform);
            Undo.RegisterCreatedObjectUndo(bar, "Create LoadPanel");
            RectTransform br = bar.GetComponent<RectTransform>();
            br.anchorMin = new Vector2(0, 0); br.anchorMax = new Vector2(1, 0);
            br.pivot = new Vector2(0.5f, 0);
            br.sizeDelta = new Vector2(0, 45);
            br.anchoredPosition = new Vector2(0, -48);
            bar.SetActive(false);

            // 读取按钮
            MkBtnWithText("LoadBtn", bar.transform, "读取存档", new Vector2(-110, 0), new Vector2(140, 38), btnSprite, font);
            // 删除按钮
            MkBtnWithText("DeleteBtn", bar.transform, "删除存档", new Vector2(110, 0), new Vector2(140, 38), btnSprite, font);
        }

        // 关闭按钮
        {
            GameObject cb = MkGO("CloseButton", frame.transform);
            Undo.RegisterCreatedObjectUndo(cb, "Create LoadPanel");
            RectTransform cr = cb.GetComponent<RectTransform>();
            cr.anchorMin = cr.anchorMax = new Vector2(0.5f, 0.5f);
            cr.sizeDelta = new Vector2(160, 50);
            cr.anchoredPosition = new Vector2(0, -210);
            cr.localScale = new Vector3(1.3f, 1.3f, 1.3f);
            Image ci = cb.AddComponent<Image>();
            if (btnSprite != null) ci.sprite = btnSprite;
            else ci.color = new Color(0.35f, 0.35f, 0.4f);
            Button cbtn = cb.AddComponent<Button>();
            cbtn.targetGraphic = ci;
            MkText("Text", cb.transform, "关闭", 22, font, Color.white, Vector2.zero, Vector2.zero, true);
        }

        overlay.SetActive(false);

        Undo.CollapseUndoOperations(undoGroup);
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Debug.Log("✅ 读取存档面板已生成在 Canvas/LoadOverlay 下");
    }

    // ====== 工具 ======
    private static GameObject MkGO(string n, Transform p)
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
    private static void MkText(string n, Transform p, string t, int s, TMP_FontAsset f, Color c, Vector2 pos, Vector2 size, bool full = false)
    {
        GameObject go = MkGO(n, p);
        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = t; tmp.fontSize = s; tmp.alignment = TextAlignmentOptions.Center; tmp.color = c;
        tmp.raycastTarget = false;
        if (f != null) tmp.font = f;
        RectTransform r = go.GetComponent<RectTransform>();
        r.anchorMin = r.anchorMax = new Vector2(0.5f, 0.5f);
        if (full) SetFull(r); else { r.sizeDelta = size; r.anchoredPosition = pos; }
    }
    private static void MkBtnWithText(string n, Transform p, string t, Vector2 pos, Vector2 size, Sprite btnSprite, TMP_FontAsset font)
    {
        GameObject go = MkGO(n, p);
        RectTransform r = go.GetComponent<RectTransform>();
        r.anchorMin = r.anchorMax = new Vector2(0.5f, 0.5f);
        r.sizeDelta = size; r.anchoredPosition = pos;
        Image img = go.AddComponent<Image>();
        if (btnSprite != null) img.sprite = btnSprite;
        else img.color = new Color(0.4f, 0.35f, 0.3f);
        Button btn = go.AddComponent<Button>();
        btn.targetGraphic = img;
        MkText("Text", go.transform, t, 16, font, Color.white, Vector2.zero, Vector2.zero, true);
    }

    [MenuItem("Tools/生成读取存档面板(强制)")]
    private static void ForceCreate()
    {
        EditorPrefs.DeleteKey(DoneKey);
        CreateLoadPanel();
    }
}
