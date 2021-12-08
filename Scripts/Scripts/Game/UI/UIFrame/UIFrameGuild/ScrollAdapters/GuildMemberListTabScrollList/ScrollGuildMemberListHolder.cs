using Com.TheFallenGames.OSA.Core;

public class ScrollGuildMemberListHolder : BaseItemViewsHolder
{
    private ScrollGuildMemberListSlot slot;
    public ScrollGuildMemberListSlot Slot { get { return slot; } }

    public override void CollectViews()
    {
        slot = root.GetComponent<ScrollGuildMemberListSlot>();
        base.CollectViews();
    }
}