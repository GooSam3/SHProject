using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SHScriptTableStatEnemy : CScriptDataTableBase
{
	public class SUnitStatEnemy
	{
		public uint UnitStatID = 0;
		public int LevelCap = 0;
		public int AttackPower = 0;
		public int DamagePercent = 0;
		public int DefencePower = 0;
		public int ReducePercent = 0;
		public int CriticalRate = 0;
		public int CriticalRateAnti = 0;
		public int CriticalRateDamage = 0;
		public int CriticalRateDamageAnti = 0;
		public int HitRate = 0;
		public int DodgeRate = 0;
		public int BlockRate = 0;
		public int BlockAntiRate = 0;
		public int MaxHitPoint = 0;
	}

	public class SUnitStatGroup
	{
		public uint UnitStatID = 0;
		public List<SUnitStatEnemy> StatLevel = new List<SUnitStatEnemy>();
	}

	private Dictionary<uint, SUnitStatGroup> m_mapStatGroup = new Dictionary<uint, SUnitStatGroup>();

	//--------------------------------------------------------------
	protected override void OnScriptDataInitialize(string strTextData)
	{
		base.OnScriptDataInitialize(strTextData);
		PrivScriptTableLoad();
	}

	//---------------------------------------------------------------
	public SHStatGroupCombat GetUnitStatEnemy(uint hUnitStatID, int iLevel)
	{
		SHStatGroupCombat pStatCombat = null;
		if (m_mapStatGroup.ContainsKey(hUnitStatID))
		{
			pStatCombat = ExtractScriptTableStatCombat(m_mapStatGroup[hUnitStatID], iLevel);
		}
		else
		{
			//Error
			Debug.LogError($"[Stat Enemy] Invalide StatID{hUnitStatID}");
		}
		return pStatCombat;
	}

	//---------------------------------------------------------------
	private void PrivScriptTableLoad()
	{
		List<SUnitStatEnemy> pListUnitEnemy = ProtDataTableRead<SUnitStatEnemy>();

		for (int i = 0; i < pListUnitEnemy.Count; i++)
		{
			SUnitStatEnemy pTableStatEnemy = pListUnitEnemy[i];

			SUnitStatGroup pStatGroup = null;
			if (m_mapStatGroup.ContainsKey(pTableStatEnemy.UnitStatID))
			{
				pStatGroup = m_mapStatGroup[pTableStatEnemy.UnitStatID];
			}
			else
			{
				pStatGroup = new SUnitStatGroup();
				m_mapStatGroup[pTableStatEnemy.UnitStatID] = pStatGroup;
			}

			pStatGroup.UnitStatID = pTableStatEnemy.UnitStatID;
			pStatGroup.StatLevel.Add(pTableStatEnemy);
		}
	}

	private SHStatGroupCombat ExtractScriptTableStatCombat(SUnitStatGroup pStatGroup, int iLevel)
	{
		SHStatGroupCombat pCombatStat = new SHStatGroupCombat();
		if (iLevel == 0) iLevel = 1;

		int iLevelCount = iLevel;
		int iLevelPrivious = 0;
		SUnitStatEnemy pStatEnemy = null;
		for (int i = 0; i < pStatGroup.StatLevel.Count; i++)
		{
			pStatEnemy = pStatGroup.StatLevel[i];
			if (iLevel <= pStatEnemy.LevelCap)
			{
				PrivScriptTableStatCalculate(pCombatStat, pStatEnemy, iLevelCount);
				iLevelCount = 0;
				break;
			}
			else
			{
				iLevelPrivious = pStatEnemy.LevelCap - iLevelPrivious;
				PrivScriptTableStatCalculate(pCombatStat, pStatEnemy, iLevelPrivious);
				if (i == 0)
				{
					iLevelPrivious--;
				}
				iLevelCount -= iLevelPrivious;
			}
		}

		if (iLevelCount > 0 && pStatEnemy != null)
		{
			PrivScriptTableStatCalculate(pCombatStat, pStatEnemy, iLevelCount);
		}

		return pCombatStat;
	}

	private void PrivScriptTableStatCalculate(SHStatGroupCombat pCombatStat, SUnitStatEnemy pStatEnemy, int iCount)
	{
		pCombatStat.AttackPower			+= pStatEnemy.AttackPower * iCount;
		pCombatStat.DamagePercent			+= pStatEnemy.DamagePercent;		// 퍼센트 값은 레벨 스케일링 하지 않는다.
		pCombatStat.DefencePower			+= pStatEnemy.DefencePower * iCount;
		pCombatStat.ReducePercent			+= pStatEnemy.ReducePercent;       // 퍼센트 값은 레벨 스케일링 하지 않는다.
		pCombatStat.CriticalRate			+= pStatEnemy.CriticalRate * iCount;
		pCombatStat.CriticalRateAnti		+= pStatEnemy.CriticalRateAnti * iCount;
		pCombatStat.CriticalDamageRate		+= pStatEnemy.CriticalRateDamage * iCount;
		pCombatStat.CriticalDamageRateAnti	+= pStatEnemy.CriticalRateDamageAnti * iCount;
		pCombatStat.HitRate				+= pStatEnemy.HitRate * iCount;
		pCombatStat.DodgeRate				+= pStatEnemy.DodgeRate * iCount;
		pCombatStat.BlockRate				+= pStatEnemy.BlockRate * iCount;
		pCombatStat.BlockAntiRate			+= pStatEnemy.BlockAntiRate * iCount;
		pCombatStat.MaxHitPoint			+= pStatEnemy.MaxHitPoint * iCount;
	}
}
