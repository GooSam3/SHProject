using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NPacketData;
using System.Linq;

public partial class SHManagerGameDB : CManagerTemplateBase<SHManagerGameDB>
{
	//----------------------------------------------------------------------
	public void ResponseStageClear(SPacketStageClear pPacketStageClear)
	{
		m_mapStageClearInfo[pPacketStageClear.WorldIndex] = pPacketStageClear;
	}
	
	public void ResponseStatUpgradeLevel(uint hHeroID, int iRemainPoint, List<SIntPair> pListStatUpgrade)
	{
		SPacketHeroStatUpgrade pStatUpgrade = FindOrAllocHeroStatUpgrade(hHeroID);		
	//	pStatUpgrade.RemainStat = iRemainPoint;
		PrivStatUpgrade(pStatUpgrade, pListStatUpgrade);
	}

	public void ResponseStatUpgradeGold(uint hHeroID, uint iTotalGold, List<SIntPair> pListStatUpgrade)
	{
		SPacketHeroStatUpgrade pStatUpgrade = FindOrAllocHeroStatUpgrade(hHeroID);
	//	PlayerCurrency.Gold = iTotalGold;
		PrivStatUpgrade(pStatUpgrade, pListStatUpgrade);
	}

	public void ResponseRewardEnemy(List<SItemData> pListReward)
	{
		UIManager.Instance.DoUIMgrFind<SHUIFrameItemBag>().DoUIFrameItemGain(pListReward);
	}

	public void ResponseEquipMain(uint hHeroID, uint hItemID)
	{
		if (m_mapHeroInfo.ContainsKey(hHeroID))
		{
			SPacketHero pHeroInfo = m_mapHeroInfo[hHeroID];
			pHeroInfo.EquipMain = hItemID;
			UIManager.Instance.DoUIMgrFind<SHUIFrameHeroInfomation>().DoHeroInfomationRefresh();
		}
	}

	public void ResponseEquipSub1(uint hHeroID, uint hItemID)
	{
		if (m_mapHeroInfo.ContainsKey(hHeroID))
		{
			SPacketHero pHeroInfo = m_mapHeroInfo[hHeroID];
			pHeroInfo.EquipSub1 = hItemID;
			UIManager.Instance.DoUIMgrFind<SHUIFrameHeroInfomation>().DoHeroInfomationRefresh();
		}
	}

	public void ResponseEquipSub2(uint hHeroID, uint hItemID)
	{
		if (m_mapHeroInfo.ContainsKey(hHeroID))
		{
			SPacketHero pHeroInfo = m_mapHeroInfo[hHeroID];
			pHeroInfo.EquipSub2 = hItemID;
			UIManager.Instance.DoUIMgrFind<SHUIFrameHeroInfomation>().DoHeroInfomationRefresh();
		}
	}

	public void ResponseEquipUnMount(uint hHeroID, uint hItemID)
	{
		if (m_mapHeroInfo.ContainsKey(hHeroID))
		{
			SPacketHero pHeroInfo = m_mapHeroInfo[hHeroID];

			if (pHeroInfo.EquipMain == hItemID)
			{
				pHeroInfo.EquipMain = 0;
			}
			else if (pHeroInfo.EquipSub1 == hItemID)
			{
				pHeroInfo.EquipSub1 = 0;
			}
			else if (pHeroInfo.EquipSub2 == hItemID)
			{
				pHeroInfo.EquipSub2 = 0;
			}

			UIManager.Instance.DoUIMgrFind<SHUIFrameHeroInfomation>().DoHeroInfomationRefresh();
		}
	}

	//---------------------------------------------------------------------------------------------
	private void PrivStatUpgrade(SPacketHeroStatUpgrade pStatUpgrade, List<SIntPair> pListStatUpgrade)
	{
		for (int i = 0; i < pListStatUpgrade.Count; i++)
		{
			bool bFind = false;
			for (int j = 0; j < pStatUpgrade.UpgradePoint.Count; j++)
			{
				if (pListStatUpgrade[i].Value1 == pStatUpgrade.UpgradePoint[j].Value1)
				{
					SIntPair rPair = pStatUpgrade.UpgradePoint[j];
					rPair.Value2 += pListStatUpgrade[i].Value2;
					pStatUpgrade.UpgradePoint[j] = rPair;
					bFind = true;
					break;
				}
			}

			if (bFind == false)
			{
				pStatUpgrade.UpgradePoint.Add(pListStatUpgrade[i]);
			}
		}
		PrivGameDBCacheHeroStat(pStatUpgrade.HeroID);
		UIManager.Instance.DoUIMgrFind<SHUIFrameHeroInfomation>().DoHeroInfomationRefresh();
	}

	private SPacketHeroStatUpgrade FindOrAllocHeroStatUpgrade(uint hHeroID)
	{
		SPacketHeroStatUpgrade pUpgrade = null;
		if (m_mapHeroStatUpgradeInfo.ContainsKey(hHeroID))
		{
			pUpgrade = m_mapHeroStatUpgradeInfo[hHeroID];
		}
		else
		{
			pUpgrade = new SPacketHeroStatUpgrade();
			m_mapHeroStatUpgradeInfo[hHeroID] = pUpgrade;
		}
		return pUpgrade;
	}
}
