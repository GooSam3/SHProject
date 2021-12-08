using Com.TheFallenGames.OSA.Core;
using UnityEngine;

/// <summary> 리스트 아이템 기반 </summary>
public abstract class TutorialSequence_FocusListItemByBaseViewHolder<UI_FRAME_TYPE, SIMPLEDATA_TYPE> : TutorialSequence_FocusListItem<UI_FRAME_TYPE, SIMPLEDATA_TYPE, BaseItemViewsHolder>
	where UI_FRAME_TYPE : CUIFrameBase
	where SIMPLEDATA_TYPE : class
{	

	protected override RectTransform GetItemTrans(int index)
	{
		return GetVeiwHolder(index)?.root;
	}

}