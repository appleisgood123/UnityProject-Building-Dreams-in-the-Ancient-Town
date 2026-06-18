using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>主场景设置：ESC 键打开/关闭设置面板，控制 BGM/SFX 音量，存档和退出</summary>
public class GameSettings : MonoBehaviour
{
    private GameObject settingsOverlay;
    private GameObject saveOverlay;
    private bool isSetup = false;

    private const string BGM_VOLUME_KEY = "BGM_Volume";
    private const string SFX_VOLUME_KEY = "SFX_Volume";
    private const string SAVE_COUNT_KEY = "SaveCount_Slot";

    private AudioSource bgmAudioSource;
    private int[] saveCounters = new int[SaveLoadManager.MAX_SLOTS];
    private int expandedSlot = -1;

    void Start()
    {
        if (ResourceManager.Instance != null)
        {
            AudioSource[] sources = ResourceManager.Instance.GetComponents<AudioSource>();
            foreach (var src in sources)
            {
                if (src.loop) { bgmAudioSource = src; break; }
            }
        }
        for (int i = 0; i < SaveLoadManager.MAX_SLOTS; i++)
            saveCounters[i] = PlayerPrefs.GetInt(SAVE_COUNT_KEY + i, 1);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (saveOverlay != null && saveOverlay.activeSelf)
                HideSaveOverlay();
            else
                ToggleSettings();
        }
    }

    public void ToggleSettings()
    {
        if (!isSetup)
        {
            SetupSettings();
            if (!isSetup) return;
        }

        bool willOpen = !settingsOverlay.activeSelf;
        settingsOverlay.SetActive(willOpen);

        if (willOpen)
        {
            if (GamePauseManager.Instance != null)
                GamePauseManager.Instance.RequestPause();
            if (MouseManager.Instance != null)
                MouseManager.Instance.SetCursorVisible(true);
        }
        else
        {
            if (GamePauseManager.Instance != null)
                GamePauseManager.Instance.RequestResume();
            if (MouseManager.Instance != null)
                MouseManager.Instance.SetCursorVisible(false);
        }
    }

    private void SetupSettings()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null) return;

        Transform overlayT = canvas.transform.Find("Overlay");
        if (overlayT == null)
            overlayT = CreateOverlay(canvas.transform);
        settingsOverlay = overlayT.gameObject;

        Transform panelT = overlayT.Find("SettingsPanel");
        if (panelT == null) return;

        TMP_FontAsset font = Resources.Load<TMP_FontAsset>("Fonts/朱雀仿宋 SDF");

        float bgmVol = PlayerPrefs.GetFloat(BGM_VOLUME_KEY, 0.5f);
        float sfxVol = PlayerPrefs.GetFloat(SFX_VOLUME_KEY, 1f);

        SetupSlider(panelT, "BGMSlider", "BGMValue", bgmVol, font, (val) =>
        {
            PlayerPrefs.SetFloat(BGM_VOLUME_KEY, val); PlayerPrefs.Save();
            if (bgmAudioSource != null) bgmAudioSource.volume = val;
        });

        SetupSlider(panelT, "SFXSlider", "SFXValue", sfxVol, font, (val) =>
        {
            PlayerPrefs.SetFloat(SFX_VOLUME_KEY, val); PlayerPrefs.Save();
        });

        // 关闭按钮
        Transform closeBtnT = panelT.Find("CloseButton");
        if (closeBtnT != null)
        {
            Button closeBtn = closeBtnT.GetComponent<Button>();
            if (closeBtn != null)
            {
                closeBtn.onClick.RemoveAllListeners();
                closeBtn.onClick.AddListener(ToggleSettings);
            }
        }

        // 背景点击关闭
        Transform bgT = overlayT.Find("Background");
        if (bgT != null)
        {
            Button bgBtn = bgT.GetComponent<Button>();
            if (bgBtn != null)
            {
                bgBtn.onClick.RemoveAllListeners();
                bgBtn.onClick.AddListener(ToggleSettings);
            }
        }

        // 绑定"保存游戏"按钮 → 打开独立存档面板
        Transform saveBtnT = panelT.Find("SaveGameButton");
        if (saveBtnT != null)
        {
            Button saveBtn = saveBtnT.GetComponent<Button>();
            if (saveBtn != null)
            {
                saveBtn.onClick.RemoveAllListeners();
                saveBtn.onClick.AddListener(ShowSaveOverlay);
            }
        }

        // 绑定"返回主菜单"按钮
        Transform exitBtnT = panelT.Find("ExitGameButton");
        if (exitBtnT != null)
        {
            Button exitBtn = exitBtnT.GetComponent<Button>();
            if (exitBtn != null)
            {
                exitBtn.onClick.RemoveAllListeners();
                exitBtn.onClick.AddListener(OnExitToMenu);
            }
        }

        settingsOverlay.SetActive(false);
        isSetup = true;
    }

    private Transform CreateOverlay(Transform canvasParent)
    {
        TMP_FontAsset font = Resources.Load<TMP_FontAsset>("Fonts/朱雀仿宋 SDF");
        Sprite bgSprite = Resources.Load<Sprite>("Pictures/UI/c6aa96a348de16d46fc9542aa8264d62");
        Sprite btnSprite = Resources.Load<Sprite>("Pictures/UI/button_ready_off");

        // === Overlay ===
        GameObject overlay = NewGO("Overlay", canvasParent);
        SetFull(overlay.GetComponent<RectTransform>());

        // === Background 遮罩（全屏背景图） ===
        GameObject background = NewGO("Background", overlay.transform);
        background.transform.SetAsFirstSibling();
        RectTransform bgRect = background.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero; bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;
        bgRect.anchoredPosition = new Vector2(0, 26);
        bgRect.localScale = new Vector3(0.81438f, 0.81438f, 0.81438f);
        Image bgImg = background.AddComponent<Image>();
        bgImg.color = Color.white;
        if (bgSprite != null) bgImg.sprite = bgSprite;
        background.AddComponent<Button>();

        // === SettingsPanel（透明容器，只用于布局和阻挡点击） ===
        GameObject panel = NewGO("SettingsPanel", overlay.transform);
        RectTransform pr = panel.GetComponent<RectTransform>();
        pr.anchorMin = pr.anchorMax = new Vector2(0.5f, 0.5f);
        pr.sizeDelta = new Vector2(650, 500);
        pr.anchoredPosition = Vector2.zero;
        Image pImg = panel.AddComponent<Image>();
        pImg.color = new Color(1, 1, 1, 0);   // 完全透明，只做 raycast 阻挡

        // 标题
        MkText("Title", panel.transform, "游戏设置", 40, font, new Vector2(0, 224), new Vector2(200, 50));

        // BGM 行
        MkText("BGMLabel", panel.transform, "背景音乐", 24, font, new Vector2(-324, 80), new Vector2(140, 36), new Vector3(2, 2, 2));
        MkSlider("BGMSlider", panel.transform, new Vector2(60, 80), 220, new Vector3(2, 2, 2));
        MkText("BGMValue", panel.transform, "50", 20, font, new Vector2(363, 80), new Vector2(50, 30), new Vector3(2, 2, 2));

        // SFX 行
        MkText("SFXLabel", panel.transform, "音    效", 24, font, new Vector2(-324, -22), new Vector2(140, 36), new Vector3(2, 2, 2));
        MkSlider("SFXSlider", panel.transform, new Vector2(58, -22), 220, new Vector3(2, 2, 2));
        MkText("SFXValue", panel.transform, "100", 20, font, new Vector2(355, -22), new Vector2(50, 30), new Vector3(2, 2, 2));

        // 关闭按钮
        GameObject cb = NewGO("CloseButton", panel.transform);
        RectTransform cr = cb.GetComponent<RectTransform>();
        cr.anchorMin = cr.anchorMax = new Vector2(0.5f, 0.5f);
        cr.sizeDelta = new Vector2(150, 50);
        cr.anchoredPosition = new Vector2(-90, -176);
        cr.localScale = new Vector3(1.2f, 1.2f, 1.2f);
        Image ci = cb.AddComponent<Image>();
        if (btnSprite != null) ci.sprite = btnSprite; else ci.color = new Color(0.35f, 0.35f, 0.4f);
        Button cbtn = cb.AddComponent<Button>();
        cbtn.targetGraphic = ci;
        MkText("Text", cb.transform, "关闭", 22, font, Color.white, Vector2.zero, Vector2.zero, true);

        // 保存游戏按钮
        CreatePanelButton(panel.transform, "SaveGameButton", "保存游戏", new Vector2(90, -176), new Vector3(1.2f, 1.2f, 1.2f), font, btnSprite);

        // 返回主菜单按钮
        CreatePanelButton(panel.transform, "ExitGameButton", "返回主菜单", new Vector2(0, -220), new Vector3(1.0f, 1.0f, 1.0f), font, btnSprite);

        return overlay.transform;
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
    private static void MkText(string n, Transform p, string t, int s, TMP_FontAsset f, Vector2 pos, Vector2 size, bool full = false)
    {
        MkText(n, p, t, s, f, Color.black, pos, size, Vector3.one, full);
    }
    private static void MkText(string n, Transform p, string t, int s, TMP_FontAsset f, Vector2 pos, Vector2 size, Vector3 scale, bool full = false)
    {
        MkText(n, p, t, s, f, Color.black, pos, size, scale, full);
    }
    private static void MkText(string n, Transform p, string t, int s, TMP_FontAsset f, Color c, Vector2 pos, Vector2 size, bool full = false)
    {
        MkText(n, p, t, s, f, c, pos, size, Vector3.one, full);
    }
    private static void MkText(string n, Transform p, string t, int s, TMP_FontAsset f, Color c, Vector2 pos, Vector2 size, Vector3 scale, bool full = false)
    {
        GameObject go = NewGO(n, p);
        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = t; tmp.fontSize = s; tmp.alignment = TextAlignmentOptions.Center; tmp.color = c;
        tmp.raycastTarget = false;
        if (f != null) tmp.font = f;
        RectTransform r = go.GetComponent<RectTransform>();
        r.anchorMin = r.anchorMax = new Vector2(0.5f, 0.5f);
        r.localScale = scale;
        if (full) SetFull(r); else { r.sizeDelta = size; r.anchoredPosition = pos; }
    }
    private static void MkSlider(string n, Transform p, Vector2 pos, float w, Vector3 scale)
    {
        GameObject bg = NewGO(n, p);
        RectTransform br = bg.GetComponent<RectTransform>();
        br.anchorMin = br.anchorMax = new Vector2(0.5f, 0.5f);
        br.sizeDelta = new Vector2(w, 12); br.anchoredPosition = pos;
        br.localScale = scale;
        bg.AddComponent<Image>().color = new Color(0.7f, 0.65f, 0.55f, 0.6f);

        GameObject fill = NewGO("Fill", bg.transform);
        RectTransform fr = fill.GetComponent<RectTransform>();
        fr.anchorMin = Vector2.zero; fr.anchorMax = Vector2.one;
        fr.pivot = new Vector2(0, 0.5f); fr.sizeDelta = Vector2.zero;
        fill.AddComponent<Image>().color = new Color(0.7f, 0.4f, 0.2f, 0.8f);

        GameObject handle = NewGO("Handle", bg.transform);
        RectTransform hr = handle.GetComponent<RectTransform>();
        hr.anchorMin = hr.anchorMax = new Vector2(0, 0.5f);
        hr.sizeDelta = new Vector2(18, 26); hr.anchoredPosition = new Vector2(w * 0.5f, 0);
        handle.AddComponent<Image>().color = new Color(0.55f, 0.3f, 0.15f);
    }

    private void SetupSlider(Transform panelT, string sliderName, string valueName, float initialValue, TMP_FontAsset font, System.Action<float> onValueChanged)
    {
        Transform sliderT = panelT.Find(sliderName);
        if (sliderT == null) return;
        RectTransform bgRect = sliderT.GetComponent<RectTransform>();
        Transform fillT = sliderT.Find("Fill");
        Transform handleT = sliderT.Find("Handle");
        TextMeshProUGUI valueText = panelT.Find(valueName)?.GetComponent<TextMeshProUGUI>();

        if (fillT != null && handleT != null)
        {
            SliderDragHandler handler = handleT.gameObject.AddComponent<SliderDragHandler>();
            handler.Init(bgRect, fillT.GetComponent<RectTransform>(), handleT.GetComponent<RectTransform>(), valueText, initialValue, onValueChanged);
            UpdateSliderVisual(bgRect, fillT.GetComponent<RectTransform>(), handleT.GetComponent<RectTransform>(), valueText, initialValue);
        }
    }

    private void UpdateSliderVisual(RectTransform bgRect, RectTransform fillRect, RectTransform handleRect, TextMeshProUGUI label, float value)
    {
        float w = bgRect.sizeDelta.x;
        handleRect.anchoredPosition = new Vector2(w * value, 0);
        fillRect.sizeDelta = new Vector2(-w + w * value, 0);
        if (label != null) label.text = Mathf.RoundToInt(value * 100).ToString();
    }

    // ====== 独立存档面板 ======

    private void EnsureSaveOverlay()
    {
        if (saveOverlay != null) return;
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null) return;

        TMP_FontAsset font = Resources.Load<TMP_FontAsset>("Fonts/朱雀仿宋 SDF");
        Sprite btnSprite = Resources.Load<Sprite>("Pictures/UI/button_ready_off");

        saveOverlay = NewGO("SaveOverlay", canvas.transform);
        saveOverlay.transform.SetAsLastSibling();
        SetFull(saveOverlay.GetComponent<RectTransform>());
        Image overlayImg = saveOverlay.AddComponent<Image>();
        overlayImg.color = new Color(0, 0, 0, 0.85f);

        // 标题
        MkText("Title", saveOverlay.transform, "存档", 40, font, Color.white, new Vector2(0, 180), new Vector2(200, 50), true);

        // 返回按钮（左上角）
        GameObject backBtn = NewGO("BackButton", saveOverlay.transform);
        RectTransform backR = backBtn.GetComponent<RectTransform>();
        backR.anchorMin = new Vector2(0, 1); backR.anchorMax = new Vector2(0, 1);
        backR.pivot = new Vector2(0, 1);
        backR.sizeDelta = new Vector2(120, 45);
        backR.anchoredPosition = new Vector2(30, -30);
        Image backImg = backBtn.AddComponent<Image>();
        if (btnSprite != null) backImg.sprite = btnSprite; else backImg.color = new Color(0.3f, 0.3f, 0.35f);
        Button backButton = backBtn.AddComponent<Button>();
        backButton.targetGraphic = backImg;
        backButton.onClick.AddListener(HideSaveOverlay);
        GameObject backText = NewGO("Text", backBtn.transform);
        TextMeshProUGUI backTmp = backText.AddComponent<TextMeshProUGUI>();
        backTmp.text = "← 返回"; backTmp.fontSize = 22; backTmp.alignment = TextAlignmentOptions.Center;
        backTmp.color = Color.white; backTmp.raycastTarget = false;
        if (font != null) backTmp.font = font;
        RectTransform backTR = backTmp.GetComponent<RectTransform>();
        backTR.anchorMin = Vector2.zero; backTR.anchorMax = Vector2.one;
        backTR.sizeDelta = Vector2.zero;

        // 3 个槽位
        for (int i = 0; i < SaveLoadManager.MAX_SLOTS; i++)
            CreateSaveSlot(saveOverlay.transform, i, font, btnSprite);

        saveOverlay.SetActive(false);
    }

    private void CreateSaveSlot(Transform parent, int slot, TMP_FontAsset font, Sprite btnSprite)
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

        // 标签文字
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
        slotBtn.onClick.AddListener(() => OnSaveSlotClicked(capturedSlot));

        // 操作栏（初始隐藏）
        GameObject actionBar = NewGO("ActionBar", slotGo.transform);
        RectTransform abR = actionBar.GetComponent<RectTransform>();
        abR.anchorMin = new Vector2(0, 0); abR.anchorMax = new Vector2(1, 0);
        abR.pivot = new Vector2(0.5f, 0);
        abR.sizeDelta = new Vector2(0, 55);
        abR.anchoredPosition = new Vector2(0, -60);
        actionBar.SetActive(false);

        CreateSmallButton(actionBar.transform, "OverwriteBtn", "覆盖存档", new Vector2(-140, 0), font, btnSprite,
            () => { OnOverwriteSave(capturedSlot); OnSaveSlotClicked(capturedSlot); });

        CreateSmallButton(actionBar.transform, "LoadBtn", "读取存档", new Vector2(0, 0), font, btnSprite,
            () => OnLoadSaveInGame(capturedSlot));

        CreateSmallButton(actionBar.transform, "DeleteBtn", "删除存档", new Vector2(140, 0), font, btnSprite,
            () => { OnDeleteSaveInGame(capturedSlot); UpdateSlotLabel(capturedSlot, label); OnSaveSlotClicked(capturedSlot); });
    }

    private void CreateSmallButton(Transform parent, string name, string text, Vector2 pos, TMP_FontAsset font, Sprite btnSprite, UnityEngine.Events.UnityAction onClick)
    {
        GameObject go = NewGO(name, parent);
        RectTransform r = go.GetComponent<RectTransform>();
        r.anchorMin = r.anchorMax = new Vector2(0.5f, 0.5f);
        r.sizeDelta = new Vector2(120, 40);
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
            label.text = $"档位 {slot + 1}  -  空白";
        }
    }

    private void OnSaveSlotClicked(int slot)
    {
        EnsureSaveOverlay();
        if (saveOverlay == null) return;

        // 空槽位：直接保存
        if (!SaveLoadManager.HasSave(slot))
        {
            OnOverwriteSave(slot);
            // 刷新该槽位标签显示时间
            Transform slotT = saveOverlay.transform.Find($"Slot{slot}");
            if (slotT != null)
            {
                Transform labelT = slotT.Find("Label");
                if (labelT != null)
                {
                    TextMeshProUGUI tmp = labelT.GetComponent<TextMeshProUGUI>();
                    if (tmp != null) UpdateSlotLabel(slot, tmp);
                }
            }
            return;
        }

        // 有存档：展开/收起操作栏
        // 收起之前展开的槽位
        if (expandedSlot >= 0 && expandedSlot != slot)
        {
            Transform oldSlot = saveOverlay.transform.Find($"Slot{expandedSlot}");
            if (oldSlot != null)
            {
                Transform oldBar = oldSlot.Find("ActionBar");
                if (oldBar != null) oldBar.gameObject.SetActive(false);
            }
        }

        Transform slotT2 = saveOverlay.transform.Find($"Slot{slot}");
        if (slotT2 == null) return;
        Transform bar = slotT2.Find("ActionBar");
        if (bar == null) return;

        bool isNowExpanded = !bar.gameObject.activeSelf;
        bar.gameObject.SetActive(isNowExpanded);
        expandedSlot = isNowExpanded ? slot : -1;
    }

    private void OnOverwriteSave(int slot)
    {
        SaveData data = SaveLoadManager.CaptureCurrentState();
        SaveLoadManager.SaveGame(slot, data);
        saveCounters[slot]++;
        PlayerPrefs.SetInt(SAVE_COUNT_KEY + slot, saveCounters[slot]);
        PlayerPrefs.Save();

        // 验证存档内容
        Debug.Log($"[存档] 档位{slot+1}: 玩家位置=({data.playerPosX:F1},{data.playerPosY:F1},{data.playerPosZ:F1}), 建筑数={data.buildings.Count}, 员工数={data.employees.Count}, 资源银两={data.silver}");

        EnsureSaveOverlay();
        if (saveOverlay != null)
        {
            Transform slotT = saveOverlay.transform.Find($"Slot{slot}");
            if (slotT != null)
            {
                Transform labelT = slotT.Find("Label");
                if (labelT != null)
                {
                    TextMeshProUGUI tmp = labelT.GetComponent<TextMeshProUGUI>();
                    if (tmp != null) UpdateSlotLabel(slot, tmp);
                }
            }
        }
        Debug.Log($"游戏已保存到档位 {slot + 1}");
    }

    private void OnLoadSaveInGame(int slot)
    {
        if (!SaveLoadManager.HasSave(slot)) return;
        SaveLoadManager.PendingLoadData = SaveLoadManager.LoadSave(slot);

        if (GamePauseManager.Instance != null)
            GamePauseManager.Instance.RequestResume();

        // DestroyImmediate ensures DontDestroyOnLoad objects are fully
        // destroyed before Scene1 reloads, preventing Awake race condition
        if (GameManager.Instance != null)
            Object.DestroyImmediate(GameManager.Instance.gameObject);

        UnityEngine.SceneManagement.SceneManager.LoadScene("Scene1");
    }

    private void OnDeleteSaveInGame(int slot)
    {
        SaveLoadManager.DeleteSave(slot);
        saveCounters[slot] = 1;
        PlayerPrefs.SetInt(SAVE_COUNT_KEY + slot, 1);
        PlayerPrefs.Save();
    }

    private void ShowSaveOverlay()
    {
        if (settingsOverlay != null)
            settingsOverlay.SetActive(false);

        EnsureSaveOverlay();
        if (saveOverlay == null) return;

        for (int i = 0; i < SaveLoadManager.MAX_SLOTS; i++)
        {
            Transform slotT = saveOverlay.transform.Find($"Slot{i}");
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
        saveOverlay.SetActive(true);
    }

    private void HideSaveOverlay()
    {
        if (saveOverlay != null)
            saveOverlay.SetActive(false);

        if (expandedSlot >= 0 && saveOverlay != null)
        {
            Transform slotT = saveOverlay.transform.Find($"Slot{expandedSlot}");
            if (slotT != null)
            {
                Transform bar = slotT.Find("ActionBar");
                if (bar != null) bar.gameObject.SetActive(false);
            }
            expandedSlot = -1;
        }

        if (settingsOverlay != null)
            settingsOverlay.SetActive(true);
    }

    // ====== 通用按钮创建 ======

    private void CreatePanelButton(Transform parent, string name, string text, Vector2 pos, Vector3 scale, TMP_FontAsset font, Sprite btnSprite)
    {
        GameObject go = NewGO(name, parent);
        RectTransform r = go.GetComponent<RectTransform>();
        r.anchorMin = r.anchorMax = new Vector2(0.5f, 0.5f);
        r.sizeDelta = new Vector2(150, 50);
        r.anchoredPosition = pos;
        r.localScale = scale;
        Image img = go.AddComponent<Image>();
        if (btnSprite != null) img.sprite = btnSprite; else img.color = new Color(0.35f, 0.35f, 0.4f);
        Button btn = go.AddComponent<Button>();
        btn.targetGraphic = img;
        MkText("Text", go.transform, text, 20, font, Color.white, Vector2.zero, Vector2.zero, true);
    }

    // ====== 退出 ======

    private void OnExitToMenu()
    {
        if (GamePauseManager.Instance != null)
            GamePauseManager.Instance.RequestResume();
        if (GameManager.Instance != null)
            Object.Destroy(GameManager.Instance.gameObject);
        UnityEngine.SceneManagement.SceneManager.LoadScene("Scene0");
    }
}
