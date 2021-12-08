using GameDB;
using System;
using UnityEngine;
using UnityEngine.UI;

public class CheatItemDetailPopup : MonoBehaviour
{
    [SerializeField] private Text title;
    [SerializeField] private Text btnText;

    [SerializeField] private Text maxValue;
    [SerializeField] private Slider itemCountSlider;
    [SerializeField] private InputField itemCountInptField;

    [SerializeField] private GameObject enchantGroup;
    [SerializeField] private Slider itemEnchantSlider;
    [SerializeField] private InputField itemEnchantInptField;

    // <count, enchant>
    private Action<int, int> onConfirm;

    private bool isEnchantable = false;

    // 장바구니로 넣을때
    public void Open(Item_Table data, Action<int, int> _onConfirm)
    {
        title.text = $"장바구니에 넣기\n{DBLocale.GetItemLocale(data)}";
        btnText.text = "넣기";
        maxValue.text = "9999";
        itemCountSlider.maxValue = 9999;
        itemCountSlider.value = itemCountSlider.minValue;
        itemCountInptField.text = itemCountSlider.minValue.ToString();

        onConfirm = _onConfirm;

        isEnchantable = EnumHelper.CheckFlag(E_ItemUseType.Equip, data.ItemUseType) && data.Step==0 && EnumHelper.CheckFlag(E_EnchantUseType.NormalEnchant, data.EnchantUseType);

        if (isEnchantable)
        {
            itemEnchantSlider.value = itemEnchantSlider.minValue;
            itemEnchantInptField.text = itemEnchantSlider.minValue.ToString();
        }
        enchantGroup.SetActive(isEnchantable);

        this.gameObject.SetActive(true);
    }

    // 장바구니에서 뺄때
    public void Open(OSA_CheatData data, Action<int, int> _onConfirm)
    {
        title.text = $"장바구니에서 빼기\n{DBLocale.GetItemLocale(data.itemTable)}";
        btnText.text = "빼기";
        maxValue.text = data.count.ToString();
        itemCountSlider.maxValue = data.count;
        itemCountSlider.value = itemCountSlider.maxValue;
        itemCountInptField.text = itemCountSlider.maxValue.ToString();

        onConfirm = _onConfirm;

        isEnchantable = false;
        enchantGroup.SetActive(isEnchantable);

        this.gameObject.SetActive(true);
    }

    // 
    public void OnInputValueChanged(string str)
    {
        if (int.TryParse(str, out int value))
        {
            itemCountSlider.SetValueWithoutNotify(value);
        }
        else
        {
            itemCountInptField.text = itemCountSlider.minValue.ToString();
            itemCountSlider.SetValueWithoutNotify(itemCountSlider.minValue);
        }

    }

    public void OnSliderValueChanged(float i)
    {
        itemCountInptField.SetTextWithoutNotify(((int)i).ToString());
    }

    public void OnClickClose()
    {
        this.gameObject.SetActive(false);
    }

    public void OnPopupConfirm()
    {
        onConfirm?.Invoke((int)itemCountSlider.value, isEnchantable ? (int)itemEnchantSlider.value : 0);
        this.gameObject.SetActive(false);
    }

    //--enchant

    public void OnEnchantSliderValueChanted(float i)
    {
        itemEnchantInptField.SetTextWithoutNotify(((int)i).ToString());
    }

    public void OnEnchantInputValueChanged(string str)
    {
        if (int.TryParse(str, out int value))
        {
            if (value > itemEnchantSlider.maxValue)
                value = (int)itemEnchantSlider.maxValue;

            itemEnchantSlider.SetValueWithoutNotify(value);
        }
        else
        {
            itemEnchantInptField.text = itemEnchantSlider.minValue.ToString();
            itemEnchantSlider.SetValueWithoutNotify(itemEnchantSlider.minValue);
        }

    }
}