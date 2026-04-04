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
        if (pauseCount == 1)
        {
            Time.timeScale = 0f;
            Debug.Log("”Œœ∑‘›Õ£");
        }
    }

    public void RequestResume()
    {
        pauseCount--;
        if (pauseCount <= 0)
        {
            pauseCount = 0;
            Time.timeScale = 1f;
            Debug.Log("”Œœ∑ª÷∏¥");
        }
    }

    public bool IsPaused => pauseCount > 0;
}