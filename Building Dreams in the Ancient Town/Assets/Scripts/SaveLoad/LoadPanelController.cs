using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>读取存档面板控制器（UI 由 Editor 脚本预生成在 Hierarchy 中）</summary>
public class LoadPanelController : MonoBehaviour
{
    private GameObject loadOverlay;
    private int expandedSlot = -1;
    private TextMeshProUGUI[] slotLabels = new TextMeshProUGUI[3];

    void Start()
    {
        // 查找 Hierarchy 中预生成的 LoadOverlay
        GameObject canvasGo = GameObject.Find("Canvas");
        Canvas canvas = canvasGo != null ? canvasGo.GetComponent<Canvas>() : null;
        if (canvas != null)
        {
            Transform ovT = canvas.transform.Find("LoadOverlay");
            if (ovT != null)
            {
                loadOverlay = ovT.gameObject;

                // 缓存标签引用
                Transform frameT = ovT.Find("Frame");
                if (frameT != null)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        Transform slotT = frameT.Find($"Slot{i}");
                        if (slotT != null)
                        {
                            Transform labelT = slotT.Find("Label");
                            if (labelT != null)
                                slotLabels[i] = labelT.GetComponent<TextMeshProUGUI>();

                            // 绑定槽位点击
                            Button slotBtn = slotT.GetComponent<Button>();
                            if (slotBtn != null)
                            {
                                int captured = i;
                                slotBtn.onClick.AddListener(() => OnSlotClicked(captured));
                            }

                            // 绑定操作栏按钮
                            Transform barT = slotT.Find("ActionBar");
                            if (barT != null)
                            {
                                Transform loadBtnT = barT.Find("LoadBtn");
                                if (loadBtnT != null)
                                {
                                    Button loadBtn = loadBtnT.GetComponent<Button>();
                                    if (loadBtn != null)
                                    {
                                        int cap = i;
                                        loadBtn.onClick.AddListener(() =>
                                        {
                                            if (!SaveLoadManager.HasSave(cap)) return;
                                            SaveLoadManager.PendingLoadData = SaveLoadManager.LoadSave(cap);
                                            UnityEngine.SceneManagement.SceneManager.LoadScene("Scene1");
                                        });
                                    }
                                }

                                Transform delBtnT = barT.Find("DeleteBtn");
                                if (delBtnT != null)
                                {
                                    Button delBtn = delBtnT.GetComponent<Button>();
                                    if (delBtn != null)
                                    {
                                        int cap = i;
                                        delBtn.onClick.AddListener(() =>
                                        {
                                            SaveLoadManager.DeleteSave(cap);
                                            RefreshAllSlots();
                                            CollapseSlot(cap);
                                            if (expandedSlot == cap) expandedSlot = -1;
                                        });
                                    }
                                }
                            }
                        }
                    }
                }

                // 绑定关闭按钮
                Transform closeT = frameT != null ? frameT.Find("CloseButton") : null;
                if (closeT != null)
                {
                    Button closeBtn = closeT.GetComponent<Button>();
                    if (closeBtn != null)
                    {
                        closeBtn.onClick.RemoveAllListeners();
                        closeBtn.onClick.AddListener(Hide);
                    }
                }
            }
        }
    }

    public void Show()
    {
        if (loadOverlay == null) return;
        RefreshAllSlots();
        loadOverlay.SetActive(true);
    }

    public void Hide()
    {
        if (loadOverlay != null) loadOverlay.SetActive(false);
        CollapseSlot(expandedSlot);
        expandedSlot = -1;
    }

    private void RefreshAllSlots()
    {
        for (int i = 0; i < 3; i++)
        {
            if (slotLabels[i] != null)
                UpdateSlotLabel(i);
        }
        CollapseSlot(expandedSlot);
        expandedSlot = -1;
    }

    private void UpdateSlotLabel(int slot)
    {
        if (slotLabels[slot] == null) return;
        if (SaveLoadManager.HasSave(slot))
        {
            SaveData data = SaveLoadManager.LoadSave(slot);
            slotLabels[slot].text = $"档位 {slot + 1}     {data.saveTime}";
        }
        else
        {
            slotLabels[slot].text = $"档位 {slot + 1}     空";
        }
    }

    private void OnSlotClicked(int slot)
    {
        if (!SaveLoadManager.HasSave(slot)) return;

        CollapseSlot(expandedSlot);

        Transform frameT = loadOverlay.transform.Find("Frame");
        if (frameT == null) return;
        Transform slotT = frameT.Find($"Slot{slot}");
        if (slotT == null) return;
        Transform bar = slotT.Find("ActionBar");
        if (bar == null) return;

        bool nowOpen = !bar.gameObject.activeSelf;
        bar.gameObject.SetActive(nowOpen);
        expandedSlot = nowOpen ? slot : -1;
    }

    private void CollapseSlot(int slot)
    {
        if (slot < 0 || loadOverlay == null) return;
        Transform frameT = loadOverlay.transform.Find("Frame");
        if (frameT == null) return;
        Transform slotT = frameT.Find($"Slot{slot}");
        if (slotT != null)
        {
            Transform bar = slotT.Find("ActionBar");
            if (bar != null) bar.gameObject.SetActive(false);
        }
    }
}
