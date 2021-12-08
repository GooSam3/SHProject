using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract public class CUnitBuffBase : CUnitStatBase
{
	private CBuffComponentBase m_pBuffComponent = null;
	//----------------------------------------------------------
	protected override void OnUnityAwake()
	{
		base.OnUnityAwake();
	}

	protected override void OnUnitStatComponent(CStatComponentBase pStatComponent)
	{
		base.OnUnitStatComponent(pStatComponent);
		m_pBuffComponent = GetComponentInChildren<CBuffComponentBase>();
		m_pBuffComponent.ImportBuffComponentInitialize(this, pStatComponent);
		OnUnitBuffComponentInitialize(m_pBuffComponent);
	}

	//----------------------------------------------------------
	protected virtual void OnUnitBuffComponentInitialize(CBuffComponentBase pBuffComponent) { }
}
