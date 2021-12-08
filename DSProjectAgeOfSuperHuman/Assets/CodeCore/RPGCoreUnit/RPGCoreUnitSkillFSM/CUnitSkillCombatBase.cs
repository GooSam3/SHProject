using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
abstract public class CUnitSkillCombatBase : CUnitBuffBase, ISkillProcessor
{
	private CCoolTime m_pCoolTime = new CCoolTime();
	private CCoolTime m_pCoolTimeGlobal = new CCoolTime();

	private CFiniteStateMachineSkillBase m_pSkillFSM = null;
	//-----------------------------------------------------
	protected override void OnUnityAwake()
	{
		base.OnUnityAwake();
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
		m_pSkillFSM = GetComponentInChildren<CFiniteStateMachineSkillBase>();
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
	public void ISetSkillCoolTimeReduce(string strCoolTimeName, float fReduceValue)
	{
		m_pCoolTime.SetCoolTimeReduce(strCoolTimeName, fReduceValue);
	}
	public void ISetSkillCoolTimeGlobal(string strCoolTimeName, float fDuration)
	{
		m_pCoolTimeGlobal.SetCoolTime(strCoolTimeName, fDuration);
	}
	public int ISkillPlay(CSkillUsage pUsage)
	{
		return OnUnitFSMSkillPlay(pUsage);
	}
	public int ISkillPlayInterrupt(CSkillDataActive pSkillData, CSkillUsage pUsage)
	{
		return OnUnitFSMSkillInterrupt(pSkillData, pUsage);
	}
	public int ISkillCondition(uint hSkillID)
	{
		return m_pSkillFSM.ImportFSMSkillCondition(hSkillID);
	}
	public void ISkillAnimation(ref CAnimationBase.SAnimationUsage rAnimUsage, UnityAction<string, bool> delAnimationEnd, UnityAction<string, int, float> delAnimationEvent)
	{
		ProtUnitAnimation(ref rAnimUsage, delAnimationEnd, delAnimationEvent);
	}
	public void ISkillAnimationReset()
	{
		ProtUnitAnimationIdle();
	}

	public void ISkillMoveToTarget(CUnitBase pTarget, float fStopRange, UnityAction<CNavAgentBase.ENavAgentEvent, Vector3> delFinish)
	{
		if (IsAlive == false) return;
		ProtNavMoveObject(pTarget, fStopRange, delFinish);
	}
	public void ISkillMoveToPosition(Vector3 vecDest, float fStopRange, UnityAction<CNavAgentBase.ENavAgentEvent, Vector3> delFinish)
	{
		if (IsAlive == false) return;
		ProtNavMoveDestination(vecDest, fStopRange, delFinish);
	}

	public void ISkillBuffTo(CUnitBase pTarget, uint hBuffID, float fDuration, float fPower)
	{
		if (IsAlive == false) return;
		OnUnitBuffTo(pTarget, hBuffID, fDuration, fPower);
	}
	public void ISkillBuffFrom(uint hBuffID, CUnitBase pBuffOrigin, float fDuration, float fPower)
	{
		if (IsAlive == false) return;
		OnUnitBuffFrom(hBuffID, pBuffOrigin, fDuration, fPower);
	}
	public void ISkillBuffEnd(uint hBuffID)
	{
		OnUnitBuffEnd(hBuffID);
	}

	public void ISkillCancle()
	{
		OnUnitSkillCancle();
	}

	//--------------------------------------------------------------------------
	public EUnitRelationType IGetUnitRelationType()
	{
		return m_eUnitRelation;
	}
	public EUnitControlType IGetUnitControlType() { return m_eUnitControlType; }
	public CUnitBase IGetUnit() { return this; }
	//-------------------------------------------------------------------------

	protected virtual void OnUnitFSMInitialize(CFiniteStateMachineSkillBase pFSM) { }
	protected virtual int  OnUnitFSMSkillPlay(CSkillUsage pUsage) { return 0; }
	protected virtual int  OnUnitFSMSkillInterrupt(CSkillDataActive pSkillData, CSkillUsage pUsage) { return 0; }
	protected virtual void OnUnitBuffTo(CUnitBase pTarget, uint hBuffID, float fDuration, float fPower) { }
	protected virtual void OnUnitBuffFrom(uint hBuffID, CUnitBase pBuffOrigin, float fDuration, float fPower) { }
	protected virtual void OnUnitBuffEnd(uint hBuffID) { }
	protected virtual void OnUnitSkillCancle() { }
	protected virtual CStatComponentBase ExtractStatComponent() { return null; }

}
