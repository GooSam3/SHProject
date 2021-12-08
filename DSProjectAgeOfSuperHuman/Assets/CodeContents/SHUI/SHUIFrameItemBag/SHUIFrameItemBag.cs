using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SHUIFrameItemBag : SHUIFrameBase
{
    [SerializeField]
    private List<SHUIIconDropItem> DropItemSlot = new List<SHUIIconDropItem>();

    private List<SItemData> m_listItemInstance = new List<SItemData>();
	//------------------------------------------------------------------------------
	protected override void OnUIFrameInitialize()
	{
		base.OnUIFrameInitialize();
		for (int i = 0; i < DropItemSlot.Count; i++)
		{
			DropItemSlot[i].SetMonoActive(false);
		}
	}

	//----------------------------------------------------------------------------
	public void DoUIFrameItemGain(List<SItemData> pListItem)
	{
		for (int i = 0; i < DropItemSlot.Count; i++)
		{
			DropItemSlot[i].SetMonoActive(false);
		}

		for (int i = 0; i < pListItem.Count; i++)
		{
			if (i < DropItemSlot.Count)
			{
				DropItemSlot[i].DoIconItemData(pListItem[i], null);
			}
		}	
	}
}
