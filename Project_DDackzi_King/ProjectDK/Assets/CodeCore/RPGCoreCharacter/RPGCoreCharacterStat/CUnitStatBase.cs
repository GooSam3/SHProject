using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract public class CUnitStatBase : CUnitSkillCombatBase, IStatOwner
{
	protected CStatComponentBase m_pStatComponent = null;
	//------------------------------------------------------------
	protected override void OnUnityAwake()
	{
		base.OnUnityAwake();
		m_pStatComponent = GetComponentInChildren<CStatComponentBase>();
		m_pStatComponent.ImportStatComponentArrange(this);
		OnUnitStatComponent(m_pStatComponent);
	}

	protected sealed override CStatComponentBase ExtractStatComponent()
	{
		return m_pStatComponent;
	}

	protected override void OnUnitInitialize()
	{
		base.OnUnitInitialize();
		m_pStatComponent.ImportStatReset();
	}

	//------------------------------------------------------------
	public void IStatUpdate(int hStatType, float fStatValue)
	{
		OnUnitStatUpdateStat(hStatType, fStatValue);
	}

	public void IStatDie(CUnitBase pKiller)
	{
		ProtUnitDeathStart();
	}

	//---------------------------------------------------------------
	protected virtual void OnUnitStatUpdateStat(int hStatType, float fStatValue) { }
	protected virtual void OnUnitStatComponent(CStatComponentBase pStatComponent) { }
	
}
