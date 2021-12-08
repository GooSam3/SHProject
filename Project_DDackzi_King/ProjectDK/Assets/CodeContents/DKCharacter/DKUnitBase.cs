using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NSkill;
public class DKUnitBase : CUnitSpriteBase , IDKSkillProcessor, IDKCombatAIProcessor
{
	protected DKCharSkillFSM m_pDKSkillFSM = null;
	protected DKCombatAIBase m_pDKCombatAI = null;
	protected DKStatComponent m_pDKStatComponent = null;
	//---------------------------------------------------------------
	protected override void OnUnitInitialize()
	{
		base.OnUnitInitialize();
	}

	protected override void OnUnitUpdate()
	{
		base.OnUnitUpdate();

		if (IsAlive)
		{
			PrivUpdateUnitDirection();
		}	
	}

	protected override void OnUnitSpriteAnimationLoopEnd(string strAnimName) 
	{
		base.OnUnitSpriteAnimationLoopEnd(strAnimName);
		m_pDKSkillFSM.EventAnimationEnd();
	}

	protected override void OnUnitFSMInitialize(CFiniteStateMachineSkillBase pFSM) 
	{
		base.OnUnitFSMInitialize(pFSM);
		m_pDKSkillFSM = pFSM as DKCharSkillFSM;
	}

	protected override void OnUnitCombatAI(CCombatAIBase pCombatAI)
	{
		base.OnUnitCombatAI(pCombatAI);
		m_pDKCombatAI = pCombatAI as DKCombatAIBase;
	}

	protected sealed override void OnUnitStatComponent(CStatComponentBase pStatComponent)
	{
		m_pDKStatComponent = pStatComponent as DKStatComponent;
	}

	protected override int OnUnitFSMSkillPlay(uint hSkillTableID, CSkillUsage pUsage) 
	{
		return m_pDKSkillFSM.DoCharSkillPlay(hSkillTableID, pUsage); 
	}

	protected override void OnUnitDeathStart()
	{
		base.OnUnitDeathStart();
		ProtSpritePlayAnimation(EAnimationType.Anim_Die.ToString(), false);
	}

	protected override void OnUnitDeathEnd()
	{
		base.OnUnitDeathEnd();
	}

	protected override void OnUnitSkillDamageFromResult(SDamageResult rResult)
	{
		base.OnUnitSkillDamageFromResult(rResult);
		DKUnitSocketAssist pSocketAssit = m_pSocketAssist as DKUnitSocketAssist;
		Vector3 vecPosition = pSocketAssit.GetSocketWorldPosition(EUnitSocket.DamagePrint);
		DKUIFrameValuePrint.EPrintType ePrintType = DKUIFrameValuePrint.EPrintType.DamageNormal;
		if (rResult.eDamageResultType == (int)EDamageResultType.Critial)
		{
			ePrintType = DKUIFrameValuePrint.EPrintType.DamageCritical;
		}
		DKUIManager.Instance.DoUIFrameValuePrint(ePrintType, (int)rResult.fDamage, vecPosition);
	}

	protected override void OnUnitSkillHealFromResult(SDamageResult rResult)
	{
		base.OnUnitSkillHealFromResult(rResult);
		DKUnitSocketAssist pSocketAssit = m_pSocketAssist as DKUnitSocketAssist;
		Vector3 vecPosition = pSocketAssit.GetSocketWorldPosition(EUnitSocket.DamagePrint, true);
		DKUIFrameValuePrint.EPrintType ePrintType = DKUIFrameValuePrint.EPrintType.HealNormal;
		if (rResult.eDamageResultType == (int)EDamageResultType.Critial)
		{
			ePrintType = DKUIFrameValuePrint.EPrintType.HealCritical;
		}
		DKUIManager.Instance.DoUIFrameValuePrint(ePrintType, (int)rResult.fDamage, vecPosition);
	}

	protected override void OnUnitNavAgentMoveFinish(ENavAgentEvent eAgentEvent, Vector3 vecPosition) 
	{
		base.OnUnitNavAgentMoveFinish(eAgentEvent, vecPosition);
		ProtUnitSpriteLoopEnd(true); 
	}

	//----------------------------------------------------------------------
	public void EventCharAnimation(EAnimEventType eAnimEventType, int iIndex)
	{
		m_pDKSkillFSM.EventAnimationTaskEvent(eAnimEventType, iIndex);
	}

	public void EventCharAnimationEnd()
	{
		string strAnimName = ProtUnitSpriteGetCurrentAnimation();
		m_pDKSkillFSM.EventAnimationEnd();
		if (strAnimName == EAnimationType.Anim_Die.ToString())
		{
			ProtUnitDeathEnd();
		}
	}

	public void SetDKUnitDirection(bool bLeft)
	{
		ProtSpriteFlip(true, !bLeft);
	}

	public void SetDKUnitDirection(Vector3 vecDest)
	{
		Vector3 vecDirection = vecDest - GetUnitPosition();
		if (vecDirection.x > 0)
		{
			ProtSpriteFlip(true, false);
		}
		else
		{
			ProtSpriteFlip(true, true);
		}
	}

	public void DoUnitMoveTo(Vector3 vecDest)
	{
		ISkillAnimation(EAnimationType.Anim_Walk.ToString(), true, 0, 1);
		ISkillMoveToPosition(vecDest, 0, null);
	}

	//----------------------------------------------------------------------
	public uint IAIGetSkillReadyToUse(EActiveSkillType eActiveSkillType)
	{
		return m_pDKSkillFSM.ExtractReadySkillTableID(eActiveSkillType);
	}

	public void IAIUseSkill(uint hSkillID, CSkillUsage pUsage)
	{
		ISkillPlay(hSkillID, pUsage);
	}

	public float IAIGetUnitStat(EDKStatType eStatType)
	{
		return m_pDKStatComponent.GetDKUnitStat(eStatType);
	}

	//----------------------------------------------------------------------
	private void PrivUpdateUnitDirection()
	{
		Vector3 vecDirection = GetNavAgentDirection();
		if (vecDirection.x > 0)
		{
			ProtSpriteFlip(true, false);
		}
		else if (vecDirection.x < 0)
		{
			ProtSpriteFlip(true, true);
		}
	}

	//------------------------------------------------------------------------
	public float GetDKUnitHP() { return m_pDKStatComponent.GetDKUnitHP(); }
	public float GetDKUnitEnergy() { return m_pDKStatComponent.GetDKUnitEnergy(); }
	public DKUnitBase IGetDKUnit() { return this; }
	public Transform GetDKUnitSocketTransform(EUnitSocket eSocketType) { return (m_pSocketAssist as DKUnitSocketAssist).GetSocketTransform(eSocketType); }
}
