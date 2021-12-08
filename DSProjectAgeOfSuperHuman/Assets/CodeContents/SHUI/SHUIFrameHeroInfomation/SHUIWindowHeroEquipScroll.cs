using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SHUIWindowHeroEquipScroll : SHUIWindowHeroBase
{
	[SerializeField]
	private SHUIScrollInventoryEquipment InventoryEquip = null;
	[SerializeField]
	private SHUIWidgetHeroEquipInfo EquipInfo = null;

	//---------------------------------------------------------------
	protected override void OnUIWidgetShowHide(bool bShow)
	{
		base.OnUIWidgetShowHide(bShow);
		if (bShow == false)
		{
			EquipInfo.DoUIWidgetShowHide(false);
		}
	}


	//----------------------------------------------------------------
	protected override void OnUIWindowHeroRefresh(uint hHeroID)
	{
		base.OnUIWindowHeroRefresh(hHeroID);
		InventoryEquip.DoInventoryEquipment(hHeroID);
	}
	//----------------------------------------------------------------

}
