using Com.TheFallenGames.OSA.Core;

public class ScrollGuildBuffListHolder : BaseItemViewsHolder
{
    private UIGuildBuffListSlot slot;
    public UIGuildBuffListSlot Slot { get { return slot; } }

    public override void CollectViews()
    {
        slot = root.GetComponent<UIGuildBuffListSlot>();
        base.CollectViews();
    }
}