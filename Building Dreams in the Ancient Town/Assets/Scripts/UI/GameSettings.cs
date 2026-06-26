using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>主场景设置：ESC 打开面板，控制音量，存档读档</summary>
public class GameSettings : MonoBehaviour
{
    private GameObject settingsOverlay;
    private GameObject saveOverlay;
    private bool isSetup = false;
    private int expandedSlot = -1;

    private const string BGM_VOLUME_KEY = "BGM_Volume";
    private const string SFX_VOLUME_KEY = "SFX_Volume";

    private AudioSource bgmAudioSource;
    private TextMeshProUGUI[] saveSlotLabels = new TextMeshProUGUI[3];

    void Start()
    {
        if (ResourceManager.Instance != null)
        {
            AudioSource[] sources = ResourceManager.Instance.GetComponents<AudioSource>();
            foreach (var src in sources) { if (src.loop) { bgmAudioSource = src; break; } }
        }
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

    // ==================== 设置面板 ====================

    public void ToggleSettings()
    {
        if (!isSetup) { SetupSettings(); if (!isSetup) return; }

        bool open = !settingsOverlay.activeSelf;
        settingsOverlay.SetActive(open);

        if (open) { Pause(); }
        else { Resume(); }
    }

    private void SetupSettings()
    {
        GameObject canvasGo = GameObject.Find("Canvas");
        Canvas canvas = canvasGo != null ? canvasGo.GetComponent<Canvas>() : null;
        if (canvas == null) return;

        // 先找 Hierarchy 里有没有，没有再动态创建
        Transform overlayT = canvas.transform.Find("Overlay");
        if (overlayT == null)
            overlayT = CreateOverlay(canvas.transform);
        settingsOverlay = overlayT.gameObject;

        Transform panelT = overlayT.Find("SettingsPanel");
        if (panelT == null) return;

        // 同时也找 SaveOverlay
        Transform saveT = canvas.transform.Find("SaveOverlay");
        if (saveT != null)
        {
            saveOverlay = saveT.gameObject;
            Transform frameT = saveT.Find("Frame");
            if (frameT != null)
            {
                for (int i = 0; i < 3; i++)
                {
                    Transform slotT = frameT.Find($"Slot{i}");
                    if (slotT != null)
                    {
                        Transform labelT = slotT.Find("Label");
                        if (labelT != null) saveSlotLabels[i] = labelT.GetComponent<TextMeshProUGUI>();
                        Button slotBtn = slotT.GetComponent<Button>();
                        if (slotBtn != null) { int cap = i; slotBtn.onClick.AddListener(() => OnSaveSlotClicked(cap)); }
                        Transform barT = slotT.Find("ActionBar");
                        if (barT != null)
                        {
                            BindBarBtn(barT, "OverwriteBtn", () => { OverwriteSave(i); OnSaveSlotClicked(i); });
                            BindBarBtn(barT, "LoadBtn", () => LoadSaveInGame(i));
                            BindBarBtn(barT, "DeleteBtn", () => { DeleteSave(i); RefreshSaveLabels(); CollapseSaveSlot(i); if (expandedSlot == i) expandedSlot = -1; });
                        }
                    }
                }
            }
        }

        TMP_FontAsset font = Resources.Load<TMP_FontAsset>("Fonts/朱雀仿宋 SDF");
        float bgmVol = PlayerPrefs.GetFloat(BGM_VOLUME_KEY, 0.5f);
        float sfxVol = PlayerPrefs.GetFloat(SFX_VOLUME_KEY, 1f);

        SetupSlider(panelT, "BGMSlider", "BGMValue", bgmVol, font, val =>
        {
            PlayerPrefs.SetFloat(BGM_VOLUME_KEY, val); PlayerPrefs.Save();
            if (bgmAudioSource != null) bgmAudioSource.volume = val;
        });
        SetupSlider(panelT, "SFXSlider", "SFXValue", sfxVol, font, val =>
        {
            PlayerPrefs.SetFloat(SFX_VOLUME_KEY, val); PlayerPrefs.Save();
        });

        BindBtn(panelT, "SaveGameButton", ShowSaveOverlay);
        BindBtn(panelT, "ExitGameButton", ExitToMenu);

        Transform bgT = overlayT.Find("Background");
        if (bgT != null) { Button bgBtn = bgT.GetComponent<Button>(); if (bgBtn != null) { bgBtn.onClick.RemoveAllListeners(); bgBtn.onClick.AddListener(ToggleSettings); } }

        settingsOverlay.SetActive(false);
        isSetup = true;
    }

    // ==================== 运行时创建 Overlay ====================

    private Transform CreateOverlay(Transform canvasParent)
    {
        TMP_FontAsset font = Resources.Load<TMP_FontAsset>("Fonts/朱雀仿宋 SDF");
        Sprite bgSprite = Resources.Load<Sprite>("Pictures/UI/c6aa96a348de16d46fc9542aa8264d62");
        Sprite btnSprite = Resources.Load<Sprite>("Pictures/UI/button_ready_off");

        GameObject overlay = MkGO("Overlay", canvasParent);
        SetFull(overlay.GetComponent<RectTransform>());

        GameObject bg = MkGO("Background", overlay.transform);
        bg.transform.SetAsFirstSibling();
        RectTransform bgr = bg.GetComponent<RectTransform>();
        bgr.anchorMin = Vector2.zero; bgr.anchorMax = Vector2.one;
        bgr.sizeDelta = Vector2.zero; bgr.anchoredPosition = new Vector2(0, 26);
        bgr.localScale = new Vector3(0.81438f, 0.81438f, 0.81438f);
        Image bgImg = bg.AddComponent<Image>();
        bgImg.color = Color.white;
        if (bgSprite != null) bgImg.sprite = bgSprite;
        bg.AddComponent<Button>();

        GameObject panel = MkGO("SettingsPanel", overlay.transform);
        RectTransform pr = panel.GetComponent<RectTransform>();
        pr.anchorMin = pr.anchorMax = new Vector2(0.5f, 0.5f);
        pr.sizeDelta = new Vector2(650, 500);
        pr.anchoredPosition = Vector2.zero;
        Image pImg = panel.AddComponent<Image>();
        pImg.color = new Color(1, 1, 1, 0);

        MkText("Title", panel.transform, "游戏设置", 40, font, Color.black, new Vector2(0, 224), new Vector2(200, 50));

        MkText("BGMLabel", panel.transform, "背景音乐", 24, font, Color.black, new Vector2(-324, 80), new Vector2(140, 36), new Vector3(2, 2, 2));
        MkSlider("BGMSlider", panel.transform, new Vector2(60, 80), 220, new Vector3(2, 2, 2));
        MkText("BGMValue", panel.transform, "50", 20, font, Color.black, new Vector2(363, 80), new Vector2(50, 30), new Vector3(2, 2, 2));

        MkText("SFXLabel", panel.transform, "音    效", 24, font, Color.black, new Vector2(-324, -22), new Vector2(140, 36), new Vector3(2, 2, 2));
        MkSlider("SFXSlider", panel.transform, new Vector2(58, -22), 220, new Vector3(2, 2, 2));
        MkText("SFXValue", panel.transform, "100", 20, font, Color.black, new Vector2(355, -22), new Vector2(50, 30), new Vector3(2, 2, 2));

        Sprite menuBtnSprite = Resources.Load<Sprite>("Pictures/UI/微信图片_20260314103302_164_12");
        MkBtn("SaveGameButton", panel.transform, "保存游戏", new Vector2(-140, -176), new Vector2(150, 50), new Vector3(1.5f, 1.5f, 1.5f), font, menuBtnSprite, Color.black);
        MkBtn("ExitGameButton", panel.transform, "返回主菜单", new Vector2(140, -176), new Vector2(150, 50), new Vector3(1.5f, 1.5f, 1.5f), font, menuBtnSprite, Color.black);

        return overlay.transform;
    }

    // ==================== 存档面板（运行时创建） ====================

    private void EnsureSaveOverlay()
    {
        if (saveOverlay != null) return;
        GameObject canvasGo = GameObject.Find("Canvas");
        if (canvasGo == null) return;
        Transform canvasT = canvasGo.transform;
        Transform saveT = canvasT.Find("SaveOverlay");
        if (saveT != null) { saveOverlay = saveT.gameObject; return; }

        TMP_FontAsset font = Resources.Load<TMP_FontAsset>("Fonts/朱雀仿宋 SDF");
        Sprite slotBg = Resources.Load<Sprite>("Pictures/UI/图片2 1");

        saveOverlay = MkGO("SaveOverlay", canvasT);
        saveOverlay.transform.SetAsLastSibling();
        SetFull(saveOverlay.GetComponent<RectTransform>());
        Image ovImg = saveOverlay.AddComponent<Image>();
        ovImg.color = new Color(0, 0, 0, 0.6f);

        Sprite menuBtnSprite = Resources.Load<Sprite>("Pictures/UI/微信图片_20260314103302_164_12");
        Sprite frameSprite = Resources.Load<Sprite>("Pictures/UI/cbbe4ffd9dd9853b6fa0a190c17c1862");

        GameObject frame = MkGO("Frame", saveOverlay.transform);
        RectTransform fr = frame.GetComponent<RectTransform>();
        fr.anchorMin = fr.anchorMax = new Vector2(0.5f, 0.5f);
        fr.sizeDelta = new Vector2(1500, 1500);
        fr.anchoredPosition = new Vector2(3.2537f, 0);
        Image frameImg = frame.AddComponent<Image>();
        if (frameSprite != null) { frameImg.sprite = frameSprite; frameImg.type = Image.Type.Simple; frameImg.preserveAspect = true; }
        else frameImg.color = new Color(0.15f, 0.12f, 0.1f, 0.95f);

        MkText("Title", frame.transform, "存档", 44, font, Color.black, new Vector2(0, 175), new Vector2(300, 60));

        float[] slotYs = { 71, -39, -154 };
        for (int i = 0; i < 3; i++)
        {
            GameObject slot = MkGO($"Slot{i}", frame.transform);
            RectTransform sr = slot.GetComponent<RectTransform>();
            sr.anchorMin = sr.anchorMax = new Vector2(0.5f, 0.5f);
            sr.sizeDelta = new Vector2(400, 50);
            sr.anchoredPosition = new Vector2(0, slotYs[i]);
            sr.localScale = new Vector3(1, 1.203125f, 1);
            Image si = slot.AddComponent<Image>();
            if (slotBg != null) si.sprite = slotBg;
            else si.color = new Color(0.25f, 0.22f, 0.18f, 0.9f);
            Button sb = slot.AddComponent<Button>();
            sb.targetGraphic = si;

            MkText("Label", slot.transform, "空", 22, font, Color.black, new Vector2(0, 0), new Vector2(380, 40));

            GameObject bar = MkGO("ActionBar", slot.transform);
            RectTransform br = bar.GetComponent<RectTransform>();
            br.anchorMin = new Vector2(0, 0); br.anchorMax = new Vector2(1, 0);
            br.pivot = new Vector2(0.5f, 0);
            br.sizeDelta = new Vector2(0, 45);
            br.anchoredPosition = new Vector2(0, -48);
            bar.SetActive(false);

            MkBtn("OverwriteBtn", bar.transform, "覆盖存档", new Vector2(-140, 0), new Vector2(120, 38), Vector3.one, font, menuBtnSprite, Color.black);
            MkBtn("LoadBtn", bar.transform, "读取存档", new Vector2(0, 0), new Vector2(120, 38), Vector3.one, font, menuBtnSprite, Color.black);
            MkBtn("DeleteBtn", bar.transform, "删除存档", new Vector2(140, 0), new Vector2(120, 38), Vector3.one, font, menuBtnSprite, Color.black);
        }

        MkBtn("CloseButton", frame.transform, "关闭", new Vector2(-12, -281), new Vector2(160, 50), new Vector3(1.3f, 1.3f, 1.3f), font, menuBtnSprite, Color.black);

        // 绑定槽位按钮
        for (int i = 0; i < 3; i++)
        {
            int cap = i;
            Transform slotT = frame.transform.Find($"Slot{i}");
            if (slotT != null)
            {
                Button sbtn = slotT.GetComponent<Button>();
                if (sbtn != null) sbtn.onClick.AddListener(() => OnSaveSlotClicked(cap));
                saveSlotLabels[i] = slotT.Find("Label")?.GetComponent<TextMeshProUGUI>();

                Transform barT = slotT.Find("ActionBar");
                if (barT != null)
                {
                    BindBarBtn(barT, "OverwriteBtn", () => { OverwriteSave(cap); OnSaveSlotClicked(cap); });
                    BindBarBtn(barT, "LoadBtn", () => LoadSaveInGame(cap));
                    BindBarBtn(barT, "DeleteBtn", () => { DeleteSave(cap); RefreshSaveLabels(); CollapseSaveSlot(cap); if (expandedSlot == cap) expandedSlot = -1; });
                }
            }
        }

        saveOverlay.SetActive(false);
    }

    // ==================== 存档面板操作 ====================

    private void ShowSaveOverlay()
    {
        if (settingsOverlay != null) settingsOverlay.SetActive(false);
        EnsureSaveOverlay();
        if (saveOverlay == null) return;
        RefreshSaveLabels();
        CollapseSaveSlot(expandedSlot);
        expandedSlot = -1;
        saveOverlay.SetActive(true);
    }

    private void HideSaveOverlay()
    {
        if (saveOverlay != null) saveOverlay.SetActive(false);
        CollapseSaveSlot(expandedSlot);
        expandedSlot = -1;
        if (settingsOverlay != null) settingsOverlay.SetActive(true);
    }

    private void RefreshSaveLabels() { for (int i = 0; i < 3; i++) UpdateSaveLabel(i); }

    private void UpdateSaveLabel(int slot)
    {
        if (saveSlotLabels[slot] == null) return;
        if (SaveLoadManager.HasSave(slot))
            saveSlotLabels[slot].text = $"档位 {slot + 1}     {SaveLoadManager.LoadSave(slot).saveTime}";
        else
            saveSlotLabels[slot].text = $"档位 {slot + 1}     空";
    }

    private void OnSaveSlotClicked(int slot)
    {
        if (!SaveLoadManager.HasSave(slot)) { OverwriteSave(slot); UpdateSaveLabel(slot); return; }
        CollapseSaveSlot(expandedSlot);
        Transform frameT = saveOverlay.transform.Find("Frame");
        if (frameT == null) return;
        Transform slotT = frameT.Find($"Slot{slot}");
        if (slotT == null) return;
        Transform bar = slotT.Find("ActionBar");
        if (bar == null) return;
        bool open = !bar.gameObject.activeSelf;
        bar.gameObject.SetActive(open);
        expandedSlot = open ? slot : -1;
    }

    private void CollapseSaveSlot(int slot)
    {
        if (slot < 0 || saveOverlay == null) return;
        Transform frameT = saveOverlay.transform.Find("Frame");
        if (frameT == null) return;
        Transform bar = frameT.Find($"Slot{slot}/ActionBar");
        if (bar != null) bar.gameObject.SetActive(false);
    }

    private void OverwriteSave(int slot) { SaveLoadManager.SaveGame(slot, SaveLoadManager.CaptureCurrentState()); }
    private void LoadSaveInGame(int slot) { if (!SaveLoadManager.HasSave(slot)) return; SaveLoadManager.PendingLoadData = SaveLoadManager.LoadSave(slot); if (GamePauseManager.Instance != null) GamePauseManager.Instance.RequestResume(); if (GameManager.Instance != null) Object.DestroyImmediate(GameManager.Instance.gameObject); UnityEngine.SceneManagement.SceneManager.LoadScene("Scene1"); }
    private void DeleteSave(int slot) { SaveLoadManager.DeleteSave(slot); }
    private void ExitToMenu() { if (GamePauseManager.Instance != null) GamePauseManager.Instance.RequestResume(); if (GameManager.Instance != null) Object.Destroy(GameManager.Instance.gameObject); UnityEngine.SceneManagement.SceneManager.LoadScene("Scene0"); }

    // ==================== 工具方法 ====================

    private void SetupSlider(Transform p, string sn, string vn, float iv, TMP_FontAsset f, System.Action<float> cb)
    {
        Transform st = p.Find(sn); if (st == null) return;
        RectTransform br = st.GetComponent<RectTransform>();
        Transform ft = st.Find("Fill"); Transform ht = st.Find("Handle");
        TextMeshProUGUI lbl = p.Find(vn)?.GetComponent<TextMeshProUGUI>();
        if (ft != null && ht != null)
        {
            SliderDragHandler h = ht.gameObject.AddComponent<SliderDragHandler>();
            h.Init(br, ft.GetComponent<RectTransform>(), ht.GetComponent<RectTransform>(), lbl, iv, cb);
            float w = br.sizeDelta.x; ht.GetComponent<RectTransform>().anchoredPosition = new Vector2(w * iv, 0);
            ft.GetComponent<RectTransform>().sizeDelta = new Vector2(-w + w * iv, 0);
            if (lbl != null) lbl.text = Mathf.RoundToInt(iv * 100).ToString();
        }
    }
    private void BindBtn(Transform p, string n, UnityEngine.Events.UnityAction a) { Transform t = p.Find(n); if (t != null) { Button b = t.GetComponent<Button>(); if (b != null) { b.onClick.RemoveAllListeners(); b.onClick.AddListener(a); } } }
    private void BindBarBtn(Transform bar, string n, UnityEngine.Events.UnityAction a) { Transform t = bar.Find(n); if (t != null) { Button b = t.GetComponent<Button>(); if (b != null) b.onClick.AddListener(a); } }
    private void Pause() { if (GamePauseManager.Instance != null) GamePauseManager.Instance.RequestPause(); if (MouseManager.Instance != null) MouseManager.Instance.SetCursorVisible(true); }
    private void Resume() { if (GamePauseManager.Instance != null) GamePauseManager.Instance.RequestResume(); if (MouseManager.Instance != null) MouseManager.Instance.SetCursorVisible(false); }

    private static GameObject MkGO(string n, Transform p) { GameObject go = new GameObject(n, typeof(RectTransform)); go.transform.SetParent(p, false); go.layer = p.gameObject.layer; return go; }
    private static void SetFull(RectTransform r) { r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one; r.sizeDelta = Vector2.zero; }
    private static void MkText(string n, Transform p, string t, int s, TMP_FontAsset f, Color c, Vector2 pos, Vector2 size, Vector3? scale = null, bool full = false)
    {
        GameObject go = MkGO(n, p); TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = t; tmp.fontSize = s; tmp.alignment = TextAlignmentOptions.Center; tmp.color = c; tmp.raycastTarget = false;
        if (f != null) tmp.font = f;
        RectTransform r = go.GetComponent<RectTransform>(); r.anchorMin = r.anchorMax = new Vector2(0.5f, 0.5f);
        if (scale.HasValue) r.localScale = scale.Value;
        if (full) SetFull(r); else { r.sizeDelta = size; r.anchoredPosition = pos; }
    }
    private static void MkSlider(string n, Transform p, Vector2 pos, float w, Vector3 scale)
    {
        GameObject bg = MkGO(n, p); RectTransform br = bg.GetComponent<RectTransform>();
        br.anchorMin = br.anchorMax = new Vector2(0.5f, 0.5f); br.sizeDelta = new Vector2(w, 12); br.anchoredPosition = pos; br.localScale = scale;
        bg.AddComponent<Image>().color = new Color(0.7f, 0.65f, 0.55f, 0.6f);
        GameObject fill = MkGO("Fill", bg.transform); RectTransform fr = fill.GetComponent<RectTransform>();
        fr.anchorMin = Vector2.zero; fr.anchorMax = Vector2.one; fr.pivot = new Vector2(0, 0.5f); fr.sizeDelta = Vector2.zero;
        fill.AddComponent<Image>().color = new Color(0.7f, 0.4f, 0.2f, 0.8f);
        GameObject handle = MkGO("Handle", bg.transform); RectTransform hr = handle.GetComponent<RectTransform>();
        hr.anchorMin = hr.anchorMax = new Vector2(0, 0.5f); hr.sizeDelta = new Vector2(18, 26); hr.anchoredPosition = new Vector2(w * 0.5f, 0);
        handle.AddComponent<Image>().color = new Color(0.55f, 0.3f, 0.15f);
    }
    private static void MkBtn(string n, Transform p, string t, Vector2 pos, Vector2 size, Vector3 scale, TMP_FontAsset f, Sprite s, Color tc)
    {
        GameObject go = MkGO(n, p); RectTransform r = go.GetComponent<RectTransform>();
        r.anchorMin = r.anchorMax = new Vector2(0.5f, 0.5f); r.sizeDelta = size; r.anchoredPosition = pos; r.localScale = scale;
        Image img = go.AddComponent<Image>(); if (s != null) img.sprite = s; else img.color = new Color(0.35f, 0.35f, 0.4f);
        Button btn = go.AddComponent<Button>(); btn.targetGraphic = img;
        MkText("Text", go.transform, t, 20, f, tc, Vector2.zero, Vector2.zero, null, true);
    }
}
