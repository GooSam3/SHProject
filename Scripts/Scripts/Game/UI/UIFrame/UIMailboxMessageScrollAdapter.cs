using Com.TheFallenGames.OSA.Core;
using Com.TheFallenGames.OSA.CustomParams;
using Com.TheFallenGames.OSA.DataHelpers;
using ZDefine;
using ZNet.Data;

public class UIMailboxMessageScrollAdapter : OSA<BaseParamsWithPrefab, UIMailboxMessageListItem>
{
	public SimpleDataHelper<ScrollMailMessageData> Data{ get; private set; }

	protected override UIMailboxMessageListItem CreateViewsHolder(int itemIndex)
	{
		var instance = new UIMailboxMessageListItem();

		instance.Init(_Params.ItemPrefab, _Params.Content, itemIndex);

		return instance;
	}

	public void RefreshData()
	{
		for (int i = 0; i < base.GetItemsCount(); i++)
			UpdateViewsHolder(base.GetItemViewsHolder(i));
	}

	protected override void UpdateViewsHolder(UIMailboxMessageListItem _holder)
	{
		if (_holder == null)
			return;

		ScrollMailMessageData model = Data[_holder.ItemIndex];
		_holder.UpdateTitleByItemIndex(model);
	}

	protected override void OnItemIndexChangedDueInsertOrRemove(UIMailboxMessageListItem shiftedViewsHolder, int oldIndex, bool wasInsert, int removeOrInsertIndex)
	{
		base.OnItemIndexChangedDueInsertOrRemove(shiftedViewsHolder, oldIndex, wasInsert, removeOrInsertIndex);

		shiftedViewsHolder.UpdateTitleByItemIndex(Data[shiftedViewsHolder.ItemIndex]);
	}

	private void Initialize()
	{
		if (Data == null)
			Data = new SimpleDataHelper<ScrollMailMessageData>(this);
	}

	public void SetScrollData()
	{
		Initialize();

		for (int i = 0; i < Me.CurCharData.MessageList.Count; i++)
		{
			if (Data.List.Find(message => message.messageData.MessageIdx == Me.CurCharData.MessageList[i].MessageIdx) == null)
				Data.InsertOne(i, new ScrollMailMessageData() { messageData = Me.CurCharData.MessageList[i] });
		}
	}
}

public class ScrollMailMessageData
{
	public MessageData messageData;
}