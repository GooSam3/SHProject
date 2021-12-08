using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NPacketData;
using UnityEngine.UI;
public class SHUIWidgetHeroStatUpgradeItem : CUIWidgetBase
{
	[SerializeField]
	private uint UpgradeID = 0;
	[SerializeField]
	private CImage UpgradeIcon = null;
	[SerializeField]
	private CText UpgradeName = null;
	[SerializeField]
	private CText UpgradeLevel = null;
	[SerializeField]
	private CUIWidgetNumberTextChart StatCurrent = null;
	[SerializeField]
	private CUIWidgetNumberTextChart StatNext = null;
	[SerializeField]
	private CUIWidgetNumberTextChart UpgradeCost = null;

	private SHUIWindowHeroUpgradeBase m_pUpgradeProcessor = null;
	private SHScriptTableHeroUpgrade.SHeroUpgradeItem m_pTableUpgrade = null;
	private int m_iUsePoint = 0;
	private int m_iPointConsume = 0;
	private long m_iGoldConsume = 0;
	//-----------------------------------------------------------------
	protected override void OnUIWidgetInitialize(CUIFrameBase pParentFrame)
	{
		base.OnUIWidgetInitialize(pParentFrame);
		UpgradeCost.gameObject.SetActive(false);
	}
	//-----------------------------------------------------------------
	public void SetHeroStatUpgradeItem(SHUIWindowHeroUpgradeBase pUpgradeProcessor)
	{
		m_pUpgradeProcessor = pUpgradeProcessor;
	}

	public int GetHeroStatUpgradePoint()
	{
		return m_iPointConsume;
	}

	public uint GetHeroStatUpgradeID()
	{
		return UpgradeID;
	}

	public void DoHeroStatUpgradeItemRefresh(uint hHeroID)
	{
		PrivStatUpgradeSetting(UpgradeID);
		m_iPointConsume = 0;
		SPacketHeroStatUpgrade pStatUpgrade = SHManagerGameDB.Instance.GetGameDBHeroStatUpgrade(hHeroID);
		if (pStatUpgrade != null)
		{
			m_iUsePoint = PrivStatUpgradePoint(pStatUpgrade);
			PrivStatUpgradePointValue(m_iUsePoint);
		}
		else
		{
			PrivStatUpgradeValueDefault(hHeroID);
		}
	}

	//--------------------------------------------------------------------------
	private int PrivStatUpgradePoint(SPacketHeroStatUpgrade pStatUpgrade)
	{
		int iPoint = 0;
		for (int i = 0; i < pStatUpgrade.UpgradePoint.Count; i++)
		{
			if (pStatUpgrade.UpgradePoint[i].Value1 == UpgradeID)
			{
				iPoint = (int)pStatUpgrade.UpgradePoint[i].Value2;
				break;
			}
		}
		return iPoint;
	}

	private void PrivStatUpgradePointValue(int iPoint)
	{
		int iValue = (int)(m_pTableUpgrade.UpgradeValue * (float)iPoint);
		PrivStatUpgradeValue((int)iPoint, iValue, (int)((float)iValue + m_pTableUpgrade.UpgradeValue));
		PrivStatUpgradeCostGold(iPoint);
	}

	private void PrivStatUpgradeSetting(uint hUpgradeID)
	{
		m_pTableUpgrade = SHManagerScriptData.Instance.ExtractTableHeroUpgrade().GetTableHeroUpgrade(hUpgradeID);
		Sprite pSprite = SHManagerAtlasLoader.Instance.DoMgrAtlasFindSprite(SHManagerAtlasLoader.EAtlasType.Lobby, m_pTableUpgrade.IconName);
		if (pSprite != null)
		{
			UpgradeIcon.sprite = pSprite;
		}

		UpgradeName.text = m_pTableUpgrade.UpgradeName;
		ContentSizeFitter pSizeFitter = UpgradeName.gameObject.GetComponent<ContentSizeFitter>();
		if (pSizeFitter)
		{
			pSizeFitter.SetLayoutHorizontal();
		}
	}

	private void PrivStatUpgradeValue(int iLevel, int iValueCurrent, int iValueNext)
	{
		UpgradeLevel.text = string.Format("Lv {0}", iLevel.ToString());
		StatCurrent.DoTextNumber(iValueCurrent);
		StatNext.DoTextNumber(iValueNext);
	}

	private void PrivStatUpgradeCostGold(int iPoint)
	{
		long iGoldCost = ExtractHeroStatCostGold(iPoint);
		if (iGoldCost == 0)
		{
			UpgradeCost.gameObject.SetActive(false);
		}
		else
		{
			m_iGoldConsume = iGoldCost;
			UpgradeCost.gameObject.SetActive(true);
			UpgradeCost.DoTextNumber(iGoldCost);
		}
	}


	private void PrivStatUpgradeValueDefault(uint hHeroID)
	{
		SHStatGroupBasic pStatGroup = SHManagerScriptData.Instance.ExtractTableHero().ExtractHeroBasicStat(hHeroID);
		SIntPair pStatDB = ExtractHeroUpgradeStat(pStatGroup, UpgradeID);
		PrivStatUpgradeValue(1, (int)pStatDB.Value1, (int)(pStatDB.Value1 + pStatDB.Value2));
	}

	private SIntPair ExtractHeroUpgradeStat(SHStatGroupBasic pStatGroup, uint hUpgradeID)
	{
		SIntPair iResult = new SIntPair();
		SHScriptTableHeroUpgrade.SHeroUpgradeItem pTableUpgrade = SHManagerScriptData.Instance.ExtractTableHeroUpgrade().GetTableHeroUpgrade(hUpgradeID);
		iResult.Value2 = pTableUpgrade.UpgradeValue;
		if (pTableUpgrade == null) return iResult;

		switch (pTableUpgrade.EUpgradeStatType)
		{
			case SHScriptTableHeroUpgrade.EUpgradeStatType.Attack:
				iResult.Value1 = pStatGroup.Attack;
				break;
			case SHScriptTableHeroUpgrade.EUpgradeStatType.Defense:
				iResult.Value1 = pStatGroup.Defence;
				break;
			case SHScriptTableHeroUpgrade.EUpgradeStatType.AttackSkill:
				iResult.Value1 = pStatGroup.AttackSkill;
				break;
			case SHScriptTableHeroUpgrade.EUpgradeStatType.DefenseSkill:
				iResult.Value1 = pStatGroup.DefenceSkill;
				break;
			case SHScriptTableHeroUpgrade.EUpgradeStatType.Critical:
				iResult.Value1 = pStatGroup.Critical;
				break;
			case SHScriptTableHeroUpgrade.EUpgradeStatType.CriticalAnti:
				iResult.Value1 = pStatGroup.CriticalAnti;
				break;
			case SHScriptTableHeroUpgrade.EUpgradeStatType.CriticalDamage:
				iResult.Value1 = pStatGroup.CriticalDamage;
				break;
			case SHScriptTableHeroUpgrade.EUpgradeStatType.CriticalDamageAnti:
				iResult.Value1 = pStatGroup.CriticalDamageAnti;
				break;
			case SHScriptTableHeroUpgrade.EUpgradeStatType.Hit:
				iResult.Value1 = pStatGroup.Hit;
				break;
			case SHScriptTableHeroUpgrade.EUpgradeStatType.Dodge:
				iResult.Value1 = pStatGroup.Dodge;
				break;
			case SHScriptTableHeroUpgrade.EUpgradeStatType.Block:
				iResult.Value1 = pStatGroup.Block;
				break;
			case SHScriptTableHeroUpgrade.EUpgradeStatType.BlockAnti:
				iResult.Value1 = pStatGroup.BlockAnti;
				break;
			case SHScriptTableHeroUpgrade.EUpgradeStatType.Stamina:
				iResult.Value1 = pStatGroup.Stamina;
				break;
			case SHScriptTableHeroUpgrade.EUpgradeStatType.RecoverPerSecond:
				iResult.Value1 = pStatGroup.RecoverPerSecond;
				break;
			case SHScriptTableHeroUpgrade.EUpgradeStatType.ExtraEXP:
				iResult.Value1 = pStatGroup.ExtraEXP;
				break;
			case SHScriptTableHeroUpgrade.EUpgradeStatType.ExtraItem:
				iResult.Value1 = pStatGroup.ExtraItem;
				break;
			case SHScriptTableHeroUpgrade.EUpgradeStatType.ExtraGold:
				iResult.Value1 = pStatGroup.ExtraGold;
				break;
		}

		return iResult;
	}

	private long ExtractHeroStatCostGold(int iPoint)
	{
		double fCost = 0;
		if (m_pTableUpgrade.ECostType == SHScriptTableHeroUpgrade.ECostType.Gold)
		{
			fCost = (double)m_pTableUpgrade.CostValue;
			for (int i = 0; i < iPoint; i++)
			{
				fCost *= (double)m_pTableUpgrade.CostLevelValue;
			}
		}

		return (long)fCost;
	}
	//--------------------------------------------------------------------
	public void HandleUpgradePlus()
	{
		int iPoint = m_iUsePoint + m_iPointConsume;
		if (m_pUpgradeProcessor.DoHeroUpgradePlus(iPoint))
		{
			m_iPointConsume++;
			PrivStatUpgradePointValue(m_iUsePoint + m_iPointConsume);
		}
	}

	public void HandleUpgradePlusGold()
	{
		if (m_pUpgradeProcessor.DoHeroUpgradePlus(m_iGoldConsume))
		{
			m_iPointConsume++;
			PrivStatUpgradePointValue(m_iUsePoint + m_iPointConsume);
		}
	}
}
