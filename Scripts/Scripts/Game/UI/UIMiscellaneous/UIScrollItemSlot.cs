using Com.TheFallenGames.OSA.CustomAdapters.GridView;
using System;

// 아이템슬롯을 스크롤러에서 쓰기 위함
public class UIScrollItemSlot : CellViewsHolder
{
    private UIItemSlot targetSlot;
    public UIItemSlot Target => targetSlot;

    private ScrollItemData scrollData;

    private Action<ScrollItemData> onClickSlot;

    public void UpdateItemSlot(ScrollItemData data, Action<ScrollItemData> _onClickSlot)
    {
        scrollData = data;

        if (data.slotType == ScrollItemData.E_SlotType.Shop)
        {
            if (data.isEmpty)
            {
                onClickSlot = null;

                targetSlot.SetEmpty();
            }
            else
            {
                onClickSlot = _onClickSlot;

                targetSlot.SetItem(data.Item.item_tid, data.Count);
                targetSlot.SetEquipState(data.Item.slot_idx > 0);
                targetSlot.SetSelectState(data.IsSelected);

                if (data.IsInteractive)
                    targetSlot.SetShadow(data.InteractiveValue);
            }
        }
        else
        {
            onClickSlot = _onClickSlot;
            targetSlot.SetItem(data.tid, data.Count);
            targetSlot.SetPostSetting(UIItemSlot.E_Item_PostSetting.ShadowOff);
        }
    }

    public override void CollectViews()
    {
        targetSlot = root.GetComponent<UIItemSlot>();
        targetSlot.InitializeScrollSlot(this, () => onClickSlot?.Invoke(scrollData));
        base.CollectViews();
    }
}
