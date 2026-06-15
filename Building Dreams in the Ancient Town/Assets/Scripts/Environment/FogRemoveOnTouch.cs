using UnityEngine;

public class FogRemoveOnTouch : MonoBehaviour
{
    [Header("雾气物体")]
    public GameObject fogObject;

   

    private void OnTriggerEnter(Collider other)
    {
       
        if (other.CompareTag("Player"))
        {
            if (ResourceManager.Instance != null && ResourceManager.Instance.Happiness >= 100)
            {
                Debug.Log("幸福度满足，删除雾气");
                Destroy(fogObject);
               
            }
           
        }
    }
}