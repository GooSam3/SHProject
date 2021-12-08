using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract public class CUnitStatBase : CUnitAnimationBase
{
	protected CStatComponentBase m_pStatComponent = null;
	//------------------------------------------------------------
	protected override void OnUnityAwake()
	{
		base.OnUnityAwake();
		
	}

	protected override void OnUnitInitialize()
	{
		base.OnUnitInitialize();
		m_pStatComponent = GetComponentInChildren<CStatComponentBase>();
		m_pStatComponent.ImportStatInitialize(this);

		OnUnitStatComponent(m_pStatComponent);
	}

	//---------------------------------------------------------------
	protected virtual void OnUnitStatComponent(CStatComponentBase pStatComponent) { }
	
}
