using System.Collections;
using System.Collections.Generic;
using NPacketData;
using System.Linq;

public partial class SHManagerGameDB : CManagerTemplateBase<SHManagerGameDB>
{
	private Dictionary<uint, SPacketHero>				m_mapHeroInfo				= new Dictionary<uint, SPacketHero>();
	private Dictionary<uint, SPacketHeroStatUpgrade>		m_mapHeroStatUpgradeInfo	= new Dictionary<uint, SPacketHeroStatUpgrade>();
	private Dictionary<uint, SPacketStageClear>			m_mapStageClearInfo		= new Dictionary<uint, SPacketStageClear>();
	private Dictionary<uint, SPackatStatCache>			m_mapHeroStatCache		= new Dictionary<uint, SPackatStatCache>();
	private Dictionary<uint, SPacketPotentialBingoInfo>	m_mapPotentialBingo		= new Dictionary<uint, SPacketPotentialBingoInfo>();
	private SPacketHeroDeck HeroDeckInfo = new SPacketHeroDeck();
	//-----------------------------------------------------------------------------
	protected override void OnUnityAwake()
	{
		base.OnUnityAwake();
		PrivGameDBLocalConfigRequire();
		PrivGameDBLocalConfigOption();
	}

	protected override void OnManagerScriptLoaded()
	{
		base.OnManagerScriptLoaded();
		PrivGameDBCacheHeroStat(101); // 임시 코드 
		PrivGameDBCacheHeroStat(102);
		PrivGameDBCacheHeroStat(103);
	}

	//-----------------------------------------------------------------------------
	public SHScriptTableStage.SStageTable GetGamgDBNextStage(uint iWorldIndex)
	{
		SHScriptTableStage.SStageTable pTable = null;
		if (m_mapStageClearInfo.ContainsKey(iWorldIndex))
		{
			uint iStageLevel = m_mapStageClearInfo[iWorldIndex].StageIndex;
			pTable = SHManagerScriptData.Instance.ExtractTableStage().FindTableStageByClearCount(iWorldIndex, iStageLevel);
		}
		else
		{
			pTable = SHManagerScriptData.Instance.ExtractTableStage().FindTableStageByClearCount(iWorldIndex, 1);
		}
		return pTable;
	}

	public bool CheckGameDBStageEnter(uint iWorldIndex, uint iStageIndex)
	{
		//bool bEnter = false;
		//if (m_mapStageClearInfo.ContainsKey(iWorldIndex))
		//{
		//	if (iStageIndex <= m_mapStageClearInfo[iWorldIndex].StageLevel)
		//	{
		//		bEnter = true;
		//	}
		//}

		//return bEnter;
		return true;
	}

	public List<SPacketHero> GetGameDBHeroList()
	{
		List<SPacketHero> pHeroInfo = new List<SPacketHero>();
		Dictionary<uint, SPacketHero>.ValueCollection.Enumerator it = m_mapHeroInfo.Values.GetEnumerator();
		while (it.MoveNext())
		{
			pHeroInfo.Add(it.Current.CopyInstance<SPacketHero>());
		}
		return pHeroInfo;
	}

	public SPacketHero GetGameDBHeroInfo(uint hHeroID)
	{
		SPacketHero pHeroInfo = null;

		if (m_mapHeroInfo.ContainsKey(hHeroID))
		{
			pHeroInfo = m_mapHeroInfo[hHeroID].CopyInstance<SPacketHero>();
		}

		return pHeroInfo;
	}

	public List<uint> GetGameDBHeroDeck(int iDeckIndex = -1)
	{
		List<uint> pList = new List<uint>();
		if (iDeckIndex < HeroDeckInfo.DeckList.Count && iDeckIndex >= 0)
		{
			pList = HeroDeckInfo.DeckList[iDeckIndex].DeckMember.ToList();
		}
		else
		{
			pList = HeroDeckInfo.DeckList[HeroDeckInfo.SelectedDeck].DeckMember.ToList();
		}

		return pList;
	}

	public uint GetGameDBHeroPower(uint hHeroID)
	{
		return ExtractGameDBPowerHero(hHeroID);
	}

	public uint GetGameDBDeckPower(int iDeckIndex = -1)
	{
		if (iDeckIndex == -1)
		{
			iDeckIndex = HeroDeckInfo.SelectedDeck;
		}
		return ExtractGameDBPowerDeck(iDeckIndex);
	}

	public int GetGameDBHeroLevel(uint hHeroID)
	{
		int iLevel = 0;
		if (m_mapHeroStatUpgradeInfo.ContainsKey(hHeroID))
		{
			iLevel = m_mapHeroStatUpgradeInfo[hHeroID].HeroLevel;
		}
		return iLevel;
	}

	public uint GetGameDBHeroReaderID()
	{
		return HeroDeckInfo.DeckList[HeroDeckInfo.SelectedDeck].ReaderID;
	}

	public void SetGameDBHeroReaderID(uint hHeroID)
	{
		HeroDeckInfo.DeckList[HeroDeckInfo.SelectedDeck].ReaderID = hHeroID;
	}

	public uint GetGameDBHeroEXP(uint hHeroID)
	{
		uint iEXP = 0;
		if (m_mapHeroStatUpgradeInfo.ContainsKey(hHeroID))
		{
			iEXP = m_mapHeroStatUpgradeInfo[hHeroID].EXPCurrent;
		}

		return iEXP;
	}

	public SPacketHeroStatUpgrade GetGameDBHeroStatUpgrade(uint hHeroID) 
	{
		SPacketHeroStatUpgrade pUpgradeInfo = null;
		if (m_mapHeroStatUpgradeInfo.ContainsKey(hHeroID))
		{
			pUpgradeInfo = m_mapHeroStatUpgradeInfo[hHeroID].CopyInstance<SPacketHeroStatUpgrade>();
		}
		return pUpgradeInfo;
	}

	public uint GetGameDBHeroStatUpgradePoint(uint hHeroID, uint hUpgradeID)
	{
		uint iPoint = 0;
		SPacketHeroStatUpgrade pUpgradeInfo = null;
		if (m_mapHeroStatUpgradeInfo.ContainsKey(hHeroID))
		{
			pUpgradeInfo = m_mapHeroStatUpgradeInfo[hHeroID];
			for (int i = 0; i < pUpgradeInfo.UpgradePoint.Count; i++)
			{
				if (pUpgradeInfo.UpgradePoint[i].Value1 == hUpgradeID)
				{
					iPoint = pUpgradeInfo.UpgradePoint[i].Value2;
				}
			}
		}

		return iPoint;
	}

	public SPacketStatValue GetGameDBHeroStatCache(uint hHeroID) //  최종 스텟값 계산
	{
		SPacketStatValue pStatValue = null;
		if (m_mapHeroStatCache.ContainsKey(hHeroID))
		{
			SPackatStatCache pStatCache = m_mapHeroStatCache[hHeroID];
			pStatValue = pStatCache.StatValue.CopyInstance<SPacketStatValue>();
		}
		return pStatValue;
	}

	public SHScriptTableDescriptionHero.SDescriptionHero GetGameDBReaderTable()
	{
		return SHManagerScriptData.Instance.ExtractTableHero().GetTableDescriptionHero(GetGameDBHeroReaderID());
	}

	public List<uint> GetGameDBHeroEquipID(uint hHeroID)
	{
		List<uint> pListEquip = new List<uint>();
		if (m_mapHeroInfo.ContainsKey(hHeroID))
		{
			SPacketHero pHeroInfo = m_mapHeroInfo[hHeroID];
			pListEquip.Add(pHeroInfo.EquipMain);
			pListEquip.Add(pHeroInfo.EquipSub1);
			pListEquip.Add(pHeroInfo.EquipSub2);
		}

		return pListEquip;
	}

	public SPacketPotentialBingoPage GetGameDBPotentialBingoPage(uint hHeroID, int iPageIndex)
	{
		SPacketPotentialBingoPage pBingoPage = null;
		if (m_mapPotentialBingo.ContainsKey(hHeroID))
		{
			SPacketPotentialBingoInfo pBingoInfo = m_mapPotentialBingo[hHeroID];
			if (iPageIndex < pBingoInfo.BingoPage.Count)
			{
				pBingoPage = pBingoInfo.BingoPage[iPageIndex].CopyInstance<SPacketPotentialBingoPage>();
			}
		}

		return pBingoPage;
	}

	//------------------------------------------------------------------------------
	private void PrivGameDBCacheHeroStat(uint hHeroID)
	{
		SPackatStatCache pStatCache = new SPackatStatCache();
		pStatCache.HeroID = hHeroID;
		m_mapHeroStatCache[hHeroID] = pStatCache;
		// 영웅 기본 수치 더하기 		
		PrivGameDBCacheDefaultStat(pStatCache.StatValue, hHeroID);
		// 일반 업그레이드 수치 더하기 
		PrivGameDBCacheUpgradeStat(pStatCache.StatValue, hHeroID);
		// 잠재력 빙고 수치 더하기 
		PrivGameDBCachePotentialStat(pStatCache.StatValue, hHeroID);
	}

	private void PrivGameDBCacheUpgradeStat(SPacketStatValue pOutStat, uint hHeroID)
	{
		SPacketHeroStatUpgrade pUpgradeInfo = PrivGameDBHeroStatUpgradeInternal(hHeroID);
		if (pUpgradeInfo == null) return;

		SHScriptTableHeroUpgrade pTableUpgrade = SHManagerScriptData.Instance.ExtractTableHeroUpgrade();
		for (int i = 0; i < pUpgradeInfo.UpgradePoint.Count; i++)
		{
			SIntPair pUpgradePoint = pUpgradeInfo.UpgradePoint[i];
			SHScriptTableHeroUpgrade.SHeroUpgradeItem pUpgradeItem = pTableUpgrade.GetTableHeroUpgrade(pUpgradePoint.Value1);
			if (pUpgradeItem != null)
			{
				PrivGameDBCacheUpgradeStatApply(pOutStat, pUpgradeItem, pUpgradePoint.Value2);
			}
		}
	}

	private void PrivGameDBCacheDefaultStat(SPacketStatValue pOutStat, uint hHeroID)
	{
		SHScriptTableDescriptionHero.SDescriptionHero pTableHero = SHManagerScriptData.Instance.ExtractTableHero().GetTableDescriptionHero(hHeroID);

		pOutStat.LevelAttack += pTableHero.Attack;
		pOutStat.LevelAttackSkill += pTableHero.AttackSkill;
		pOutStat.LevelAttackPercent += pTableHero.AttackPercent;

		pOutStat.LevelDefense += pTableHero.Defense;
		pOutStat.LevelDefenseSkill += pTableHero.DefenseSkill;
		pOutStat.LevelDefensePercent += pTableHero.DefensePercent;

		pOutStat.LevelBlock += pTableHero.Block;
		pOutStat.LevelBlockAnti += pTableHero.BlockAnti;

		pOutStat.LevelCritical += pTableHero.Critical;
		pOutStat.LevelCriticalAnti += pTableHero.CriticalAnti;
		pOutStat.LevelCriticalDamage += pTableHero.CriticalDamage;
		pOutStat.LevelCriticalDamageAnti += pTableHero.CriticalDamageAnti;

		pOutStat.LevelDodge += pTableHero.Dodge;
		pOutStat.LevelHit += pTableHero.Hit;

		pOutStat.LevelRecoverPerSecond += pTableHero.RecoverPerSecond;
		pOutStat.LevelStaminaPercent += pTableHero.StaminaPercent;
		pOutStat.LevelStamina += pTableHero.Stamina;

		pOutStat.LevelExtraEXP += pTableHero.ExtraEXP;
		pOutStat.LevelExtraGold += pTableHero.ExtraGold;
		pOutStat.LevelExtraItem += pTableHero.ExtraItem;

	}

	private void PrivGameDBCachePotentialStat(SPacketStatValue pOutStat, uint hHeroID)
	{
		if (m_mapPotentialBingo.ContainsKey(hHeroID) == false) return;

		SPacketPotentialBingoInfo pBingoInfo = m_mapPotentialBingo[hHeroID];
		for (int i = 0; i < pBingoInfo.BingoPage.Count; i++)
		{
			SPacketPotentialBingoPage pBingoPage = pBingoInfo.BingoPage[i];

			for (int j = 0; j < pBingoPage.BingoBoard.Count; j++)
			{
				PrivGameDBCachePotentialStatApply(pOutStat, pBingoPage.BingoBoard[j]);
			}
		}
	}

	private void PrivGameDBCacheUpgradeStatApply(SPacketStatValue pOutStat, SHScriptTableHeroUpgrade.SHeroUpgradeItem pUpgradeItem, uint iPoint)
	{
		if (pUpgradeItem.EUpgradeType != SHScriptTableHeroUpgrade.EUpgradeType.CombatStat) return;

		uint iUpgradeValue = (pUpgradeItem.UpgradeValue * iPoint);

		if (pUpgradeItem.EUpgradeStatType == SHScriptTableHeroUpgrade.EUpgradeStatType.Attack)
		{
			pOutStat.LevelAttack += iUpgradeValue;
		}
		else if (pUpgradeItem.EUpgradeStatType == SHScriptTableHeroUpgrade.EUpgradeStatType.AttackSkill)
		{
			pOutStat.LevelAttackSkill += iUpgradeValue;
		}
		else if (pUpgradeItem.EUpgradeStatType == SHScriptTableHeroUpgrade.EUpgradeStatType.Defense)
		{
			pOutStat.LevelDefense += iUpgradeValue;
		}
		else if (pUpgradeItem.EUpgradeStatType == SHScriptTableHeroUpgrade.EUpgradeStatType.DefenseSkill)
		{
			pOutStat.LevelDefenseSkill += iUpgradeValue;
		}
		else if (pUpgradeItem.EUpgradeStatType == SHScriptTableHeroUpgrade.EUpgradeStatType.Critical)
		{
			pOutStat.LevelCritical += iUpgradeValue;
		}
		else if (pUpgradeItem.EUpgradeStatType == SHScriptTableHeroUpgrade.EUpgradeStatType.CriticalAnti)
		{
			pOutStat.LevelCriticalAnti += iUpgradeValue;
		}
		else if (pUpgradeItem.EUpgradeStatType == SHScriptTableHeroUpgrade.EUpgradeStatType.CriticalDamage)
		{
			pOutStat.LevelCriticalDamage += iUpgradeValue;
		}
		else if (pUpgradeItem.EUpgradeStatType == SHScriptTableHeroUpgrade.EUpgradeStatType.CriticalDamageAnti)
		{
			pOutStat.LevelCriticalDamageAnti += iUpgradeValue;
		}
		else if (pUpgradeItem.EUpgradeStatType == SHScriptTableHeroUpgrade.EUpgradeStatType.Hit)
		{
			pOutStat.LevelHit += iUpgradeValue;
		}
		else if (pUpgradeItem.EUpgradeStatType == SHScriptTableHeroUpgrade.EUpgradeStatType.Dodge)
		{
			pOutStat.LevelDodge += iUpgradeValue;
		}
		else if (pUpgradeItem.EUpgradeStatType == SHScriptTableHeroUpgrade.EUpgradeStatType.RecoverPerSecond)
		{
			pOutStat.LevelRecoverPerSecond += iUpgradeValue;
		}
		else if (pUpgradeItem.EUpgradeStatType == SHScriptTableHeroUpgrade.EUpgradeStatType.Stamina)
		{
			pOutStat.LevelStamina += iUpgradeValue;
		}
		else if (pUpgradeItem.EUpgradeStatType == SHScriptTableHeroUpgrade.EUpgradeStatType.ExtraGold)
		{
			pOutStat.LevelExtraGold += iUpgradeValue;
		}
		else if (pUpgradeItem.EUpgradeStatType == SHScriptTableHeroUpgrade.EUpgradeStatType.ExtraEXP)
		{
			pOutStat.LevelExtraEXP += iUpgradeValue;
		}
		else if (pUpgradeItem.EUpgradeStatType == SHScriptTableHeroUpgrade.EUpgradeStatType.ExtraItem)
		{
			pOutStat.LevelExtraItem += iUpgradeValue;
		}
		else if (pUpgradeItem.EUpgradeStatType == SHScriptTableHeroUpgrade.EUpgradeStatType.AttackPercent)
		{
			pOutStat.LevelAttackPercent += iUpgradeValue;
		}
		else if (pUpgradeItem.EUpgradeStatType == SHScriptTableHeroUpgrade.EUpgradeStatType.AttackPercent)
		{
			pOutStat.LevelAttackPercent += iUpgradeValue;
		}
		else if (pUpgradeItem.EUpgradeStatType == SHScriptTableHeroUpgrade.EUpgradeStatType.DefensePercent)
		{
			pOutStat.LevelDefensePercent += iUpgradeValue;
		}
		else if (pUpgradeItem.EUpgradeStatType == SHScriptTableHeroUpgrade.EUpgradeStatType.AttackPercent)
		{
			pOutStat.LevelStaminaPercent += iUpgradeValue;
		}
	}

	private void PrivGameDBCachePotentialStatApply(SPacketStatValue pOutStat, SPacketPotentialBingoSlot pBingoSlot)
	{
		for (int i = 0; i < pBingoSlot.BingoOption.Count; i++)
		{
			ESHStatType pStatType = (ESHStatType)pBingoSlot.BingoOption[i].StatID;

		}
	}

	private SPacketHeroStatUpgrade PrivGameDBHeroStatUpgradeInternal(uint hHeroID) // DB에저장된 업그레이드 수치만 리턴
	{
		SPacketHeroStatUpgrade pUpgradeInfo = null;
		if (m_mapHeroStatUpgradeInfo.ContainsKey(hHeroID))
		{
			pUpgradeInfo = m_mapHeroStatUpgradeInfo[hHeroID];
		}
		return pUpgradeInfo;
	}
}
