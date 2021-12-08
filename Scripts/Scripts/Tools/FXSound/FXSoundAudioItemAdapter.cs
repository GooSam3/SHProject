#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using Com.TheFallenGames.OSA.Core;
using Com.TheFallenGames.OSA.CustomParams;
using Com.TheFallenGames.OSA.DataHelpers;

namespace Tools
{
	public class FXSoundAudioItemAdapter : OSA<BaseParamsWithPrefab, FXSoundAudioItemHolder>
	{
		private SimpleDataHelper<FXSoundData> data;
		private Action<uint> clickItem;

		public void Initialize(Action<uint> _clickItem)
		{
			clickItem = _clickItem;

			data = new SimpleDataHelper<FXSoundData>(this);
			Init();
		}

		public void Refresh(List<FXSoundData> dataList)
		{
			data.ResetItems(dataList);
		}

		protected override FXSoundAudioItemHolder CreateViewsHolder(int itemIndex)
		{
			var holder = new FXSoundAudioItemHolder();
			holder.Init(_Params.ItemPrefab, _Params.Content, itemIndex);
			return holder;
		}

		protected override void UpdateViewsHolder(FXSoundAudioItemHolder holder)
		{
			holder.Item.SetData(data[holder.ItemIndex], clickItem);
		}
	}

	public class FXSoundAudioItemHolder : BaseItemViewsHolder
	{
		public FXSoundAudioItem Item { get; private set; }

		public override void CollectViews()
		{
			Item = root.GetComponent<FXSoundAudioItem>();
			base.CollectViews();
		}
	}


}

#endif