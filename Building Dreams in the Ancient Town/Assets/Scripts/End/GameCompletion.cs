using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using System.Collections;

public class GameCompletion : MonoBehaviour
{
    [Header("完成条件")]
    public float fadeDuration = 2f;
    public int targetHappiness = 300;

    private bool hasTriggered = false;

    private void Start()
    {
        if (ResourceManager.Instance != null)
            ResourceManager.Instance.OnResourcesChanged += CheckHappiness;
    }

    private void OnDestroy()
    {
        if (ResourceManager.Instance != null)
            ResourceManager.Instance.OnResourcesChanged -= CheckHappiness;
    }

    private void CheckHappiness()
    {
        if (hasTriggered) return;
        if (ResourceManager.Instance != null && ResourceManager.Instance.Happiness >= targetHappiness)
        {
            hasTriggered = true;
            StartCoroutine(PlayEndingAndQuit());
        }
    }

    private IEnumerator PlayEndingAndQuit()
    {
        // 暂停游戏
        if (GamePauseManager.Instance != null)
            GamePauseManager.Instance.RequestPause();

        // 先短暂黑屏过渡
        yield return StartCoroutine(FadeToBlack(fadeDuration));

        // 播放结束视频
        yield return PlayVideo("Video/结束");

        // 退出游戏
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private IEnumerator FadeToBlack(float duration)
    {
        GameObject fadeCanvas = new GameObject("FadeCanvas");
        Canvas canvas = fadeCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;

        GameObject panel = new GameObject("BlackPanel");
        panel.transform.SetParent(canvas.transform, false);
        Image image = panel.AddComponent<Image>();
        image.color = new Color(0, 0, 0, 0);
        image.raycastTarget = false;

        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float alpha = Mathf.Lerp(0f, 1f, elapsed / duration);
            image.color = new Color(0, 0, 0, alpha);
            yield return null;
        }

        // 保持黑屏不销毁（作为视频背景）
    }

    /// <summary>播放指定路径的视频，等待播完后自动清理</summary>
    private IEnumerator PlayVideo(string resourcePath)
    {
        VideoClip clip = Resources.Load<VideoClip>(resourcePath);
        if (clip == null)
        {
            Debug.LogWarning($"未找到视频: {resourcePath}");
            yield break;
        }

        // 创建全屏 Canvas（排序层级高于黑屏）
        GameObject canvasObj = new GameObject("EndVideoCanvas", typeof(Canvas));
        Canvas canvas = canvasObj.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;

        GameObject rawImageObj = new GameObject("RawImage", typeof(RectTransform), typeof(RawImage));
        rawImageObj.transform.SetParent(canvasObj.transform, false);

        RectTransform rectTransform = rawImageObj.GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.sizeDelta = Vector2.zero;

        RawImage rawImage = rawImageObj.GetComponent<RawImage>();

        VideoPlayer vp = canvasObj.AddComponent<VideoPlayer>();
        vp.source = VideoSource.VideoClip;
        vp.clip = clip;
        vp.renderMode = VideoRenderMode.RenderTexture;
        vp.audioOutputMode = VideoAudioOutputMode.Direct;

        RenderTexture rt = new RenderTexture(1920, 1080, 0);
        vp.targetTexture = rt;
        rawImage.texture = rt;

        bool finished = false;
        vp.loopPointReached += (source) => finished = true;
        vp.Play();

        yield return new WaitUntil(() => finished);

        // 清理
        rt.Release();
        Destroy(rt);
        Destroy(canvasObj);
    }
}
