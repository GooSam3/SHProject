#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using Com.TheFallenGames.OSA.Core;
using Com.TheFallenGames.OSA.CustomParams;
using Com.TheFallenGames.OSA.DataHelpers;

namespace Tools
{
	public class FXSoundItemAdpater : OSA<BaseParamsWithPrefab, FXSoundItemHolder>
	{
		private SimpleDataHelper<FXSoundEffectData> data;
		private Action<uint> clickItem;

		public void Initialize(Action<uint> _clickItem)
		{
			clickItem = _clickItem;

			data = new SimpleDataHelper<FXSoundEffectData>(this);
			Init();
		}

		public void Refresh(List<FXSoundEffectData> dataList)
		{
			data.ResetItems(dataList);
		}

		protected override FXSoundItemHolder CreateViewsHolder(int itemIndex)
		{
			var holder = new FXSoundItemHolder();
			holder.Init(_Params.ItemPrefab, _Params.Content, itemIndex);
			return holder;
		}

		protected override void UpdateViewsHolder(FXSoundItemHolder holder)
		{
			holder.Item.SetData(data[holder.ItemIndex], clickItem);
		}
	}

	public class FXSoundItemHolder : BaseItemViewsHolder
	{
		public FXSoundItem Item { get; private set; }

		public override void CollectViews()
		{
			Item = root.GetComponent<FXSoundItem>();
			base.CollectViews();
		}
	}
}

#endif