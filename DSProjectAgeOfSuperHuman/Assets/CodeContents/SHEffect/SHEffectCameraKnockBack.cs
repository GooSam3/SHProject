using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SHEffectCameraKnockBack : CEffectTransformTweenBase
{
    [SerializeField]
    private SpriteRenderer      BackGround;

    private Transform m_pTargetObject = null;
    private Vector3 m_vecTargetOriginScale = Vector3.zero;
	private Vector3 m_vecTargetOriginPosition = Vector3.zero;
	private Vector3 m_vecRendererOriginScale = Vector3.zero;
	private Vector3 m_vecRendererOriginPosition = Vector3.zero;
	//-----------------------------------------------------------------
	protected override void OnEffectInitialize()
	{
		base.OnEffectInitialize();
		if (BackGround)
		{
			m_vecRendererOriginScale = BackGround.transform.localScale;
			m_vecRendererOriginPosition = BackGround.transform.position;
		}
	}

	//-----------------------------------------------------------------
	public void DoCameraKnockBackStart(Transform pTargetObject)
	{
		SetMonoActive(true);
		m_pTargetObject = pTargetObject;
		m_vecTargetOriginScale = pTargetObject.localScale;
		m_vecTargetOriginPosition = pTargetObject.position;
		
		STransformTween pTransTween = FindTransformTween(1);
		if (pTransTween != null)
		{
			PrivCameraKnockBackTweenStart(m_pTargetObject, pTransTween, m_vecTargetOriginScale);
		}

		pTransTween = FindTransformTween(2);
		if (pTransTween != null)
		{
			PrivCameraKnockBackTweenStart(m_pTargetObject, pTransTween, m_vecTargetOriginPosition);
		}
	}
	//------------------------------------------------------------------
	private void PrivCameraKnockBackTweenStart(Transform pTarget, STransformTween pTransTween, Vector3 vecOrigin)
	{
		pTransTween.Tween.firstTween.Init(pTarget);
		pTransTween.Tween.Rewind();

		pTransTween.Tween.firstTween.fromVector = pTransTween.OriginVectorFrom + vecOrigin;
		pTransTween.Tween.firstTween.toVector = pTransTween.OriginVectorTo + vecOrigin;
		pTransTween.Tween.Play();
	}
    
    //------------------------------------------------------------------
    public void HandleCameraKnockBackPushFinish()
	{
		STransformTween pTransTween = FindTransformTween(0);
		if (pTransTween != null)
		{
			PrivCameraKnockBackTweenStart(BackGround.transform, pTransTween, m_vecRendererOriginScale);
		}

		pTransTween = FindTransformTween(5);
		if (pTransTween != null)
		{
			PrivCameraKnockBackTweenStart(BackGround.transform, pTransTween, m_vecRendererOriginPosition);
		}

		pTransTween = FindTransformTween(3);
		if (pTransTween != null)
		{
			PrivCameraKnockBackTweenStart(m_pTargetObject, pTransTween, m_pTargetObject.localScale);
		}

		pTransTween = FindTransformTween(4);
		if (pTransTween != null)
		{
			pTransTween.Tween.firstTween.Init(m_pTargetObject);
			pTransTween.Tween.Rewind();

			pTransTween.Tween.firstTween.fromVector = m_pTargetObject.position;
			pTransTween.Tween.firstTween.toVector = m_vecTargetOriginPosition;
			pTransTween.Tween.Play();
		}


		SHStageCombatSceneNormal pCombatScene = SHManagerStage.Instance.GetMgrStageCurrent() as SHStageCombatSceneNormal;
		if (pCombatScene != null)
		{
			pCombatScene.DoStageEffectDashScreen(pTransTween.Tween.animationTime);
		}
	}

	public void HandleCameraKnockBackPullFinsish()
	{
		ProtTransformTweenStopAll();
		BackGround.transform.localScale = m_vecRendererOriginScale;
		BackGround.transform.position = m_vecRendererOriginPosition;
		m_pTargetObject.localScale = m_vecTargetOriginScale;
		m_pTargetObject.position = m_vecTargetOriginPosition;
	}

}
