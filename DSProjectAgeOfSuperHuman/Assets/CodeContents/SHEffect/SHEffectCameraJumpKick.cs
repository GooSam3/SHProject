using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class SHEffectCameraJumpKick : CEffectTransformTweenBase
{	
	private Vector3 m_vecCameraOrigin = Vector3.zero;
	private Vector3 m_vecHeroOrigin = Vector3.zero;
	private Transform m_pCamera = null;
	private Transform m_pHero = null;
	private UnityAction m_delFinish = null;
	//---------------------------------------------------------------
	public void DoEffectCameraJumpKick(Transform pCamera, Transform pHero, UnityAction defFinish)
	{
		m_pCamera = pCamera;
		m_pHero = pHero;
		m_vecCameraOrigin = pCamera.position;
		m_vecHeroOrigin = pHero.position;
		m_delFinish = defFinish;

		STransformTween pTransTweenCamera = FindTransformTween(0);
		if (pTransTweenCamera != null)
		{
			PrivEffectJumpKickStart(pCamera, pTransTweenCamera);
		}

		DoEffectStart(null);
	
	}
	//-----------------------------------------------------------------
	private void PrivEffectJumpKickStart(Transform pTarget, STransformTween pTransformTween)
	{
		if (pTransformTween.Tween.firstTween == null) return;

		pTransformTween.Tween.firstTween.Init(pTarget);
		pTransformTween.Tween.Rewind();
		Vector3 vecOrigin = pTarget.position;
		pTransformTween.Tween.firstTween.fromVector = pTransformTween.OriginVectorFrom + vecOrigin;
		pTransformTween.Tween.firstTween.toVector = pTransformTween.OriginVectorTo + vecOrigin;
		pTransformTween.Tween.Play();
	}

	//-----------------------------------------------------------------
	public void HandleCameraUpperCutEnd()
	{
		m_pCamera.position = m_vecCameraOrigin;
		m_pHero.position = m_vecHeroOrigin;
		ProtTransformTweenStopAll();
		DoEffectEnd();
		m_delFinish?.Invoke();
	}
}
