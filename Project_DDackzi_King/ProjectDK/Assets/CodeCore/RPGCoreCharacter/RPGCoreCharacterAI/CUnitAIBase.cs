using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract public class CUnitAIBase : CUnitBuffBase, ICombatAIProcessor
{
    private CCombatAIBase m_pCombatAI = null;
	private CFiniteStateMachineSkillBase m_pSkillFSMReference = null;
	//---------------------------------------------
	protected override void OnUnityAwake()
	{
		base.OnUnityAwake();
		m_pCombatAI = GetComponentInChildren<CCombatAIBase>();
		m_pCombatAI.SetCombatAIProcessor(this);
	}

	protected override void OnUnitFSMInitialize(CFiniteStateMachineSkillBase pFSM)
	{
		base.OnUnitFSMInitialize(pFSM);
		m_pSkillFSMReference = pFSM;
	}

	public void SetUnitAIEnable(bool bEnable)
	{
		if (IsAlive == false) return;
		m_pCombatAI.SetCombatAIEnable(bEnable);
	}

	//----------------------------------------------
	public bool IHasSkillPlay()
	{
		return m_pSkillFSMReference.IsSkillPlay;
	}
	public CUnitAIBase IGetCombatUnit()
	{
		return this;
	}

	//--------------------------------------------------
	protected virtual void OnUnitCombatAI(CCombatAIBase pCombatAI) { }
}
