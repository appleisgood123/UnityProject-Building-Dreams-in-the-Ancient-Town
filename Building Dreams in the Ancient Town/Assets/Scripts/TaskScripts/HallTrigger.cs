using UnityEngine;

public class HallTrigger : MonoBehaviour
{
    public GameObject panelToShow;   // ÍÏ×§ÒªÏÔÊ¾µÄ Panel

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (panelToShow != null)
                panelToShow.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (panelToShow != null)
                panelToShow.SetActive(false);
        }
    }
}