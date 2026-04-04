using UnityEngine;
using UnityEngine.SceneManagement;

public class StartMenu : MonoBehaviour
{
    // 开始游戏
    public void StartGame()
    {
        // 这里改成你真正的游戏场景名称
        SceneManager.LoadScene("Scene1");
    }

    // 退出游戏
    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
        }
}