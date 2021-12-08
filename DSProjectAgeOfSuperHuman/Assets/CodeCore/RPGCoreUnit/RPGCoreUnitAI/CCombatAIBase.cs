using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICombatAIProcessor
{
	public bool			IHasSkillPlay();
	public CUnitAIBase	IGetCombatUnit();
}


public abstract class CCombatAIBase : CMonoBase
{
	private bool m_bEnableAI = false; public bool GetCombatAIEnable() { return m_bEnableAI;} protected void SetCombatAIEnable(bool bEnalbe) { m_bEnableAI = bEnalbe;}
	private ICombatAIProcessor m_pCombatAIProcessor = null;
	private CUnitAIBase m_pUnit = null;
	//-----------------------------------------------------------------
	private void Update()
	{
		if (m_bEnableAI)
		{
			if (m_pUnit != null)
			{
				OnCombatAIUpdate();
			}
		}
	}

	internal void SetCombatAIProcessor(ICombatAIProcessor pCombatAIProcessor)
	{
		m_pCombatAIProcessor = pCombatAIProcessor;
		m_pUnit = pCombatAIProcessor.IGetCombatUnit();
		OnCombatAIProcessor(pCombatAIProcessor);
	}

	internal void DoCombatAIReset()
	{
		OnCombatAIReset();
	}

	//---------------------------------------------------------------
	protected virtual void OnCombatAIUpdate() { }
	protected virtual void OnCombatAIReset() { }
	protected virtual void OnCombatAIProcessor(ICombatAIProcessor pCombatAIProcessor) { }
}
