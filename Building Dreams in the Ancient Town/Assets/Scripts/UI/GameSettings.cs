using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>主场景设置：ESC 键打开/关闭设置面板，控制 BGM/SFX 音量</summary>
public class GameSettings : MonoBehaviour
{
    private GameObject settingsOverlay;
    private bool isSetup = false;

    private const string BGM_VOLUME_KEY = "BGM_Volume";
    private const string SFX_VOLUME_KEY = "SFX_Volume";

    private AudioSource bgmAudioSource;

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
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            ToggleSettings();
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
        cr.anchoredPosition = new Vector2(0, -176);
        cr.localScale = new Vector3(1.5f, 1.5f, 1.5f);
        Image ci = cb.AddComponent<Image>();
        if (btnSprite != null) ci.sprite = btnSprite; else ci.color = new Color(0.35f, 0.35f, 0.4f);
        Button cbtn = cb.AddComponent<Button>();
        cbtn.targetGraphic = ci;
        MkText("Text", cb.transform, "关闭", 22, font, Color.white, Vector2.zero, Vector2.zero, true);

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
}
