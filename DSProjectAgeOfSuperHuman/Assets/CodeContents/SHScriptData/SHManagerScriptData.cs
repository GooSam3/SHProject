using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SHManagerScriptData : CManagerScriptDataBase
{ public static new SHManagerScriptData Instance { get { return CManagerScriptDataBase.Instance as SHManagerScriptData; } }

	private SHScriptSkill	m_pScriptSkill = null;
	private SHScriptBuff	m_pScriptBuff = null;
	private SHScriptTableDescriptionEnemy	m_pTableEnemy = null;			public SHScriptTableDescriptionEnemy		ExtractTableEnemy() { return m_pTableEnemy; }
	private SHScriptTableDescriptionHero	m_pTableHero = null;			public SHScriptTableDescriptionHero			ExtractTableHero() { return m_pTableHero; }
	private SHScriptTableDescriptionSkill	m_pTableSkill = null;			public SHScriptTableDescriptionSkill		ExtractTableSkill() { return m_pTableSkill; }
	private SHScriptTableStage				m_pTableStage = null;			public SHScriptTableStage					ExtractTableStage() { return m_pTableStage; }
	private SHScriptTableStatEnemy			m_pTableStatEnemy = null;		public SHScriptTableStatEnemy				ExtractTableStatEnemy() { return m_pTableStatEnemy; }
	private SHScriptTableHeroUpgrade		m_pTableStatHeroUpgreade = null; public SHScriptTableHeroUpgrade			ExtractTableHeroUpgrade() { return m_pTableStatHeroUpgreade; }
	private SHScriptTableGameConfig			m_pTableGameConfig = null;		public SHScriptTableGameConfig				ExtractTableGameConfig() { return m_pTableGameConfig; }
	private SHScriptTableReward			m_pTableReward = null;			public SHScriptTableReward					ExtractTableReward() { return m_pTableReward; }
	private SHScriptTableRewardGroup		m_pTableRewardGroup = null;	public SHScriptTableRewardGroup				ExtractTableRewardGroup() { return m_pTableRewardGroup; }
	private SHScriptTableItem				m_pTableItem = null;			public SHScriptTableItem					ExtractTableItem() { return m_pTableItem; }
	private SHScriptTableLevel				m_pTableLevel = null;			public SHScriptTableLevel					ExtractTableLevel() { return m_pTableLevel; }
	//------------------------------------------------------------------
	protected override void OnScriptDataInitialize(CScriptDataBase pScriptData)
	{
		SHScriptSkill pScriptSkill = pScriptData as SHScriptSkill;
		if (pScriptSkill != null)
		{
			m_pScriptSkill = pScriptSkill;
		}

		SHScriptBuff pScriptBuff = pScriptData as SHScriptBuff;
		if (pScriptBuff)
		{
			m_pScriptBuff = pScriptBuff;
		}

		SHScriptTableDescriptionEnemy pTableEnemy = pScriptData as SHScriptTableDescriptionEnemy;
		if (pTableEnemy)
		{
			m_pTableEnemy = pTableEnemy;
		}

		SHScriptTableDescriptionHero pTableHero = pScriptData as SHScriptTableDescriptionHero;
		if (pTableHero)
		{
			m_pTableHero = pTableHero;
		}

		SHScriptTableDescriptionSkill pTableSkill = pScriptData as SHScriptTableDescriptionSkill;
		if (pTableSkill)
		{
			m_pTableSkill = pTableSkill;
		}

		SHScriptTableStage pTableStage = pScriptData as SHScriptTableStage;
		if (pTableStage)
		{
			m_pTableStage = pTableStage;
		}

		SHScriptTableStatEnemy pTableStatEnemy = pScriptData as SHScriptTableStatEnemy;
		if (pTableStatEnemy)
		{
			m_pTableStatEnemy = pTableStatEnemy;
		}

		SHScriptTableHeroUpgrade pTableStatHeroUpgrade = pScriptData as SHScriptTableHeroUpgrade;
		if (pTableStatHeroUpgrade)
		{
			m_pTableStatHeroUpgreade = pTableStatHeroUpgrade;
		}

		SHScriptTableGameConfig pTableGameConfig = pScriptData as SHScriptTableGameConfig;
		if (pTableGameConfig)
		{
			m_pTableGameConfig = pTableGameConfig;
		}

		SHScriptTableReward pTableReward = pScriptData as SHScriptTableReward;
		if (pTableReward)
		{
			m_pTableReward = pTableReward;
		}

		SHScriptTableRewardGroup pTableRewardGroup = pScriptData as SHScriptTableRewardGroup;
		if (pTableRewardGroup)
		{
			m_pTableRewardGroup = pTableRewardGroup;
		}

		SHScriptTableItem pTableItem = pScriptData as SHScriptTableItem;
		if (pTableItem)
		{
			m_pTableItem = pTableItem;
		}

		SHScriptTableLevel pTableLevel = pScriptData as SHScriptTableLevel;
		if (pTableLevel)
		{
			m_pTableLevel = pTableLevel;
		}
	}

	//---------------------------------------------------------------------
	public SHSkillDataActive DoLoadSkillActive(uint hSkillID)
	{
		if (m_pScriptSkill == null) return null;

		return m_pScriptSkill.DoScriptSkillActive(hSkillID);
	}
	public SHSkillDataPassive DoLoadSkillPassive(uint hSkillID)
	{
		if (m_pScriptSkill == null) return null;

		return m_pScriptSkill.DoScriptSkillPassive(hSkillID);
	}
	public SHBuffInstance DoLoadBuff(uint hBuffID)
	{
		if (m_pScriptSkill == null) return null;

		return m_pScriptBuff.DoScriptBuffMakeInstance(hBuffID);
	}

	//--------------------------------------------------------------------
	public SHScriptTableDescriptionHero.SDescriptionHero GetScriptDataHero(uint hHeroID)
	{
		return ExtractTableHero().GetTableDescriptionHero(hHeroID);
	}
	
}
