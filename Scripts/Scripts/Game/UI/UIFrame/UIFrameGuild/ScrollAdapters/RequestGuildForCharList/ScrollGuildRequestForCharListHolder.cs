using Com.TheFallenGames.OSA.Core;

public class ScrollGuildRequestForCharListHolder : BaseItemViewsHolder
{
    private UIGuildRequestForCharListSlot slot;
    public UIGuildRequestForCharListSlot Slot { get { return slot; } }

    public override void CollectViews()
    {
        slot = root.GetComponent<UIGuildRequestForCharListSlot>();
        base.CollectViews();
    }
}