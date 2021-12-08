using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SHEffectCameraShake : CEffectTransformBase
{
	private float m_fStrength = 0;
	private float m_fAngleRange = 0;
	private Transform m_pTransform = null;
	private bool m_bForceEnd = false;
	//-----------------------------------------------------------------
	protected override void OnEffectStartTransform(Transform pFollow, Vector3 vecDest, float fDuration, params object[] aParams)
	{
		m_pTransform = pFollow;
		if (aParams.Length < 2)
		{
			return;
		}
		m_fStrength = (float)aParams[0];
		m_fAngleRange = (float)aParams[1];

		PrivTaskEffectCameraShake();
		m_bForceEnd = false;
		base.OnEffectStartTransform(pFollow, vecDest, fDuration, aParams);
	}

	protected override void OnEffectEnd(bool bForce)
	{
		base.OnEffectEnd(bForce);
		m_bForceEnd = bForce;
	}

	//-------------------------------------------------------------------
	private void PrivTaskEffectCameraShake()
	{
		float fAngle = (float)Random.Range(-m_fAngleRange, m_fAngleRange);
		Vector3 vecDirection = Vector3.right;
		vecDirection = Quaternion.AngleAxis(fAngle, Vector3.forward) * vecDirection;
		vecDirection *= m_fStrength;
		ProtEffectTransformRefreshInstance(0, m_pTransform, vecDirection);
	}
	//-------------------------------------------------------------------
	public void HandleEffectCharShakeEnd()
	{
		if (m_bForceEnd == false)
		{
			ProtEffectTransformReset();
		}
	}
}