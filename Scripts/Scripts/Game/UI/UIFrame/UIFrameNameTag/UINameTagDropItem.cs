using GameDB;
using UnityEngine;

public class UINameTagDropItem : UINameTagBase
{
	private string mItemText = null;
	private Color  mItemColor = Color.white;
	private DropItemComponent mDropItem = null;
	//---------------------------------------------------------------
	public void DoNameTagInitialize(DropItemComponent _followItem, Item_Table _itemTable)
	{
		_followItem.OnShowTierEffect = HandleDropItemText;
		
		mItemColor = DBUIResouce.GetGradeColor(E_UIType.Item, _itemTable.DropEffectGrade);
		mItemText = _itemTable.ItemTextID;
		mDropItem = _followItem;
		NameTagFollowTarget(_followItem.gameObject, _followItem.transform, null, 0);
		DoUIWidgetFocus(false);
		SetNameTagShowHide(false);
	}

	//---------------------------------------------------------------
	private void HandleDropItemText()
	{
		if (mDropItem == null) return;

		SetNameTagShowHide(true);
		SetNameTag(mItemText, mItemColor);
	}
}
