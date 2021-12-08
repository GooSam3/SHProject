using Com.TheFallenGames.OSA.CustomAdapters.GridView;
using System;

// 샵 아이템슬롯 
public class UIScrollShopItemSlot : CellViewsHolder
{
    private UIShopItemSlot targetSlot;
    public UIShopItemSlot Target => targetSlot;

    private ScrollShopItemData scrollData;

    private Action<ScrollShopItemData> onClickSlot;

    public void UpdateItemSlot(ScrollShopItemData data, Action<ScrollShopItemData> _onClickSlot, bool isAutoBuy)
    {
        scrollData = data;
        onClickSlot = _onClickSlot;

        targetSlot.SetItem(data, isAutoBuy);
        targetSlot.SetSelectState(data.IsSelected);
    }

    public override void CollectViews()
    {
        targetSlot = root.GetComponent<UIShopItemSlot>();
        targetSlot.InitializeScrollSlot(this, () => onClickSlot?.Invoke(scrollData));
        base.CollectViews();
    }
}
