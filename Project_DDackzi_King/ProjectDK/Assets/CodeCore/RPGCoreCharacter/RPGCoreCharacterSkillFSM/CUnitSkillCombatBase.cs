using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
abstract public class CUnitSkillCombatBase : CUnitNevAgentBase, ISkillProcessor
{
	private CCoolTime m_pCoolTime = new CCoolTime();
	private CCoolTime m_pCoolTimeGlobal = new CCoolTime();

	private CFiniteStateMachineSkillBase m_pSkillFSM = null;
	//-----------------------------------------------------
	protected override void OnUnityAwake()
	{
		base.OnUnityAwake();
		m_pSkillFSM = GetComponentInChildren<CFiniteStateMachineSkillBase>();
	}

	protected override void OnUnitUpdate()
	{
		base.OnUnitUpdate();
		m_pCoolTime.UpdateCoolTime(Time.deltaTime);
		m_pCoolTimeGlobal.UpdateCoolTime(Time.deltaTime);
	}

	protected override void OnUnitInitialize()
	{
		base.OnUnitInitialize();
		m_pSkillFSM.ImportFSMInitialize(this);
		OnUnitFSMInitialize(m_pSkillFSM);
	}
	//--------------------------------------------------------------------
	internal CStatComponentBase ImportExtractStatComponent()
	{
		return ExtractStatComponent();
	}

	//--------------------------------------------------------------------
	public float IGetSkillCoolTime(string strCooltimeName)
	{
		return m_pCoolTime.GetCoolTime(strCooltimeName);
	}
	public float IGetSkillCoolTimeGlobal(string strGlobalName)
	{
		return m_pCoolTimeGlobal.GetCoolTime(strGlobalName);
	}
	public void ISetSkillCoolTime(string strCoolTimeName, float fDuration)
	{
		m_pCoolTime.SetCoolTime(strCoolTimeName, fDuration);
	}
	public void ISetSkillCoolTimeGlobal(string strCoolTimeName, float fDuration)
	{
		m_pCoolTimeGlobal.SetCoolTime(strCoolTimeName, fDuration);
	}
	public int ISkillPlay(uint hSkillID, CSkillUsage pUsage)
	{
		return OnUnitFSMSkillPlay(hSkillID, pUsage);
	}
	public int ISkillPlayInterrupt(CSkillDataActive pSkillData, CSkillUsage pUsage)
	{
		return OnUnitFSMSkillInterrupt(pSkillData, pUsage);
	}
	public int ISkillCondition(uint hSkillID)
	{
		return m_pSkillFSM.ImportFSMSkillCondition(hSkillID);
	}
	public void ISkillAnimation(string strAnimName, bool bLoop, float fDuration, float fAniSpeed)
	{
		OnUnitSkillAnimation(strAnimName, bLoop, fDuration, fAniSpeed);
	}
	public void ISkillMoveToTarget(CUnitBase pTarget, float fStopRange, UnityAction<ENavAgentEvent, Vector3> delFinish)
	{
		ProtNavMoveObject(pTarget, fStopRange, delFinish);
	}
	public void ISkillMoveToPosition(Vector3 vecDest, float fStopRange, UnityAction<ENavAgentEvent, Vector3> delFinish)
	{
		if (IsAlive == false) return;
		ProtNavMoveDestination(vecDest, fStopRange, delFinish);
	}
	public void ISkillDamageTo(CUnitBase pDest, CUnitBuffBase.SDamageResult rResult)
	{
		if (IsAlive == false || pDest.IsAlive == false) return;

		OnUnitSkillDamageTo(pDest as CUnitSkillCombatBase, rResult);
	}
	public void ISkillDamageFrom(CUnitBase pOrigin, CUnitBuffBase.SDamageResult rResult)
	{
		if (IsAlive == false) return;
		OnUnitSkillDamageFrom(pOrigin as CUnitSkillCombatBase, rResult);
	}
	public void ISkillHealTo(CUnitBase pDest, CUnitBuffBase.SDamageResult rResult)
	{
		OnUnitSkillHealTo(pDest as CUnitSkillCombatBase, rResult);
	}
	public void ISkillHealFrom(CUnitBase pOrigin, CUnitBuffBase.SDamageResult rResult)
	{
		OnUnitSkillHealFrom(pOrigin as CUnitSkillCombatBase, rResult);
	}

	public EUnitRelationType IGetUnitRelationType(CUnitBase pCompareUnit)
	{
		return ProtUnitExtractRelationType(pCompareUnit);
	}
	public EUnitControlType	IGetUnitControlType() { return m_eUnitControlType; }
	public CUnitBase			IGetUnit() { return this; }
	//-------------------------------------------------------------------------
	protected virtual void OnUnitSkillAnimation(string strAnimName, bool bLoop, float fDuration, float fAniSpeed) { }
	protected virtual void OnUnitFSMInitialize(CFiniteStateMachineSkillBase pFSM) { }
	protected virtual int  OnUnitFSMSkillPlay(uint hSkillTableID, CSkillUsage pUsage) { return 0; }
	protected virtual int	 OnUnitFSMSkillInterrupt(CSkillDataActive pSkillData, CSkillUsage pUsage) { return 0; }
	protected virtual void OnUnitSkillDamageTo(CUnitSkillCombatBase pDest, CUnitBuffBase.SDamageResult rResult) { }
	protected virtual void OnUnitSkillDamageFrom(CUnitSkillCombatBase pOrigin, CUnitBuffBase.SDamageResult rResult) { }
	protected virtual void OnUnitSkillHealTo(CUnitSkillCombatBase pDest, CUnitBuffBase.SDamageResult rResult) { }
	protected virtual void OnUnitSkillHealFrom(CUnitSkillCombatBase pOrigin, CUnitBuffBase.SDamageResult rResult) { }
	protected virtual CStatComponentBase ExtractStatComponent() { return null; }

}
