using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIScrollDropItem : CUGUIScrollRectListBase
{

	private List<UIFrameWorldMap.SMonsterDropItem> mDropItemList = null;
	//--------------------------------------------------------
	protected override void OnUIWidgetInitialize(CUIFrameBase _UIFrameParent)
	{
		base.OnUIWidgetInitialize(_UIFrameParent);
	}

	protected override void OnUIScrollRectListRefreshItem(int _Index, CUGUIWidgetSlotItemBase _NewItem)
	{
		UIScrollDropItemCell item = _NewItem as UIScrollDropItemCell;
		item.DoDropItem(mDropItemList[_Index]);
	}

	//--------------------------------------------------------
	public void DoDropItemList(List<UIFrameWorldMap.SMonsterDropItem> _listDropItem)
	{
		mDropItemList = _listDropItem;
		ProtUIScrollListInitialize(_listDropItem.Count);
	}
}
