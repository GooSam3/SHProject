using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;



public abstract partial class SHUnitBase : CUnitAIBase, ISHSkillProcessor
{
	protected int m_iTagSlot = 0;									public int GetUnitTagSlot() { return m_iTagSlot; }
	protected SHCombatAIBase		m_pSHCombatAI = null;
	protected SHStatComponentBase	m_pSHStatComponent = null;			public SHStatComponentBase ExportUnitStat() { return m_pSHStatComponent; }
	protected SHBuffComponent		m_pSHBuffComponent = null;			public SHBuffComponent ExportUnitBuffComp() { return m_pSHBuffComponent; }
	protected SHAnimationSpineBase	m_pSHAnimationSpine = null;		
	protected SHUnitSkillFSMBase	m_pSHSkillFSM = null;
	
	//-------------------------------------------------------------------
	protected override void OnUnitInitialize()
	{
		base.OnUnitInitialize();
		ProtUnitStandBy();
	}

	protected override void OnUnitUpdate()
	{
		base.OnUnitUpdate();
		m_pTagCoolTimer.UpdateCoolTime(Time.deltaTime);
	}

	protected override void OnUnitStandBy()
	{
		base.OnUnitStandBy();
		m_pSHBuffComponent.DoBuffComponentClear();
		m_pSHSkillFSM.DoUnitSkillResetCoolTime();
	}

	protected override void OnUnitDeathEnd()
	{
		base.OnUnitDeathEnd();
		m_pSHSkillFSM.DoUnitSkillReset();
		SHManagerUnit.Instance.DoMgrUnitUnRegist(this);
	}

	protected override void OnUnitSpawned(UnityAction delFinish)
	{
		base.OnUnitSpawned(delFinish);
		m_pSHAnimationSpine.SetMonoActive(true);
		CAnimationBase.SAnimationUsage rAnimation = new CAnimationBase.SAnimationUsage();
		rAnimation.AnimName = "spawn";
		rAnimation.bLoop = false;
		rAnimation.fAniSpeed = 1f;
		rAnimation.fDuration = 0;
		m_pSHAnimationSpine.DoAnimationSpineChangeMaterial(SHAnimationSpineBase.ESpineMaterialType.None);
		ProtUnitAnimation(ref rAnimation, (AniName, bFinish) =>
		{
			ProtUnitAlive();
			delFinish?.Invoke();
		}, null);
	}

	protected override void OnUnitForceUpdate(float fDelta)
	{
		base.OnUnitForceUpdate(fDelta);
		m_pSHBuffComponent.DoBuffComponentForceUpdate(fDelta);
	}

	//------------------------------------------------------------------------------
	protected override void OnUnitCombatAI(CCombatAIBase pCombatAI)
	{
		base.OnUnitCombatAI(pCombatAI);
		m_pSHCombatAI = pCombatAI as SHCombatAIBase;
	}

	protected sealed override void OnUnitStatComponent(CStatComponentBase pStatComponent)
	{
		base.OnUnitStatComponent(pStatComponent);
		m_pSHStatComponent = pStatComponent as SHStatComponentBase;
	}

	protected sealed override void OnUnitInitializeAnimation(CAnimationBase pAnimation) 
	{
		base.OnUnitInitializeAnimation(pAnimation);
		m_pSHAnimationSpine = pAnimation as SHAnimationSpineBase;
	}

	protected sealed override void OnUnitBuffComponentInitialize(CBuffComponentBase pBuffComponent)
	{
		base.OnUnitBuffComponentInitialize(pBuffComponent);
		m_pSHBuffComponent = pBuffComponent as SHBuffComponent;
	}

	protected override void OnUnitBuffTo(CUnitBase pTarget, uint hBuffID, float fDuration, float fPower)
	{
		base.OnUnitBuffTo(pTarget, hBuffID, fDuration, fPower);
		SHUnitBase pBuffTarget = pTarget as SHUnitBase;
		m_pSHBuffComponent.DoBuffComponentTo(hBuffID, pBuffTarget);
		pBuffTarget.ISkillBuffFrom(hBuffID, this, fDuration, fPower);
	}

	protected override void OnUnitBuffFrom(uint hBuffID, CUnitBase pBuffOrigin, float fDuration, float fPower)
	{
		base.OnUnitBuffFrom(hBuffID, pBuffOrigin, fDuration, fPower);
		SHUnitBase pUnitOrigin = pBuffOrigin as SHUnitBase;
		m_pSHBuffComponent.DoBuffComponentFrom(hBuffID, pUnitOrigin.ExportUnitBuffComp(), fDuration, fPower);
	}

	protected override void OnUnitBuffEnd(uint hBuffID)
	{
		base.OnUnitBuffEnd(hBuffID);
		m_pSHBuffComponent.DoBuffComponentEnd(hBuffID);
	}

	protected override void OnUnitFSMInitialize(CFiniteStateMachineSkillBase pFSM)
	{
		base.OnUnitFSMInitialize(pFSM);
		m_pSHSkillFSM = pFSM as SHUnitSkillFSMBase;
	}

	protected override int OnUnitFSMSkillPlay(CSkillUsage pUsage) 
	{
		return m_pSHSkillFSM.DoUnitSkillActive(pUsage as SHSkillUsage); 
	}

	protected override void OnUnitSkillCancle()
	{
		base.OnUnitSkillCancle();
		m_pSHSkillFSM.DoUnitSkillReset();
	}

	//--------------------------------------------------------------------------------------
	public void DoUnitSkillActive(int iSlotIndex, SHUnitBase pTarget)
	{
		if (m_bAlive == false) return;
		m_pSHSkillFSM.DoUnitSkillActive(iSlotIndex, pTarget);
	}

	public void DoUnitSkillResetAnimation()
	{
		m_pSHAnimationSpine.DoAnimationIdle();
	}

	public void DoUnitAIEnable(bool bEnable)
	{
		if (bEnable)
		{
			m_pSHCombatAI.DoCombatAIStart();
		}
		else
		{
			m_pSHCombatAI.DoCombatAIEnd();
		}
	}

	public void DoUnitRecoverHP(float fDelta)
	{
		m_pSHStatComponent.DoStatRecoverHP(fDelta);
	}

	public void DoUnitShapeShowHide(bool bShow)
	{
		ProtUnitAnimationShapeShowHide(bShow);
	}

	public void AddUnitBuffNotify(UnityAction<bool, uint> delNotify)
	{
		m_pSHBuffComponent.DoBuffComponentNotify(delNotify);
	}

	public void DoUnitStatDeathStart()
	{
		ProtUnitDeathStart();
	}

	public bool IsUnitSkillPlay()
	{
		return m_pSHSkillFSM.IsSkillPlay;
	}
}
