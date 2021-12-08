using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NPacketData;

public abstract class SHUIWindowHeroUpgradeBase : SHUIWindowHeroBase
{
	[SerializeField]
	protected List<SHUIWidgetHeroStatUpgradeItem> UpgradeItem = null;

	//-----------------------------------------------------------------------------
	protected override void OnUIWidgetInitializePost(CUIFrameBase pParentFrame)
	{
		base.OnUIWidgetInitializePost(pParentFrame);
		for (int i = 0; i < UpgradeItem.Count; i++)
		{
			UpgradeItem[i].SetHeroStatUpgradeItem(this);
		}
	}

	protected override void OnUIWindowHeroRefresh(uint hHeroID)
	{
		base.OnUIWindowHeroRefresh(hHeroID);

		for (int i = 0; i < UpgradeItem.Count; i++)
		{
			UpgradeItem[i].DoHeroStatUpgradeItemRefresh(hHeroID);
		}
	}
	
	//-----------------------------------------------------------------------------
	protected void ProtHeroUpgradeTrainingSendPacket(bool bLevel)
	{
		List<SIntPair> pListUpgradeStat = new List<SIntPair>();
		for (int i = 0; i < UpgradeItem.Count; i++)
		{
			uint iPoint = (uint)UpgradeItem[i].GetHeroStatUpgradePoint();
			if (iPoint > 0)
			{
				SIntPair rPair = new SIntPair();
				rPair.Value1 = UpgradeItem[i].GetHeroStatUpgradeID();
				rPair.Value2 = iPoint;
				pListUpgradeStat.Add(rPair);
			}
		}

		if (bLevel)
		{
			SHManagerGameSession.Instance.RequestStatConfirmLevel(pHeroID, pListUpgradeStat);
		}
		else
		{
			SHManagerGameSession.Instance.RequestStatConfirmGold(pHeroID, pListUpgradeStat);
		}
	}

	//-----------------------------------------------------------------------------
	
}
