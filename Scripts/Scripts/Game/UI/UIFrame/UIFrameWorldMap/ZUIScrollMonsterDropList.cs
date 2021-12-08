using System.Collections.Generic;
using UnityEngine.UI;

public class ZUIScrollMonsterDropList : CUGUIScrollRectBase
{
	private ContentSizeFitter mSizeFitter = null;
	private List<ZUIScrollMonsterDropListItem> m_listDropList = new List<ZUIScrollMonsterDropListItem>();
	//-----------------------------------------------------------------
	protected override void OnUIWidgetInitialize(CUIFrameBase _UIFrameParent)
	{
		base.OnUIWidgetInitialize(_UIFrameParent);
		mSizeFitter = GetComponent<ContentSizeFitter>();
	}

	//-----------------------------------------------------------------
	public void DoMonsterDropList(List<UIFrameWorldMap.SMonsterInfo> ListMonster)
	{
		ResetDropList();

		for (int i = 0; i < ListMonster.Count; i++)
		{
//			ZUIScrollMonsterDropListItem Item = ProtUIScrollSlotItemRequest() as ZUIScrollMonsterDropListItem;
//			Item.DoMonsterDropItem(ListMonster[i]);
		}
	}

	//-----------------------------------------------------------------
	private void ResetDropList()
	{
		m_listDropList.Clear();

		while(mScrollRect.content.childCount != 0)
		{
			ZUIScrollMonsterDropListItem Item = mScrollRect.content.GetChild(0).GetComponent<ZUIScrollMonsterDropListItem>();
			ProtUIScrollSlotItemReturn(Item);
		}
	}

	
}
