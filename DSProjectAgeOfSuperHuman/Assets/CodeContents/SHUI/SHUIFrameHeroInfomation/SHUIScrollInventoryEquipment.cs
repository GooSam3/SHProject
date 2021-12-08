using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NPacketData;
public class SHUIScrollInventoryEquipment : CUIScrollRectBase
{
	private uint m_hHeroID = 0; 
	private List<SHUIIconEquipmentInventory> m_lisIconInstance = new List<SHUIIconEquipmentInventory>();
	//--------------------------------------------------------------------
	protected override void OnUIWidgetInitialize(CUIFrameBase _UIFrameParent)
	{
		base.OnUIWidgetInitialize(_UIFrameParent);
		int iTotal = mScrollRect.content.childCount;
		for (int i = 0; i < iTotal; i++)
		{
			SHUIIconEquipmentInventory pIconEquipment = mScrollRect.content.GetChild(i).gameObject.GetComponent<SHUIIconEquipmentInventory>();
			if (pIconEquipment != null)
			{
				m_lisIconInstance.Add(pIconEquipment);
			}
		}

		DoUIWidgetShowHide(false);
	}

	//------------------------------------------------------------------
	public void DoInventoryEquipment(uint hHeroID)
	{
		DoUIWidgetShowHide(true);
		m_hHeroID = hHeroID;
		PrivInventoryEquipLoadDB(hHeroID);
	}

	//-------------------------------------------------------------------
	private void PrivInventoryEquipRefresh(List<SItemData> pListItemData)
	{
		for (int i = 0; i < m_lisIconInstance.Count; i++)
		{
			m_lisIconInstance[i].DoIconItemDataList(pListItemData);
		}
	}

	private void PrivInventoryEquipLoadDB(uint hHeroID)
	{
		List<SPacketItem> pListPacketItemData = SHManagerGameDB.Instance.GetGameDBHeroEquip(hHeroID);
		List<uint> pListEquip = SHManagerGameDB.Instance.GetGameDBHeroEquipID(hHeroID);
		List<SItemData> pListItemData = new List<SItemData>();

		for (int i = 0; i < pListPacketItemData.Count; i++)
		{
			SItemData pItemData = new SItemData();
			pItemData.ItemID = pListPacketItemData[i].ItemTID;
			pItemData.ItemIDB = pListPacketItemData[i];
			pItemData.ItemTable = SHManagerScriptData.Instance.ExtractTableItem().GetTableItem(pItemData.ItemID);

			for (int j = 0; j < pListEquip.Count; j++)
			{
				if (pListEquip[j] == pItemData.ItemID)
				{
					pItemData.ItemEquip = true;
					break;
				}
			}

			pListItemData.Add(pItemData);
		}

		PrivInventoryEquipRefresh(pListItemData);
	}
	//-------------------------------------------------------------------
	public void HandleInventoryEquipmentComposeAll()
	{

	}


}
