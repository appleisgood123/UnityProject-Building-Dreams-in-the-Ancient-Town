using UnityEngine;
using UnityEngine.Video;

[RequireComponent(typeof(VideoPlayer))]
public class VideoBackground : MonoBehaviour
{
    [Header("视频播放器")]
    public VideoPlayer videoPlayer;

    [Header("显示视频的UI")]
    public UnityEngine.UI.RawImage rawImage;

    void Start()
    {
        // 自动获取组件
        if (videoPlayer == null)
            videoPlayer = GetComponent<VideoPlayer>();

        // 关键：把视频渲染到 UI 上
        videoPlayer.renderMode = VideoRenderMode.RenderTexture;

        // 创建临时纹理（不用手动新建 RenderTexture）
        RenderTexture rt = new RenderTexture(1920, 1080, 0);
        videoPlayer.targetTexture = rt;
        rawImage.texture = rt;

        // 自动循环播放
        videoPlayer.isLooping = true;
        videoPlayer.Play();
    }
}