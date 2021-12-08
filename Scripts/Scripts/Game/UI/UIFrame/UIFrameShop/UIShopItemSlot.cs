using GameDB;
using System;
using UnityEngine;
using UnityEngine.UI;

public class UIShopItemSlot : MonoBehaviour
{
    const string ICON_NAME_FORMAT = "ShopIcon_{0}";
    // 0: shopid
    const string AUTOBUY_KEY_FORMAT = "AutoBuy_{0}";

    [SerializeField] private UIItemSlot itemSlot;
    [SerializeField] private Text txtName;

    [SerializeField] private GameObject objSelected;

    [Header("ShopItemInfo"), Space(10)]
    [SerializeField] private Image classIcon;
    [SerializeField] private Image goodsIcon;
    [SerializeField] private Text txtPrice;

    [Header("AutoBuy"), Space(10)]
    [SerializeField] private Slider sliderAutoBuy;
    [SerializeField] private Text txtAutoBuyValue;

    private bool isAutoBuy = false;

    private UIScrollShopItemSlot target;
    private ScrollShopItemData data;

    private Action onClickSlot;

    public void SetItem(ScrollShopItemData _data, bool _isAutoBuy)
    {
        data = _data;
        NormalShop_Table table = data.tableData;

        itemSlot.SetItem(table.GoodsItemID, 0);
        itemSlot.SetShadow(false);
        itemSlot.SetItemCount(0, false);
        itemSlot.SetOnClick(onClickSlot);

        isAutoBuy = _isAutoBuy;

        sliderAutoBuy.gameObject.SetActive(isAutoBuy);
        txtAutoBuyValue.gameObject.SetActive(isAutoBuy);

        classIcon.gameObject.SetActive(!isAutoBuy);
        goodsIcon.gameObject.SetActive(!isAutoBuy);
        txtPrice.gameObject.SetActive(!isAutoBuy);


        if (DBItem.GetItem(table.GoodsItemID, out Item_Table itemTable))
        {
            txtName.text = DBLocale.GetText(itemTable.ItemTextID);

            if (isAutoBuy == false)
            {
                classIcon.sprite = 
                    ZManagerUIPreset.Instance.GetSprite(DBLocale.GetText(string.Format(ICON_NAME_FORMAT, itemTable.UseCharacterType)));
                goodsIcon.sprite = 
                    ZManagerUIPreset.Instance.GetSprite(DBItem.GetItem(table.BuyItemID).IconID);
                txtPrice.text = table.BuyItemCount.ToString();
            }
        }
        else
        {
            txtName.text = string.Empty;
        }

        if (isAutoBuy == true)
        {
            sliderAutoBuy.maxValue = table.AutoBuyCount;

            // playerprefs~~
            int value = DeviceSaveDatas.LoadCurCharData(string.Format(AUTOBUY_KEY_FORMAT, table.NormalShopID), 0);

            sliderAutoBuy.SetValueWithoutNotify(value);
            txtAutoBuyValue.text = value.ToString();
        }
    }

    public void SetSelectState(bool state) => objSelected.SetActive(state);

    public void OnSliderValueChanged(float f)
    {
        int value = (int)f;

        txtAutoBuyValue.text = value.ToString();
        DeviceSaveDatas.SaveCurCharData(string.Format(AUTOBUY_KEY_FORMAT, data.tableData.NormalShopID), value);
    }

    public void OnClickSlot()
    {
        onClickSlot?.Invoke();
    }

    public void InitializeScrollSlot(UIScrollShopItemSlot _target, Action _onClick)
    {
        target = _target;
        onClickSlot = _onClick;
    }
}
