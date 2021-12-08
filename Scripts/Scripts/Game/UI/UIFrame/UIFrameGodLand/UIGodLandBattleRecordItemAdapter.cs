using Com.TheFallenGames.OSA.Core;
using Com.TheFallenGames.OSA.CustomParams;
using Com.TheFallenGames.OSA.DataHelpers;
using System.Collections.Generic;

public class UIGodLandBattleRecordItemAdapter : OSA<BaseParamsWithPrefab, UIGodLandRecordItemHolder>
{
	private SimpleDataHelper<GodLandBattleRecordConverted> data;

	public void Initialize()
	{
		data = new SimpleDataHelper<GodLandBattleRecordConverted>(this);
		Init();
	}

	public void Refresh(List<GodLandBattleRecordConverted> dataList)
	{
		if (dataList != null) {
			data.ResetItems(dataList);
		}
	}

	protected override UIGodLandRecordItemHolder CreateViewsHolder(int itemIndex)
	{
		var holder = new UIGodLandRecordItemHolder();
		holder.Init(_Params.ItemPrefab, _Params.Content, itemIndex);
		return holder;
	}

	protected override void UpdateViewsHolder(UIGodLandRecordItemHolder holder)
	{
		holder.Item.SetData(data[holder.ItemIndex]);
	}
}

public class UIGodLandRecordItemHolder : BaseItemViewsHolder
{
	public UIGodLandBattleRecordItem Item { get; private set; }

	public override void CollectViews()
	{
		Item = root.GetComponent<UIGodLandBattleRecordItem>();
		base.CollectViews();
	}
}

