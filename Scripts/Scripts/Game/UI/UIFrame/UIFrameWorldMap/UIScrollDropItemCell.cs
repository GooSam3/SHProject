using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameDB;
public class UIScrollDropItemCell : CUGUIWidgetSlotItemBase
{
	[SerializeField] ZUIIconNormal ItemIcon = null;
	//--------------------------------------------------------
	public void DoDropItem(UIFrameWorldMap.SMonsterDropItem _dropItem)
	{
		Item_Table itemTable = DBItem.GetItem(_dropItem.ItemTID);
		if (itemTable == null) return;
		ItemIcon.DoUIIconSetting(itemTable, _dropItem.ItemMaxCount);
	}
}
