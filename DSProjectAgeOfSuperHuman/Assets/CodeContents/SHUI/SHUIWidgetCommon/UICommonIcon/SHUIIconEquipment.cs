using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class SHUIIconEquipment : SHUIIconBase
{
	private SItemData m_pItemData = null;  public SItemData GetIconEquipItemData() { return m_pItemData; }
	//--------------------------------------------------------------------------------
	public void DoIconItemData(SItemData pItemData, UnityAction<SHUIIconBase> delClick)
	{
		m_pItemData = pItemData;
		if (pItemData != null)
		{
			ProtSHIconInfo(pItemData.ItemIDB.ItemTID, pItemData.ItemIDB.ItemSID, delClick);
			ProtSHIconBody(pItemData.ItemTable.IconName, pItemData.ItemTable.EItemGradeUI, pItemData.ItemIDB.ItemCount, pItemData.ItemIDB.ItemCount);
			ProtSHIconLevel(pItemData.ItemIDB.ItemLevel);
			ProtIconStickerEnable(EIconStickerType.LeftTop, pItemData.ItemEquip);
		}
		else
		{
			ProtIconReset();
		}
	}

}
