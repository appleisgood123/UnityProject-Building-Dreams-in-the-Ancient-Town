using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>统一音效管理器，挂载在 ResourceManager 同物体上</summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    private Dictionary<string, AudioClip> clipCache = new Dictionary<string, AudioClip>();

    /// <summary>获取当前 SFX 音量（0-1），从 PlayerPrefs 读取</summary>
    public static float SFXVolume
    {
        get { return PlayerPrefs.GetFloat("SFX_Volume", 1f); }
    }

    /// <summary>获取当前 BGM 音量（0-1），从 PlayerPrefs 读取</summary>
    public static float BGMVolume
    {
        get { return PlayerPrefs.GetFloat("BGM_Volume", 0.5f); }
    }

    private void Awake()
    {
        if (Instance != null) { Destroy(this); return; }
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    /// <summary>播放指定音效（从 Resources/Audio/ 加载），音量受设置控制</summary>
    public void PlaySFX(string clipName)
    {
        AudioClip clip = GetClip(clipName);
        if (clip == null)
        {
            Debug.LogWarning($"音效未找到: {clipName}");
            return;
        }
        StartCoroutine(PlaySFXCoroutine(clip, SFXVolume));
    }

    private IEnumerator PlaySFXCoroutine(AudioClip clip, float volume)
    {
        GameObject tempGO = new GameObject("SFX_" + clip.name);
        tempGO.transform.position = Camera.main != null ? Camera.main.transform.position : Vector3.zero;
        AudioSource source = tempGO.AddComponent<AudioSource>();
        source.clip = clip;
        source.volume = volume;
        source.Play();
        yield return new WaitForSeconds(clip.length + 0.1f);
        Destroy(tempGO);
    }

    /// <summary>获取音效 Clip（带缓存）</summary>
    public AudioClip GetClip(string clipName)
    {
        if (!clipCache.TryGetValue(clipName, out AudioClip clip))
        {
            clip = Resources.Load<AudioClip>($"Audio/{clipName}");
            if (clip != null) clipCache[clipName] = clip;
        }
        return clip;
    }
}
