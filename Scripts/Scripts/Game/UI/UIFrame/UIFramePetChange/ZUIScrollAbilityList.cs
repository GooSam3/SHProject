using System.Collections.Generic;

public class ZUIScrollAbilityList : CUGUIScrollRectListBase
{
    private List<UIAbilityData> listAbility = new List<UIAbilityData>();

    protected override void OnUIWidgetInitializePost(CUIFrameBase _UIFrameParent)
    {
        base.OnUIWidgetInitializePost(_UIFrameParent);
    }

    protected override void OnUIScrollRectListRefreshItem(int _Index, CUGUIWidgetSlotItemBase _NewItem)
    {
        ZUIScrollAbilityListItem item = _NewItem as ZUIScrollAbilityListItem;
        UIAbilityData data = listAbility[_Index];
        item.SetSlot(data);
    }

    public void ClearListData()
    {
        listAbility.Clear();
    }

    public void AddListData(UIAbilityData ability)
    {
        listAbility.Add(ability);
    }

    public void AddListData(List<UIAbilityData> lAbility)
    {
        listAbility.AddRange(lAbility);
    }

    public void DoRefresh()
    {
        ProtUIScrollListInitialize(listAbility.Count);
    }
}
