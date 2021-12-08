using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CUnitAnimationBase : CUnitMovableBase
{
	private CAnimationBase m_pAnimation = null;
	//-------------------------------------------------------
	protected override void OnUnityAwake()
	{
		base.OnUnityAwake();
	}

	protected override void OnUnitInitialize()
	{
		base.OnUnitInitialize();
		m_pAnimation = GetComponentInChildren<CAnimationBase>();
		if (m_pAnimation != null)
		{
			m_pAnimation.ImportAnimationInitialize(this);
			OnUnitInitializeAnimation(m_pAnimation);
		}
	}

	//----------------------------------------------------------
	protected void ProtUnitAnimation(ref CAnimationBase.SAnimationUsage rAnimUsage, UnityAction<string, bool> delAnimationEnd, UnityAction<string, int, float> delAnimEvent)
	{
		m_pAnimation.DoAnimationStart(ref rAnimUsage, delAnimationEnd, delAnimEvent);
	}

	protected void ProtUnitAnimationIdle()
	{
		m_pAnimation.DoAnimationIdle();
	}

	protected void ProtUnitAnimationShapeShowHide(bool bShow)
	{
		m_pAnimation.DoAnimationShapeShowHide(bShow);
	}

	//----------------------------------------------------------
	protected virtual void OnUnitInitializeAnimation(CAnimationBase pAnimation) { }
}



