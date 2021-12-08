using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class CEffectBase : CMonoBase
{
	private uint m_hEffectID = 0; public uint GetEffectID() { return m_hEffectID; }
	private event UnityAction<uint, bool> m_delEffectNotify;  // 이펙트 아이디 및 시작/종료

	protected Vector3 m_vecOrigion = Vector3.zero;
	protected Vector3 m_vecDest = Vector3.zero;
	protected float m_fDuration = 0;
	//---------------------------------------------------------------
	public void DoEffectInitialize()
	{
		OnEffectInitialize();
	}

	public void DoEffectStart(Vector3 vecOrigin,  Vector3 vecDest = new Vector3(), float fDuration = 0f)
	{
		m_vecOrigion = vecOrigin;
		m_vecDest = vecDest;
		m_delEffectNotify?.Invoke(m_hEffectID, true);
		OnEffectStart(vecOrigin, vecDest, fDuration);
	}

	public void DoEffectStop()
	{
		OnEffectStop();
	}

	public void DoEffectEnd()
	{
		OnEffectEnd();
		m_delEffectNotify?.Invoke(m_hEffectID, false);
		m_delEffectNotify = null;
	}

	public void DoEffectNotifyAdd(UnityAction<uint, bool> delEffectNotify)
	{
		m_delEffectNotify += delEffectNotify;		
	}

	//--------------------------------------------------------------


	//---------------------------------------------------------------
	protected virtual void OnEffectInitialize() { }
	protected virtual void OnEffectStart(Vector3 vecOrigin, Vector3 vecDest, float fDuration) { }
	protected virtual void OnEffectEnd() { }
	protected virtual void OnEffectStop() { }
}
