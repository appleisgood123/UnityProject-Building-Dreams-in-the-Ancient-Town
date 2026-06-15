using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TutorialPanelController : MonoBehaviour
{
    [Header("动画设置")]
    public float slideDownDuration = 0.5f;      // 滑下动画时间
    public float visibleDuration = 10f;          // 停留时间
    public float fadeOutDuration = 1f;           // 淡出时间

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Vector2 startPos;
    private Vector2 targetPos;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        // 起始位置：屏幕顶部外（假设 Canvas 锚点为全屏，坐标原点为中心）
        startPos = new Vector2(0, Screen.height + rectTransform.rect.height);
        targetPos = Vector2.zero; // 屏幕中央

        rectTransform.anchoredPosition = startPos;
        canvasGroup.alpha = 1f;
    }

    private void Start()
    {
        StartCoroutine(PlayTutorial());
    }

    private IEnumerator PlayTutorial()
    {
        // 1. 滑下
        float elapsed = 0f;
        while (elapsed < slideDownDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / slideDownDuration;
            rectTransform.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
            yield return null;
        }
        rectTransform.anchoredPosition = targetPos;

        // 2. 等待停留时间
        yield return new WaitForSeconds(visibleDuration);

        // 3. 淡出消失
        elapsed = 0f;
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeOutDuration);
            yield return null;
        }
        canvasGroup.alpha = 0f;

        // 4. 销毁或隐藏面板
        Destroy(gameObject);
        // 或者 gameObject.SetActive(false);
    }
}