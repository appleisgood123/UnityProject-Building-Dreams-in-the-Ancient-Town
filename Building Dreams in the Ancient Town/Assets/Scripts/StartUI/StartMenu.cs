using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class StartMenu : MonoBehaviour
{
    private GameObject settingsOverlay;
    private SliderDragHandler bgmDragHandler;
    private SliderDragHandler sfxDragHandler;
    private TextMeshProUGUI bgmValueText;
    private TextMeshProUGUI sfxValueText;
    private VideoPlayer bgVideoPlayer;

    private LoadPanelController loadPanelController;

    private const string BGM_VOLUME_KEY = "BGM_Volume";
    private const string SFX_VOLUME_KEY = "SFX_Volume";

    void Start()
    {
        // 缓存开始界面背景视频（StartVideo 上的 VideoPlayer）
        bgVideoPlayer = FindObjectOfType<VideoPlayer>();
        // 应用已保存的 BGM 音量到背景视频
        ApplyBGMVolume(PlayerPrefs.GetFloat(BGM_VOLUME_KEY, 0.5f));
        TMP_FontAsset font = Resources.Load<TMP_FontAsset>("Fonts/朱雀仿宋 SDF");

        // 给四个按钮添加黑色文字
        SetupButtonText("StartButton", "开始游戏", font);
        SetupButtonText("LoadButton", "读取存档", font);
        SetupButtonText("SettingButton", "设置", font);
        SetupButtonText("ExitButton", "退出游戏", font);

        // 绑定读档按钮
        GameObject loadBtn = GameObject.Find("LoadButton");
        if (loadBtn != null)
        {
            Button btn = loadBtn.GetComponent<Button>();
            if (btn != null)
                btn.onClick.AddListener(LoadGame);
        }

        // 绑定设置按钮
        GameObject settingBtn = GameObject.Find("SettingButton");
        if (settingBtn != null)
        {
            Button btn = settingBtn.GetComponent<Button>();
            if (btn != null)
                btn.onClick.AddListener(ToggleSettings);
        }

        // 查找 Hierarchy 中已生成的设置面板
        SetupSettingsPanelFromHierarchy();

        // 初始化读档面板
        loadPanelController = gameObject.AddComponent<LoadPanelController>();
    }

    private void SetupButtonText(string buttonName, string text, TMP_FontAsset font)
    {
        GameObject btnObj = GameObject.Find(buttonName);
        if (btnObj == null) return;

        Transform existingText = btnObj.transform.Find("ButtonText");
        if (existingText != null) Destroy(existingText.gameObject);

        GameObject textObj = new GameObject("ButtonText", typeof(RectTransform), typeof(TextMeshProUGUI));
        textObj.transform.SetParent(btnObj.transform, false);

        TextMeshProUGUI tmp = textObj.GetComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = 36;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.black;
        tmp.fontStyle = FontStyles.Bold;
        if (font != null) tmp.font = font;

        RectTransform rect = textObj.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;
    }

    private void SetupSettingsPanelFromHierarchy()
    {
        // 找 Canvas 下的 Overlay
        Canvas canvas = GameObject.Find("Canvas")?.GetComponent<Canvas>();
        if (canvas == null) return;

        Transform overlayT = canvas.transform.Find("Overlay");
        if (overlayT == null)
        {
            Debug.LogWarning("未找到 Overlay，请在 Unity 菜单 Tools > 生成设置面板");
            return;
        }
        settingsOverlay = overlayT.gameObject;

        Transform panelT = overlayT.Find("SettingsPanel");
        if (panelT == null) return;

        // 读取 PlayerPrefs 中的音量
        float bgmVol = PlayerPrefs.GetFloat(BGM_VOLUME_KEY, 0.5f);
        float sfxVol = PlayerPrefs.GetFloat(SFX_VOLUME_KEY, 1f);

        // 初始化 BGM 滑块
        Transform bgmSlider = panelT.Find("BGMSlider");
        if (bgmSlider != null)
        {
            RectTransform bgRect = bgmSlider.GetComponent<RectTransform>();
            Transform fillT = bgmSlider.Find("Fill");
            Transform handleT = bgmSlider.Find("Handle");
            bgmValueText = panelT.Find("BGMValue")?.GetComponent<TextMeshProUGUI>();

            if (fillT != null && handleT != null)
            {
                bgmDragHandler = handleT.gameObject.AddComponent<SliderDragHandler>();
                bgmDragHandler.Init(
                    bgRect,
                    fillT.GetComponent<RectTransform>(),
                    handleT.GetComponent<RectTransform>(),
                    bgmValueText,
                    bgmVol,
                    (val) =>
                    {
                        PlayerPrefs.SetFloat(BGM_VOLUME_KEY, val);
                        PlayerPrefs.Save();
                        ApplyBGMVolume(val);
                    }
                );
                // 设置初始位置
                UpdateSliderVisual(bgRect, fillT.GetComponent<RectTransform>(), handleT.GetComponent<RectTransform>(), bgmValueText, bgmVol);
            }
        }

        // 初始化 SFX 滑块
        Transform sfxSlider = panelT.Find("SFXSlider");
        if (sfxSlider != null)
        {
            RectTransform bgRect = sfxSlider.GetComponent<RectTransform>();
            Transform fillT = sfxSlider.Find("Fill");
            Transform handleT = sfxSlider.Find("Handle");
            sfxValueText = panelT.Find("SFXValue")?.GetComponent<TextMeshProUGUI>();

            if (fillT != null && handleT != null)
            {
                sfxDragHandler = handleT.gameObject.AddComponent<SliderDragHandler>();
                sfxDragHandler.Init(
                    bgRect,
                    fillT.GetComponent<RectTransform>(),
                    handleT.GetComponent<RectTransform>(),
                    sfxValueText,
                    sfxVol,
                    (val) =>
                    {
                        PlayerPrefs.SetFloat(SFX_VOLUME_KEY, val);
                        PlayerPrefs.Save();
                    }
                );
                UpdateSliderVisual(bgRect, fillT.GetComponent<RectTransform>(), handleT.GetComponent<RectTransform>(), sfxValueText, sfxVol);
            }
        }

        // 绑定关闭按钮（Editor 中 lambda 无法序列化，运行时重新绑定）
        Transform closeBtnT = panelT.Find("CloseButton");
        if (closeBtnT != null)
        {
            Button closeBtn = closeBtnT.GetComponent<Button>();
            if (closeBtn != null)
            {
                closeBtn.onClick.RemoveAllListeners();
                closeBtn.onClick.AddListener(() => settingsOverlay.SetActive(false));
            }
        }

        // 初始隐藏
        if (settingsOverlay != null)
            settingsOverlay.SetActive(false);
    }

    private void UpdateSliderVisual(RectTransform bgRect, RectTransform fillRect, RectTransform handleRect, TextMeshProUGUI label, float value)
    {
        float w = bgRect.sizeDelta.x;
        handleRect.anchoredPosition = new Vector2(w * value, 0);
        fillRect.sizeDelta = new Vector2(-w + w * value, 0);
        if (label != null) label.text = Mathf.RoundToInt(value * 100).ToString();
    }

    public void StartGame()
    {
        SaveLoadManager.PendingLoadData = null; // 新游戏不清除存档数据
        StartCoroutine(PlayIntroSequence());
    }

    public void LoadGame()
    {
        if (loadPanelController != null)
            loadPanelController.Show();
    }

    private IEnumerator PlayIntroSequence()
    {
        if (settingsOverlay != null) settingsOverlay.SetActive(false);

        GameObject menuPanel = GameObject.Find("Panel");
        if (menuPanel != null) menuPanel.SetActive(false);

        VideoPlayer bgPlayer = FindObjectOfType<VideoPlayer>();
        if (bgPlayer != null)
        {
            bgPlayer.Stop();
            bgPlayer.enabled = false;
            RawImage bgRawImage = bgPlayer.GetComponent<RawImage>();
            if (bgRawImage == null) bgRawImage = bgPlayer.gameObject.GetComponent<RawImage>();
            if (bgRawImage != null) bgRawImage.enabled = false;
        }

        // 渐黑
        Image fadeImg = CreateFadeOverlay();
        yield return Fade(fadeImg, 0f, 1f, 0.8f);

        // 开始1 和 开始2（视频层级高于黑屏，直接可见）
        yield return PlayVideoOnFade("Video/开始1");
        yield return PlayVideoOnFade("Video/开始2");

        SceneManager.LoadScene("Scene1");
    }

    // ==================== 设置面板控制 ====================

    public void ToggleSettings()
    {
        if (settingsOverlay == null)
        {
            SetupSettingsPanelFromHierarchy();
            if (settingsOverlay == null) return;
        }

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX("普通点击");

        settingsOverlay.SetActive(!settingsOverlay.activeSelf);
    }

    // ====== 视频播放 + 渐黑辅助 ======

    private Image CreateFadeOverlay()
    {
        GameObject go = new GameObject("FadeCanvas", typeof(Canvas), typeof(Image));
        Canvas c = go.GetComponent<Canvas>();
        c.renderMode = RenderMode.ScreenSpaceOverlay;
        c.sortingOrder = 999;
        Image img = go.GetComponent<Image>();
        img.color = new Color(0, 0, 0, 0);
        img.raycastTarget = true;
        RectTransform r = go.GetComponent<RectTransform>();
        r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one;
        r.sizeDelta = Vector2.zero;
        return img;
    }

    private IEnumerator Fade(Image img, float from, float to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            img.color = new Color(0, 0, 0, Mathf.Lerp(from, to, elapsed / duration));
            yield return null;
        }
        img.color = new Color(0, 0, 0, to);
    }

    private IEnumerator PlayVideoOnFade(string resourcePath)
    {
        VideoClip clip = Resources.Load<VideoClip>(resourcePath);
        if (clip == null) { Debug.LogWarning($"未找到视频: {resourcePath}"); yield break; }

        GameObject canvasObj = new GameObject("VideoCanvas", typeof(Canvas));
        Canvas canvas = canvasObj.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;

        GameObject rawObj = new GameObject("RawImage", typeof(RectTransform), typeof(RawImage));
        rawObj.transform.SetParent(canvasObj.transform, false);
        RectTransform rt = rawObj.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.sizeDelta = Vector2.zero;
        RawImage rawImage = rawObj.GetComponent<RawImage>();

        VideoPlayer vp = canvasObj.AddComponent<VideoPlayer>();
        vp.source = VideoSource.VideoClip;
        vp.clip = clip;
        vp.renderMode = VideoRenderMode.RenderTexture;
        vp.audioOutputMode = VideoAudioOutputMode.Direct;

        RenderTexture renderTex = new RenderTexture(1920, 1080, 0);
        vp.targetTexture = renderTex;
        rawImage.texture = renderTex;

        bool finished = false;
        vp.loopPointReached += (source) => finished = true;
        vp.Play();
        yield return new WaitUntil(() => finished);

        vp.Stop();
        renderTex.Release();
        Destroy(renderTex);
        Destroy(canvasObj);
    }

    /// <summary>应用 BGM 音量：开始界面视频 + 保存到 PlayerPrefs（Scene1 的 ResourceManager 启动时读取）</summary>
    private void ApplyBGMVolume(float volume)
    {
        // 调整开始界面循环视频的音量
        if (bgVideoPlayer != null)
            bgVideoPlayer.SetDirectAudioVolume(0, volume);
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}

/// <summary>简易滑块拖拽处理</summary>
internal class SliderDragHandler : MonoBehaviour, UnityEngine.EventSystems.IBeginDragHandler, UnityEngine.EventSystems.IDragHandler
{
    private RectTransform bgRect, fillRect, handleRect;
    private TextMeshProUGUI label;
    private float value;
    private System.Action<float> onValueChanged;
    private float sliderWidth;

    public void Init(RectTransform bg, RectTransform fill, RectTransform handle, TextMeshProUGUI labelTmp, float initialValue, System.Action<float> callback)
    {
        bgRect = bg;
        fillRect = fill;
        handleRect = handle;
        label = labelTmp;
        value = initialValue;
        onValueChanged = callback;
        sliderWidth = bgRect.sizeDelta.x;
    }

    public void OnBeginDrag(UnityEngine.EventSystems.PointerEventData eventData)
    {
        UpdateValue(eventData);
    }

    public void OnDrag(UnityEngine.EventSystems.PointerEventData eventData)
    {
        UpdateValue(eventData);
    }

    private void UpdateValue(UnityEngine.EventSystems.PointerEventData eventData)
    {
        Vector2 localPos;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(bgRect, eventData.position, eventData.pressEventCamera, out localPos))
        {
            float halfW = sliderWidth / 2;
            value = Mathf.Clamp01((localPos.x + halfW) / sliderWidth);
            handleRect.anchoredPosition = new Vector2(sliderWidth * value, 0);
            fillRect.sizeDelta = new Vector2(-sliderWidth + sliderWidth * value, 0);
            label.text = Mathf.RoundToInt(value * 100).ToString();
            onValueChanged?.Invoke(value);
        }
    }
}
