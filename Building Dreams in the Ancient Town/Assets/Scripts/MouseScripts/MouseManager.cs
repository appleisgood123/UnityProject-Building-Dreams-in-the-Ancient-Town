using UnityEngine;

public class MouseManager : MonoBehaviour
{
    public static MouseManager Instance { get; private set; }

    [Header("自定义光标设置")]
    public Texture2D customCursor;
    public Vector2 hotSpot = Vector2.zero;

    [Header("光标状态")]
    public bool confineToWindow = true;
    public bool cursorVisible = true;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            if (transform.parent != null)
                transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (customCursor != null)
            Cursor.SetCursor(customCursor, hotSpot, CursorMode.Auto);
    }

    private void Update()
    {
       

        if (Input.GetKeyDown(KeyCode.E))
        {
         
            cursorVisible = !cursorVisible;
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            
            confineToWindow = !confineToWindow;
        }

        // 其余代码...
        Cursor.visible = cursorVisible;
        Cursor.lockState = confineToWindow ? CursorLockMode.Confined : CursorLockMode.None;
    }

    public void SetCursorVisible(bool visible)
    {
        cursorVisible = visible;
    }

    public void SetConfineToWindow(bool confine)
    {
        confineToWindow = confine;
    }
}