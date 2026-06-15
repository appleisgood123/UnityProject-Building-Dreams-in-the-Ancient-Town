using UnityEngine;
using Cinemachine;

public class CameraSwitch : MonoBehaviour
{
    public CinemachineFreeLook playerCam;
    public CinemachineVirtualCamera godCam;
    public MonoBehaviour playerMovement;
    public GameObject buildPanel;
    public KeyCode switchKey = KeyCode.Q;
    public MouseManager mouseManager;

    [HideInInspector] public bool isGodView = false;

    void Start()
    {
        if (mouseManager == null)
            mouseManager = FindObjectOfType<MouseManager>();

        playerCam.Priority = 10;
        godCam.Priority = 0;
        if (playerMovement != null)
            playerMovement.enabled = true;
        if (buildPanel != null)
            buildPanel.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(switchKey))
        {
            SetGodView(!isGodView);
        }
    }

    // 公共方法，用于外部切换上帝视角
    public void SetGodView(bool enable)
    {
        isGodView = enable;
        if (enable)
        {
            playerCam.Priority = 0;
            godCam.Priority = 10;
            if (playerMovement != null) playerMovement.enabled = false;
            if (buildPanel != null) buildPanel.SetActive(true);
            if (mouseManager != null) mouseManager.SetCursorVisible(true);
        }
        else
        {
            playerCam.Priority = 10;
            godCam.Priority = 0;
            if (playerMovement != null) playerMovement.enabled = true;
            if (buildPanel != null) buildPanel.SetActive(false);
            if (mouseManager != null) mouseManager.SetCursorVisible(false);
        }
    }
}