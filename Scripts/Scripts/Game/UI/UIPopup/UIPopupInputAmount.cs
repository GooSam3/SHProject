using System;
using UnityEngine;
using UnityEngine.UI;

public class UIPopupInputAmount : MonoBehaviour
{
    private const int MIN_VALUE = 0;
    private const int MAX_VALUE = 9999;

    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private Text txtCurAmount;

    private Action<int> onConfirm;

    private int amount;

    private bool isModuleType = false;

    private int maxValue = MAX_VALUE;

    public void Initialize(Transform parent, Action<int> _onConfirm, int customMaxValue = MAX_VALUE)
    {
        transform.SetParent(parent);
        transform.localScale = Vector3.one;
        transform.localPosition = Vector3.zero;// z값때문에;;

        rectTransform.SetAnchor(AnchorPresets.StretchAll);
        rectTransform.sizeDelta = Vector2.zero;

        onConfirm = _onConfirm;
        amount = MIN_VALUE;
        maxValue = customMaxValue;

        isModuleType = false;

        RefreshText();
    }

    public void InitializeModule(int customMaxValue = MAX_VALUE)
    {
        amount = MIN_VALUE;

        maxValue = customMaxValue;
        isModuleType = true;

        RefreshText();
    }

    public int GetAmount()
    {
        return amount;
    }

    private void RefreshText()
    {
        amount = Mathf.Clamp(amount, MIN_VALUE, maxValue);
        txtCurAmount.text = amount.ToString();
    }

    // 숫자 // -1일시 max
    public void OnClickNum(int num)
    {
        if(num<0)
        {
            amount = maxValue;
        }
        else
        {
            if (int.TryParse($"{amount}{num}", out amount) == false)
                amount = MIN_VALUE;
        }

        RefreshText();
    }

    // 더하기
    public void OnClickAddNum(int num)
    {
        amount += num;
        RefreshText();
    }

    // 백스페이스
    public void OnClickBackSpace()
    {
        string result = $"{amount}".Substring(0, $"{amount}".Length - 1);

        if(int.TryParse(result, out amount) == false)
        {
            amount = MIN_VALUE;
        }
        RefreshText();
    }

    // 초기화
    public void OnClickClear()
    {
        amount = MIN_VALUE;
        RefreshText();
    }

    // 닫기
    public void OnClickClose()
    {
        if (isModuleType) return;

        Destroy(gameObject);
    }

    // 확인
    public void OnClickConfirm()
    {
        if (isModuleType) return;
        onConfirm?.Invoke(amount);

        OnClickClose();
    }
}
