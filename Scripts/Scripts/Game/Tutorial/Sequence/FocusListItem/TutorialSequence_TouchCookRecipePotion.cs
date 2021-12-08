using Com.TheFallenGames.OSA.Core;
using System.Collections.Generic;
using UnityEngine;

/// <summary> 요리 ui 레시피 페이지에서 포션을 터치한다 </summary>
public class TutorialSequence_TouchCookRecipePotion : TutorialSequence_FocusListItemByBaseViewHolder<UIFrameCook, ScrollRecipeData>
{
	protected override string Path { get { return "ItemListSlot_Inven/Img_Bg"; } }

	protected override Transform GetHighlightObject()
	{
		return mButton?.transform.parent.transform;
	}

	protected override bool CheckAdapter()
	{
		return null != OwnerUI?.RecipeListScrollAdapter?.Data?.List;
	}
	protected override List<ScrollRecipeData> GetDataList()
	{
		return OwnerUI?.RecipeListScrollAdapter?.Data?.List ?? null;
	}

	protected override BaseItemViewsHolder GetVeiwHolder(int index)
	{
		return OwnerUI.RecipeListScrollAdapter.GetBaseItemViewsHolderIfVisible(index);
	}

	protected override void ScrollTo(int index)
	{
		OwnerUI.RecipeListScrollAdapter.ScrollTo(index);
	}

	protected override bool CheckData(ScrollRecipeData data, uint tid)
	{
		return data.CookTid == tid;
	}

	protected override bool Check()
	{
		if (true == TryGetParam(out uint tid))
		{
			if(true == DBCooking.TryGet(tid, out var table))
			{
				//요리 재료 체크
				if (false == CheckMaterial(table.MaterialItemID_1))
					return false;
				if (false == CheckMaterial(table.MaterialItemID_2))
					return false;
				if (false == CheckMaterial(table.MaterialItemID_3))
					return false;
				if (false == CheckMaterial(table.MaterialItemID_4))
					return false;
				if (false == CheckMaterial(table.MaterialItemID_5))
					return false;
				if (false == CheckMaterial(table.MaterialItemID_6))
					return false;
			}			
		}

		return true;
	}

	private bool CheckMaterial(uint tid)
	{		
		if(0 == tid || true == ZNet.Data.Me.CurCharData.CheckCountInvenItemUsingMaterial(tid, 1))
			return true;

		ZLog.LogError(ZLogChannel.Quest, $"{tid}해당 재료를 찾을 수 없다.");

		return false;
	}
}