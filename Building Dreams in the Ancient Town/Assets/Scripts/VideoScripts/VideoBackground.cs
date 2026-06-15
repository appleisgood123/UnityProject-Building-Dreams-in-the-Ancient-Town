using UnityEngine;
using UnityEngine.Video;

[RequireComponent(typeof(VideoPlayer))]
public class VideoBackground : MonoBehaviour
{
    [Header("视频组件")]
    public VideoPlayer videoPlayer;

    [Header("显示视频的UI")]
    public UnityEngine.UI.RawImage rawImage;

    private RenderTexture rt;

    void Start()
    {
        // 自动获取组件
        if (videoPlayer == null)
            videoPlayer = GetComponent<VideoPlayer>();

        // 关键：设置视频渲染到 UI
        videoPlayer.renderMode = VideoRenderMode.RenderTexture;

        // 创建运行时 RenderTexture
        rt = new RenderTexture(1920, 1080, 0);
        videoPlayer.targetTexture = rt;
        rawImage.texture = rt;

        // 自动循环播放
        videoPlayer.isLooping = true;
        videoPlayer.Play();
    }

    void OnDestroy()
    {
        if (rt != null)
        {
            rt.Release();
            Destroy(rt);
            rt = null;
        }
    }
}
