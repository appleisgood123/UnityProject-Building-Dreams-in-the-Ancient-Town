using UnityEngine;
using Cinemachine;

public class CameraSwitch : MonoBehaviour
{
    public CinemachineFreeLook playerCam;
    public CinemachineVirtualCamera godCam;
    public MonoBehaviour playerMovement;
    public GameObject buildPanel;          // 底部建造面板
    public KeyCode switchKey = KeyCode.Q;
    public MouseManager mouseManager;      // 可手动拖拽，若未拖拽则自动查找

    [HideInInspector] public bool isGodView = false;

    void Start()
    {
        // 自动查找 MouseManager（如果未拖拽）
        if (mouseManager == null)
            mouseManager = FindObjectOfType<MouseManager>();

        playerCam.Priority = 10;
        godCam.Priority = 0;
        if (playerMovement != null)
            playerMovement.enabled = true;
        if (buildPanel != null)
            buildPanel.SetActive(false);   // 初始隐藏建造面板
    }

    void Update()
    {
        if (Input.GetKeyDown(switchKey))
        {
            isGodView = !isGodView;
            if (isGodView)
            {
                // 切换到神视角
                playerCam.Priority = 0;
                godCam.Priority = 10;
                if (playerMovement != null)
                    playerMovement.enabled = false;
                if (buildPanel != null)
                    buildPanel.SetActive(true);

                // 显示鼠标
                if (mouseManager != null)
                    mouseManager.SetCursorVisible(true);
            }
            else
            {
                // 切换到玩家视角
                playerCam.Priority = 10;
                godCam.Priority = 0;
                if (playerMovement != null)
                    playerMovement.enabled = true;
                if (buildPanel != null)
                    buildPanel.SetActive(false);

                // 隐藏鼠标
                if (mouseManager != null)
                    mouseManager.SetCursorVisible(false);
            }
        }
    }
}