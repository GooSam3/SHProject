using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIScrollMonsterDrop : CUGUIScrollRectListBase
{
	private UIWorldMapMonsterDrop mProcessor = null;
	private List<UIFrameWorldMap.SMonsterInfo> mMonsterList = null;
	//----------------------------------------------------------------------------
	protected override void OnUIWidgetInitialize(CUIFrameBase _UIFrameParent)
	{
		base.OnUIWidgetInitialize(_UIFrameParent);
		
	}

	protected override void OnUIScrollRectListRefreshItem(int _Index, CUGUIWidgetSlotItemBase _NewItem)
	{
		UIScrollMonsterDropCell item = _NewItem as UIScrollMonsterDropCell;
		item.DoMonsterDropCell(mMonsterList[_Index].MontsetTID, mMonsterList[_Index].MonsterName, UIWorldMapMonsterDrop.ExtractMonsterAttributeSprite(mMonsterList[_Index].MonsterTable.AttributeType),  mProcessor);
	} 

	//-----------------------------------------------------------------------------
	public void  DoMonsterDrop(List<UIFrameWorldMap.SMonsterInfo> _monsterList, UIWorldMapMonsterDrop _processor)
	{
		mProcessor = _processor;
		mMonsterList = _monsterList;
		ProtUIScrollListInitialize(mMonsterList.Count);
		SelectFirstItem();
	}

	//------------------------------------------------------------------------------
	private void SelectFirstItem()
	{
		if (mScrollRect.content.childCount > 0)
		{
			ZButton buttonItem = mScrollRect.content.GetChild(0).gameObject.GetComponent<ZButton>();
			buttonItem.Select();
			buttonItem.onClick?.Invoke();
		}
	}
}
