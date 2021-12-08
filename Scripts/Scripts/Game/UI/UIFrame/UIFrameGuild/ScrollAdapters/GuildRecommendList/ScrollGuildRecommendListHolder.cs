using Com.TheFallenGames.OSA.Core;

public class ScrollGuildRecommendListHolder : BaseItemViewsHolder
{
    private UIGuildRecommendListSlot slot;
    public UIGuildRecommendListSlot Slot { get { return slot; } }

    public override void CollectViews()
    {
        slot = root.GetComponent<UIGuildRecommendListSlot>();
        base.CollectViews();
    }
}