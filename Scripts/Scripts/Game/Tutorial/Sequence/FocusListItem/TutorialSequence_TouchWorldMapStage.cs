using Com.TheFallenGames.OSA.Core;
using System.Collections.Generic;

/// <summary> 스테이지 터치 </summary>
public class TutorialSequence_TouchWorldMapStage : TutorialSequence_FocusListItemByBaseViewHolder<UIFrameWorldMap, C_WorldMapData>
{
	protected override string Path { get { return "World_Local_Slot"; } }

	protected override bool CheckAdapter()
	{
		return null != OwnerUI?.WorldMapScrollAdapter?.Data?.List;
	}
	protected override List<C_WorldMapData> GetDataList()
	{
		return OwnerUI?.WorldMapScrollAdapter?.Data?.List ?? null;
	}

	protected override BaseItemViewsHolder GetVeiwHolder(int index)
	{
		return OwnerUI.WorldMapScrollAdapter.GetBaseItemViewsHolderIfVisible(index);
	}

	protected override void ScrollTo(int index)
	{
		OwnerUI.WorldMapScrollAdapter.ScrollTo(index);
	}

	protected override bool CheckData(C_WorldMapData data, uint tid)
	{
		return data.worldInfo.StageID == tid;
	}

	protected override bool Check()
	{		
		return true;
	}
}