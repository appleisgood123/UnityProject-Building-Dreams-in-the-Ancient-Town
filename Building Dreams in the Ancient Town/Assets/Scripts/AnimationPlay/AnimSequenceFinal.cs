using UnityEngine;
using System.Collections.Generic;

// 类名保持你原来的！不要改！
public class AnimSequenceFinal : MonoBehaviour
{
    // 你场景里自带的 Animation 组件
    private Animation anim;

    // 当前播放第几个动画
    private int currentIndex = 0;

    // 缓存所有动画名称，避免 O(n²) 遍历
    private List<string> clipNames = new List<string>();

    void Start()
    {
        // 获取物体上自带的 Animation
        anim = GetComponent<Animation>();

        // 关闭自动播放，我们自己控制
        anim.playAutomatically = false;

        // 一次性缓存所有动画名称
        foreach (AnimationState state in anim)
            clipNames.Add(state.name);

        // 开始播放第一个
        PlayNextAnimation();
    }

    void PlayNextAnimation()
    {
        // 如果已经播完所有动画 → 停止
        if (currentIndex >= clipNames.Count)
        {
            Debug.Log("✅ 全部动画播放完毕！");
            anim.Stop();
            return;
        }

        // 获取第 currentIndex 个动画的名称（O(1) 索引）
        string clipName = clipNames[currentIndex];

        // 播放它
        anim.Play(clipName);

        // 等这个动画播完，再播放下一个
        Invoke(nameof(PlayNextAnimation), anim[clipName].length);

        // 序号+1
        currentIndex++;
    }
}
