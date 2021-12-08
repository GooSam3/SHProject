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
	private bool m_bEnableAI = false; public bool GetCombatAIEnable() { return m_bEnableAI;} public void SetCombatAIEnable(bool bEnalbe) { m_bEnableAI = bEnalbe;}
	private ICombatAIProcessor m_pCombatAIProcessor = null;
	//-----------------------------------------------------------------
	private void Update()
	{
		if (m_bEnableAI)
		{
			OnCombatAIUpdate();
		}
	}

	internal void SetCombatAIProcessor(ICombatAIProcessor pCombatAIProcessor)
	{
		m_pCombatAIProcessor = pCombatAIProcessor;
		OnCombatAIProcessor(pCombatAIProcessor);
	}
	

	//---------------------------------------------------------------
	protected virtual void OnCombatAIUpdate() { }
	protected virtual void OnCombatAIProcessor(ICombatAIProcessor pCombatAIProcessor) { }
}
