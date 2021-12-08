using UnityEngine;
using UnityEngine.UI;
using Com.TheFallenGames.OSA.CustomAdapters.GridView;
using System.Collections.Generic;

/// <summary> 클래스, 펫, 탈것 기반 </summary>
public abstract class TutorialSequence_FocusListItemPetChangeBase<UI_FRAME_TYPE> : TutorialSequence_FocusListItemByCellViewsHolder<UI_FRAME_TYPE, C_PetChangeData>
	where UI_FRAME_TYPE : UIFramePetChangeBase
{
	protected override string Path { get { return ""; } }
	protected override Button GetButton(GameObject item)
	{
		//return item.transform.GetComponent<Button>();
		return item.transform.GetComponentInParent<Button>();
	}

	protected override bool CheckAdapter()
	{
		return null != OwnerUI?.GetScrollPetChangeAdapter()?.Data?.List;
	}
	protected override List<C_PetChangeData> GetDataList()
	{
		return OwnerUI?.GetScrollPetChangeAdapter()?.Data?.List ?? null;
	}

	protected override CellViewsHolder GetVeiwHolder(int index)
	{
		return OwnerUI.GetScrollPetChangeAdapter().GetCellViewsHolderIfVisible(index);
	}

	protected override void ScrollTo(int index)
	{
		OwnerUI.GetScrollPetChangeAdapter().ScrollTo(index);
	}

	protected override bool CheckData(C_PetChangeData data, uint tid)
	{
		return data.Tid == tid;
	}
}