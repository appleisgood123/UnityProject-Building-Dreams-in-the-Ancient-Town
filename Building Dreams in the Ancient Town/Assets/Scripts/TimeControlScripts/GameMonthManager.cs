using UnityEngine;
using TMPro;

public class GameMonthManager : MonoBehaviour
{
    [Header("时间参数")]
    [Tooltip("现实1秒对应游戏内多少个月（基础速率）")]
    [SerializeField] private float baseMonthPerSecond = 1f / 60f; // 1倍速：60秒 = 1个月
    [SerializeField] private float timeScale = 1f;                // 当前倍速
    [SerializeField] private float totalMonths = 0f;              // 游戏内已过总月数（浮点）

    [Header("倍速范围")]
    [SerializeField] private float minTimeScale = 0f;
    [SerializeField] private float maxTimeScale = 64f;

    [Header("循环倍速选项")]
    [SerializeField] private float[] speedCycleOptions = new float[] { 1f, 2f, 4f, 8f, 16f, 32f };

    [Header("UI 显示")]
    [SerializeField] private TextMeshProUGUI timeDisplay;         // 显示游戏内时间的文本
    [SerializeField] private TextMeshProUGUI speedButtonText;     // 倍速按钮上的文本组件（仅用于显示）

    [Header("每月天数（用于计算日/时，可选）")]
    [SerializeField] private int daysPerMonth = 30;
    [SerializeField] private int hoursPerDay = 24;
    [SerializeField] private int minutesPerHour = 60;
    [SerializeField] private int secondsPerMinute = 60;

    // 事件：每帧时间前进时触发，参数为增加的月数（浮点数）
    public event System.Action<float> OnMonthUpdated;

    // 当前帧游戏时间增量（月）
    public float DeltaMonths { get; private set; }

    // 总月数（原始值）
    public float TotalMonths => totalMonths;

    // 原始年份和月份（0基）
    public int Years => Mathf.FloorToInt(totalMonths / 12f);
    public int Months => Mathf.FloorToInt(totalMonths % 12f);

    // 显示用的年份和月份（第1年1月对应 totalMonths = 0）
    public int DisplayYears => Mathf.FloorToInt(totalMonths / 12f) + 1;
    public int DisplayMonths => Mathf.FloorToInt(totalMonths % 12f) + 1;

    // 日、时、分、秒（基于小数月）
    public int Days => Mathf.FloorToInt((totalMonths - Mathf.Floor(totalMonths)) * daysPerMonth);
    public int Hours => Mathf.FloorToInt(((totalMonths - Mathf.Floor(totalMonths)) * daysPerMonth - Days) * hoursPerDay);
    public int Minutes => Mathf.FloorToInt((((totalMonths - Mathf.Floor(totalMonths)) * daysPerMonth - Days) * hoursPerDay - Hours) * minutesPerHour);
    public int Seconds => Mathf.FloorToInt((((((totalMonths - Mathf.Floor(totalMonths)) * daysPerMonth - Days) * hoursPerDay - Hours) * minutesPerHour - Minutes) * secondsPerMinute));

    // 单例
    public static GameMonthManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        UpdateSpeedButtonText();

        // 清除 UI 焦点，确保空格键立即生效
        if (UnityEngine.EventSystems.EventSystem.current != null)
            UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
    }

    private void Update()
    {
        // 按空格键循环切换倍速
        if (Input.GetKeyDown(KeyCode.Space))
        {
            CycleSpeed();
        }

        float deltaMonths = Time.deltaTime * timeScale * baseMonthPerSecond;
        totalMonths += deltaMonths;
        DeltaMonths = deltaMonths;
        OnMonthUpdated?.Invoke(deltaMonths);

        if (timeDisplay != null)
        {
            timeDisplay.text = FormatTime();
        }
    }

    /// <summary> 设置倍速（自动钳制），并更新按钮文字 </summary>
    public void SetTimeScale(float newScale)
    {
        timeScale = Mathf.Clamp(newScale, minTimeScale, maxTimeScale);
        UpdateSpeedButtonText();
    }

    /// <summary> 循环切换倍速 </summary>
    public void CycleSpeed()
    {
        if (speedCycleOptions == null || speedCycleOptions.Length == 0)
            return;

        int currentIndex = -1;
        for (int i = 0; i < speedCycleOptions.Length; i++)
        {
            if (Mathf.Approximately(timeScale, speedCycleOptions[i]))
            {
                currentIndex = i;
                break;
            }
        }

        int nextIndex = currentIndex >= 0
            ? (currentIndex + 1) % speedCycleOptions.Length
            : 0;

        SetTimeScale(speedCycleOptions[nextIndex]);
    }

    /// <summary> 更新倍速按钮上的文字（×1, ×2, ×0.5 等）</summary>
    private void UpdateSpeedButtonText()
    {
        if (speedButtonText != null)
        {
            if (Mathf.Approximately(timeScale, Mathf.Round(timeScale)))
            {
                speedButtonText.text = $"×{Mathf.RoundToInt(timeScale)}";
            }
            else
            {
                speedButtonText.text = $"×{timeScale:F1}";
            }
        }
    }

    /// <summary> 直接设置当前总月数 </summary>
    public void SetTotalMonths(float months)
    {
        totalMonths = Mathf.Max(0, months);
    }

    /// <summary> 通过年和月设置时间（从第1年1月开始）</summary>
    public void SetTime(int displayYears, int displayMonths)
    {
        totalMonths = (displayYears - 1) * 12 + (displayMonths - 1);
    }

    /// <summary> 格式化显示时间 </summary>
    public string FormatTime(bool showDetailed = false)
    {
        if (showDetailed)
        {
            return $"第{DisplayYears}年 {DisplayMonths}月 {Days}天 {Hours:D2}:{Minutes:D2}:{Seconds:D2}";
        }
        else
        {
            return $"第{DisplayYears}年 {DisplayMonths}月";
        }
    }

    // ---------- 快捷倍速设置（可选，供其他脚本调用）----------
    public void SetSpeedNormal() => SetTimeScale(1f);
    public void SetSpeed2x() => SetTimeScale(2f);
    public void SetSpeed4x() => SetTimeScale(4f);
    public void SetSpeed8x() => SetTimeScale(8f);
    public void SetSpeed16x() => SetTimeScale(16f);
    public void SetSpeed32x() => SetTimeScale(32f);
    public void SetSpeedHalf() => SetTimeScale(0.5f);
    public void SetSpeed0() => SetTimeScale(0f);
}