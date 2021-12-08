using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SHUIIconEquipmentInventory : SHUIIconEquipment
{
	private static SHUIIconBase g_EquipIconCurrent = null;
	[SerializeField]
	private EItemGrade ItemGrade = EItemGrade.None;

	//---------------------------------------------------------------------------------
	public void DoIconItemDataList(List<SItemData> pListItemData)
	{
		ProtIconReset();
		for (int i = 0; i < pListItemData.Count; i++)
		{
			if (pListItemData[i].ItemTable.EItemGrade == ItemGrade)
			{
				DoIconItemData(pListItemData[i], HandleInventoryIconClick);
				break;
			}
		}
	}

	//---------------------------------------------------------------------------------
	public void HandleInventoryIconClick(SHUIIconBase pClickIcon)
	{
		if (g_EquipIconCurrent != null)
		{
			g_EquipIconCurrent.DoUIIconFocus(false);
		}
		g_EquipIconCurrent = pClickIcon;
		g_EquipIconCurrent.DoUIIconFocus(true);

		SHUIIconEquipment pIconEquip = pClickIcon as SHUIIconEquipment;
		SHUIFrameHeroInfomation pHeroInfo = UIManager.Instance.DoUIMgrFind<SHUIFrameHeroInfomation>();
		pHeroInfo.DoHeroEquipView(pIconEquip.GetIconEquipItemData());
	}
}
