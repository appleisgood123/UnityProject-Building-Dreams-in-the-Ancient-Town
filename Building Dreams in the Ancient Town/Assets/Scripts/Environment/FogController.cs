using UnityEngine;

public class FogController : MonoBehaviour
{
    public GameObject fogObject;          // 拖拽雾气物体
    public int requiredHappiness = 100;   // 幸福度阈值

    private void Start()
    {
        if (ResourceManager.Instance != null)
        {
            // 订阅资源变化事件（幸福度变化会触发）
            ResourceManager.Instance.OnResourcesChanged += CheckHappiness;
            // 立即检查一次
            CheckHappiness();
        }
    }

    private void CheckHappiness()
    {
        if (fogObject == null) return;
        if (ResourceManager.Instance.Happiness >= requiredHappiness && fogObject.activeSelf)
        {
            fogObject.SetActive(false);
            Debug.Log("幸福度达到100，雾气已隐藏");
        }
    }

    private void OnDestroy()
    {
        if (ResourceManager.Instance != null)
            ResourceManager.Instance.OnResourcesChanged -= CheckHappiness;
    }
}