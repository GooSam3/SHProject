using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SHScriptTableHeroUpgrade : CScriptDataTableBase
{
	public enum EUpgradeType
	{
		None,
		CombatStat,
		SkillPoint,
	}

	public enum EUpgradeStatType
	{
		None,
		Attack,
		Defense,
		Stamina,

		AttackPercent,
		DefensePercent,
		StaminaPercent,

		AttackSkill,
		DefenseSkill,

		Critical,
		CriticalAnti,

		CriticalDamage,
		CriticalDamageAnti,

		Hit,
		Dodge,

		Block,
		BlockAnti,

		RecoverPerSecond,
		
		ExtraGold,
		ExtraEXP,
		ExtraItem,
		//----------------------------------------
		SkillReader,
		SkillNormal,
		SkillCombo,
		SkillSlot1,
		SkillSlot2,
		SkillSlot3,
		SkillSlot4,

	}

	public enum ECostType
	{
		None,
		LevelPoint,
		Gold,
		Soul,
	}

	public class SHeroUpgradeItem  : CObjectInstanceBase // ToDo 보안필드, 암호화 할것
	{
		public uint				UpgradeID = 0;
		public string				UpgradeName;
		public EUpgradeType		EUpgradeType = EUpgradeType.None;
		public EUpgradeStatType	EUpgradeStatType = EUpgradeStatType.None;
		public ECostType			ECostType = ECostType.None;
		public float				CostValue = 0;
		public float				CostLevelValue = 0;
		public uint				UpgradeValue = 0;
		public string				IconName;
	}

	private Dictionary<uint, SHeroUpgradeItem> m_mapHeroUpgrade = new Dictionary<uint, SHeroUpgradeItem>();
	//---------------------------------------------------------------
	protected override void OnScriptDataInitialize(string strTextData)
	{		
		base.OnScriptDataInitialize(strTextData);
		PrivScriptTableLoad();
	}
	//----------------------------------------------------------------
	private void PrivScriptTableLoad()
	{
		List<SHeroUpgradeItem> pListTable = ProtDataTableRead<SHeroUpgradeItem>();
	
		for (int i = 0; i < pListTable.Count; i++)
		{
			SHeroUpgradeItem pUpgradeTable = pListTable[i];
			m_mapHeroUpgrade.Add(pUpgradeTable.UpgradeID, pUpgradeTable);		 
		}
	}

	//----------------------------------------------------------------
	public SHeroUpgradeItem GetTableHeroUpgrade(uint hUpgradeID)
	{
		SHeroUpgradeItem pHeroUpgrade = null;
		if (m_mapHeroUpgrade.ContainsKey(hUpgradeID))
		{
			pHeroUpgrade = m_mapHeroUpgrade[hUpgradeID].CopyInstance<SHeroUpgradeItem>();
		}
		return pHeroUpgrade;
	}	

	public uint GetTableHeroUpgradeCost(uint hUpgradeID, uint iPoint)
	{
		float fCost = 0;
		if (m_mapHeroUpgrade.ContainsKey(hUpgradeID))
		{
			SHeroUpgradeItem pTableUpgrade = m_mapHeroUpgrade[hUpgradeID];
			fCost = pTableUpgrade.CostValue;
			for (int i = 0; i < iPoint; i++)
			{
				fCost *= pTableUpgrade.CostLevelValue;
			}
		}
		return (uint)fCost;
	}

	public uint GetTableHeroUpgradeValue(uint hUpgradeID, uint iPoint)
	{
		uint iValue = 0;
		if (m_mapHeroUpgrade.ContainsKey(hUpgradeID))
		{
			SHeroUpgradeItem pTableUpgrade = m_mapHeroUpgrade[hUpgradeID];
			iValue = pTableUpgrade.UpgradeValue * iPoint;
		}
		return iValue;
	}
}
