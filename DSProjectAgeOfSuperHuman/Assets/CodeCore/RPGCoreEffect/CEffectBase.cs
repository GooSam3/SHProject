using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class CEffectBase : CMonoBase
{
	private  bool m_bActive = false;  public bool IsActive { get { return m_bActive; } }
	private  Transform m_pFollowTransform = null;
	private  UnityAction<CEffectBase, bool> m_delEffectNotify;  // 이펙트 아이디 및 시작/종료

	protected Vector3 m_vecOrigin = Vector3.zero;
	protected Vector3 m_vecDest = Vector3.zero;
	protected Vector3 m_vecOffset = Vector3.zero;
	protected float m_fDuration = 0;

	private float m_fDurationCurrent = 0;
	private bool m_bInitialize = false;

	//-------------------------------------------------------------
	protected override void OnUnityAwake()
	{
		base.OnUnityAwake();
		m_vecOffset = transform.position;
		SetMonoActive(false);
		DoEffectInitialize();
	}

	private void Update()
	{
		if (m_bActive)
		{
			UpdateEffectDuration(Time.deltaTime);
			OnUnityUpdate();
		}
	}

	//---------------------------------------------------------------
	public void DoEffectInitialize()
	{
		if (m_bInitialize) return;
		m_bInitialize = true;
		ResetGameObjectName(gameObject);
		OnEffectInitialize();
	}

	public void DoEffectStart(Vector3 vecOrigin, UnityAction<CEffectBase, bool> delEffectNotify, float fDuration = 0f,  Vector3 vecDest = new Vector3(), params object [] aParams)
	{
		if (m_bActive) DoEffectEnd(true);

		m_pFollowTransform = null;
		m_vecOrigin = vecOrigin;
		transform.position = m_vecOrigin + m_vecOffset;
		m_vecDest = vecDest;

		m_fDuration = fDuration;
		m_fDurationCurrent = 0;
		m_bActive = true;
		m_delEffectNotify = delEffectNotify;

		SetMonoActive(true);
		OnEffectStartWorldPosition(vecOrigin, vecDest, fDuration, aParams);
		OnEffectStart();
	}

	public void DoEffectStart(UnityAction<CEffectBase, bool> delEffectNotify, float fDuration = 0f, params object[] aParams)
	{
		if (m_bActive) DoEffectEnd(true);

		m_bActive = true;
		m_delEffectNotify = delEffectNotify;
		SetMonoActive(true);
		m_fDuration = fDuration;
		OnEffectStartSingle(fDuration, aParams);
		OnEffectStart();
	}

	public void DoEffectStart(Transform pFollow, UnityAction<CEffectBase, bool> delEffectNotify, float fDuration = 0f, Vector3 vecDest = new Vector3(), params object[] aParams)
	{
		if (m_bActive) DoEffectEnd(true);

		m_pFollowTransform = pFollow;		
		transform.position = pFollow.position + m_vecOffset;

		m_vecOrigin = pFollow.position;
		m_vecDest = vecDest;
		m_fDuration = fDuration;
		m_bActive = true;
		m_delEffectNotify = delEffectNotify;

		SetMonoActive(true);
		OnEffectStartTransform(pFollow, vecDest, fDuration, aParams);
		OnEffectStart();
	}

	public void DoEffectStop()
	{
		OnEffectStop();
	}

	public void DoEffectEnd(bool bForce = false)
	{
		m_bActive = false;
		SetMonoActive(false);
		OnEffectEnd(bForce);
		m_delEffectNotify?.Invoke(this, false);
		m_delEffectNotify = null;
	}


	public string GetEffectName()
	{
		return gameObject.name;
	}

	//--------------------------------------------------------------
	private void UpdateEffectDuration(float fDelta)
	{
		if (m_fDuration == 0) return;

		m_fDurationCurrent += fDelta;
		if (m_fDurationCurrent >= m_fDuration)
		{
			m_fDurationCurrent = 0;
			DoEffectEnd();
		}
	}


	//---------------------------------------------------------------
	protected virtual void OnEffectInitialize() { }
	protected virtual void OnEffectStart() { }
	protected virtual void OnEffectStartWorldPosition(Vector3 vecOrigin, Vector3 vecDest, float fDuration, params object[] aParams) { }
	protected virtual void OnEffectStartTransform(Transform pFollow, Vector3 vecDest, float fDuration, params object[] aParams) { }
	protected virtual void OnEffectStartSingle(float fDuration, params object[] aParams) { }
	protected virtual void OnEffectEnd(bool bForce) { }
	protected virtual void OnEffectStop() { }
	protected virtual void OnUnityUpdate() { }
}
