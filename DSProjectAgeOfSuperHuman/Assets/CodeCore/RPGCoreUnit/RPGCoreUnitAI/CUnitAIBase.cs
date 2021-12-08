using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract public class CUnitAIBase : CUnitSkillCombatBase, ICombatAIProcessor
{
    private CCombatAIBase m_pCombatAI = null;
	private CFiniteStateMachineSkillBase m_pSkillFSMReference = null;
	//---------------------------------------------

	protected override void OnUnitInitialize()
	{
		base.OnUnitInitialize();
		m_pCombatAI = GetComponentInChildren<CCombatAIBase>();
		m_pCombatAI.SetCombatAIProcessor(this);
		OnUnitCombatAI(m_pCombatAI);
	}

	protected override void OnUnitFSMInitialize(CFiniteStateMachineSkillBase pFSM)
	{
		base.OnUnitFSMInitialize(pFSM);
		m_pSkillFSMReference = pFSM;
	}

	protected override void OnUnitReset()
	{
		base.OnUnitReset();
		m_pCombatAI.DoCombatAIReset();
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
