using Com.TheFallenGames.OSA.CustomAdapters.GridView;
using UnityEngine;

/// <summary> 리스트 아이템 기반 </summary>
public abstract class TutorialSequence_FocusListItemByCellViewsHolder<UI_FRAME_TYPE, SIMPLEDATA_TYPE> : TutorialSequence_FocusListItem<UI_FRAME_TYPE, SIMPLEDATA_TYPE, CellViewsHolder>
	where UI_FRAME_TYPE : CUIFrameBase
	where SIMPLEDATA_TYPE : class
{	

	protected override RectTransform GetItemTrans(int index)
	{
		return GetVeiwHolder(index)?.views;
	}

}