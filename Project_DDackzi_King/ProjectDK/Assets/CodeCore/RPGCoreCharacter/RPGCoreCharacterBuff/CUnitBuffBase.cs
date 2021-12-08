using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract public class CUnitBuffBase : CUnitStatBase
{
	public struct SDamageResult
	{
		public uint hSkillID;
		public uint hBuffID;
		public CUnitBase pAttacker;
		public CUnitBase pDefender;
		public int	eDamageType;
		public int   eDamageResultType;
		public float fDamageRate;
		public float fDamage;
		public float fAggroRate;
		public float fAggro;
	}

	private CBuffProcessorBase m_pBuffProcessor = null;

	//----------------------------------------------------------
	protected override void OnUnityAwake()
	{
		base.OnUnityAwake();
		m_pBuffProcessor = GetComponentInChildren<CBuffProcessorBase>();
	}

	protected override void OnUnitSkillDamageTo(CUnitSkillCombatBase pDest, SDamageResult rResult) 
	{
		rResult = m_pBuffProcessor.ImportBuffProcessDamageCalculation(m_pStatComponent, pDest.ImportExtractStatComponent(),  rResult);
		pDest.ISkillDamageFrom(this, rResult);
		OnUnitSkillDamageToResult(rResult);
	}
	protected override void OnUnitSkillDamageFrom(CUnitSkillCombatBase pOrigin, SDamageResult rResult)
	{
		rResult = m_pBuffProcessor.ImportBuffProcessDamageApply(m_pStatComponent, pOrigin.ImportExtractStatComponent(), rResult);
		OnUnitSkillDamageFromResult(rResult);
	}
	protected override void OnUnitSkillHealTo(CUnitSkillCombatBase pDest, SDamageResult rResult) 
	{
		rResult = m_pBuffProcessor.ImportBuffProcessHealCalculation(m_pStatComponent, pDest.ImportExtractStatComponent(), rResult);
		pDest.ISkillHealTo(this, rResult);
		OnUnitSkillHealToResult(rResult);
	}
	protected override void OnUnitSkillHealFrom(CUnitSkillCombatBase pOrigin, SDamageResult rResult) 
	{
		rResult = m_pBuffProcessor.ImportBuffProcessHeadApply(m_pStatComponent, pOrigin.ImportExtractStatComponent(), rResult);
		OnUnitSkillHealFromResult(rResult);
	}

	//---------------------------------------------------------
	protected virtual void OnUnitSkillDamageToResult(SDamageResult rResult) { }
	protected virtual void OnUnitSkillDamageFromResult(SDamageResult rResult) { }
	protected virtual void OnUnitSkillHealToResult(SDamageResult rResult) { }
	protected virtual void OnUnitSkillHealFromResult(SDamageResult rResult) { }
}
