using TMPro;
using UnityEngine;

public class MoneyPanel : MonoBehaviour
{
    private TextMeshProUGUI currencyText;

    private void Awake()
    {
        Transform currencyTrans = transform.Find("CurrencyText");
        if (currencyTrans == null)
        {
            Debug.LogError("MoneyPanel: 找不到子对象 'CurrencyText'，请检查层级结构！");
            return;
        }
        currencyText = currencyTrans.GetComponent<TextMeshProUGUI>();
        if (currencyText == null)
        {
            Debug.LogError("MoneyPanel: CurrencyText 上没有 TextMeshProUGUI 组件！");
            return;
        }
    }

    private void Start()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("MoneyPanel: GameManager 实例不存在！");
            return;
        }

        GameManager.Instance.OnCurrencyChanged += UpdateCurrency;
        UpdateCurrency(GameManager.Instance.CurrentCurrency);
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnCurrencyChanged -= UpdateCurrency;
    }

    private void UpdateCurrency(int newAmount)
    {
        if (currencyText != null)
            currencyText.text = $"¥ {newAmount}";   // 使用人民币符号
    }
}