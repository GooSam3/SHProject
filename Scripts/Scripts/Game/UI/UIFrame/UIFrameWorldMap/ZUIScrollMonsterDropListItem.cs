using System.Collections.Generic;
using UnityEngine;

public class ZUIScrollMonsterDropListItem : CUGUIWidgetSlotItemBase
{
	[SerializeField] private ZText ItemName = null;   
	private bool mDropListOpen = false;
	private UIFrameWorldMap.SMonsterInfo  mMonsterInfo = null;
	private List<ZUIMonsterDropItemLabel> m_listLabel = new List<ZUIMonsterDropItemLabel>();
	//----------------------------------------------------------------------
	protected override void OnUIWidgetInitialize(CUIFrameBase _UIFrameParent)
	{
		base.OnUIWidgetInitialize(_UIFrameParent);
		GetComponentsInChildren<ZUIMonsterDropItemLabel>(true, m_listLabel);
		for (int i = 0; i < m_listLabel.Count; i++)
		{
			m_listLabel[i].SetMonoActive(false);
		}
	}

	//----------------------------------------------------------------------
	public void DoMonsterDropItem(UIFrameWorldMap.SMonsterInfo _monsterInfo)
	{
		ItemName.text = _monsterInfo.MonsterName;
		mMonsterInfo = _monsterInfo;
	}
	
	public void HandleDropItemList()
	{
		ResetAllItemLable(!mDropListOpen);
	}

	
	//----------------------------------------------------------------------

	private void ResetAllItemLable(bool _show)
	{
		mDropListOpen = _show;
		if (_show)
		{
			DropDownItemLabel();
		}
		else
		{
			for (int i = 0; i < m_listLabel.Count; i++)
			{
				m_listLabel[i].SetMonoActive(false);
			}
		}
	}

	private void DropDownItemLabel()
	{
		if (mMonsterInfo == null) return;

		for (int i = 0; i < mMonsterInfo.ListMonsterDropInfo.Count; i++)
		{
			if (i >= m_listLabel.Count) continue;

			UIFrameWorldMap.SMonsterDropItem DropItem = mMonsterInfo.ListMonsterDropInfo[i];		
			ZUIMonsterDropItemLabel Item = m_listLabel[i];
			Item.DoMonsterDropItemLabel(DropItem);
		}
	}

	
}
