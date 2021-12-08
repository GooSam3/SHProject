using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NPacketData;
public class SHUIScrollHeroList : CUIScrollRectBase
{
	//---------------------------------------------------------------------
	protected override void OnUIWidgetRefreshOrder(int iOrder)
	{
		base.OnUIWidgetRefreshOrder(iOrder);
		PrivScrollHeroListRefresh();
	}

	protected override void OnUIWidgetShowHide(bool bShow)
	{
		base.OnUIWidgetShowHide(bShow);
		if (bShow)
		{
			PrivScrollHeroListRefresh();
		}
	}


	//---------------------------------------------------------------------
	private void PrivScrollHeroListRefresh()
	{
		DoTemplateReturnAll();

		List<SHScriptTableDescriptionHero.SDescriptionHero> pListTableHero = SHManagerScriptData.Instance.ExtractTableHero().GetTableDescriptionHeroList();
		for (int i = 0; i < pListTableHero.Count; i++)
		{
			SHScriptTableDescriptionHero.SDescriptionHero pHeroTable = pListTableHero[i];
			SPacketHeroStatUpgrade pStatUpgrade = SHManagerGameDB.Instance.GetGameDBHeroStatUpgrade(pHeroTable.UnitID);
			if (pStatUpgrade != null)
			{
				PrivScrollHeroListMake(pHeroTable, pStatUpgrade.HeroLevel, true);
			}
			else
			{
				PrivScrollHeroListMake(pHeroTable, 1, false);
			}
		}

		PrivScrollHeroListSelect(UIManager.Instance.DoUIMgrFind<SHUIFrameNavigationBar>().GetNavigationBarSeletHero());
	}

	private void PrivScrollHeroListMake(SHScriptTableDescriptionHero.SDescriptionHero pHeroTable, int iLevel, bool bHasHero)
	{
		SHUIScrollHeroListSlot pSlotItem = DoTemplateRequestItem() as SHUIScrollHeroListSlot;
		pSlotItem.DoUIHeroListSlot(pHeroTable.UnitID, iLevel, pHeroTable.UIFaceName, pHeroTable.UnitName, !bHasHero);
	}

	private void PrivScrollHeroListSelect(uint hHeroID)
	{
		List<CUIWidgetTemplateItemBase> pListChild = GetWidgetTemplateList();
		for (int i = 0; i < pListChild.Count; i++)
		{
			SHUIScrollHeroListSlot pSlotItem = pListChild[i] as SHUIScrollHeroListSlot;
			if (pSlotItem.GetHeroListSlotID() == hHeroID)
			{
				pSlotItem.DoUIHeroListSlotSelect();
				break;
			}
		}
	}
}
