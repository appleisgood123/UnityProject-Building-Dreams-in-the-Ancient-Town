using UnityEngine;

public class GamePauseManager : MonoBehaviour
{
    public static GamePauseManager Instance;

    private int pauseCount = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void RequestPause()
    {
        pauseCount++;
        UnityEngine.Debug.Log($"RequestPause: pauseCount={pauseCount}");
        if (pauseCount == 1)
        {
            Time.timeScale = 0f;
        }
    }

    public void RequestResume()
    {
        if (pauseCount <= 0)
        {
            UnityEngine.Debug.LogWarning("RequestResume called but pauseCount already 0");
            return;
        }
        pauseCount--;
        UnityEngine.Debug.Log($"RequestResume: pauseCount={pauseCount}");
        if (pauseCount == 0)
        {
            Time.timeScale = 1f;
        }
    }

    // 强制恢复游戏（用于异常情况）
    public void ForceResume()
    {
        pauseCount = 0;
        Time.timeScale = 1f;
        UnityEngine.Debug.Log("ForceResume: 强制恢复游戏");
    }

    public bool IsPaused => pauseCount > 0;
}