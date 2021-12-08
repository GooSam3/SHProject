using System;

public class UITowerRewardListItem : UIItemSlot
{
	uint itemTid;

	private Action<uint> selectEvent = null;

	public void DoUpdate(uint _itemTid, uint count, Action<uint> itemEvent)
	{
		base.SetItem(_itemTid, count);

		itemTid = _itemTid;

		if (selectEvent == null) {
			selectEvent = itemEvent;
		}
	}

	public void UIRewardItemClick()
	{
		ZLog.Log(ZLogChannel.Default, $"UIListItemClick {itemTid}");
		if (selectEvent != null) {
			selectEvent(itemTid);
		}
	}
}
