using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class SHStageCombatSceneNormal : SHStageBase
{
	[SerializeField]
	private SpriteRenderer StageBackGround = null;
	[SerializeField]
	private SHStageMovingControl StageMovingControl = null;
	[SerializeField]
	private SHEffectCameraJumpKick JumpKick = null;
	[SerializeField]
	private SHEffectParticleNormal DashScreen = null;
	[SerializeField]
	private SHEffectCameraKnockBack KnockBack = null;
	[SerializeField]
	private SHEffectCameraShake CameraShake = null;
	[SerializeField]
	private SHEffectParticleNormal RageCombo = null;
	[SerializeField]
	private SHEffectParticleNormal EnemyDisappear = null;
	[SerializeField]
	private SHEffectParticleNormal KnockBackSmoke = null;

	private bool m_bMoveForward = false; public bool IsStageMoveForward() { return m_bMoveForward; }
	private Vector3 m_vecCameraOrigin = Vector3.zero;
	//---------------------------------------------------------------------------
	protected override void OnStageStart()
	{
		base.OnStageStart();
		CCameraControllerBase pCamera = SHManagerCamera.Instance.GetCameraMain();
		m_vecCameraOrigin = pCamera.transform.position;
		SHManagerStageSpawner.Instance.DoMgrStageSpawnerStart();
		
		List<SHUnitHero> pListHero = SHManagerUnit.Instance.GetUnitHeroDeck();
		UIManager.Instance.DoUIMgrShow<SHUIFrameCombatHero>().DoUIFrameCombatStageHeroList(pListHero);
		SHManagerUnit.Instance.DoMgrUnitHeroTagOnFirst();
	}

	//----------------------------------------------------------------------------
	public void DoStageMoveForward(Sprite pSprite, UnityAction delFinish)
	{
		if (m_bMoveForward) return;
		m_bMoveForward = true;
		StageMovingControl.DoStageMovingStart(StageBackGround.sprite, ()=> {
			m_bMoveForward = false;
			delFinish?.Invoke();
		});
		StageBackGround.sprite = pSprite;

		UIManager.Instance.DoUIMgrFind<SHUIFrameNumberTag>().DoNumberTagClear();
	}

	public void DoStageEffectCameraJumpKick(int iType)
	{
		if (CameraShake.IsActive)
		{
			CameraShake.DoEffectEnd(true);
		}

		CCameraControllerBase pCamera = SHManagerCamera.Instance.GetCameraMain();
		SHUnitHero pHero = SHManagerUnit.Instance.GetUnitHero();
		SHUnitEnemy pEnemy = SHManagerUnit.Instance.GetUnitEnemy();
		if (pCamera != null && pHero != null && pEnemy != null)
		{
			if (pEnemy.IsAlive)
			{
				pCamera.transform.position = m_vecCameraOrigin;
				JumpKick.DoEffectCameraJumpKick(pCamera.transform, pHero.transform, ()=> {
					
				});
			}
		}
	}

	public void DoStageEffectCameraShake(float fStrength, float fAngleRange, float fDuration)
	{
		if (JumpKick.IsActive) return;
		
		CCameraControllerBase pCamera = SHManagerCamera.Instance.GetCameraMain();
		if (pCamera != null)
		{
			
			CameraShake.DoEffectStart(pCamera.transform, (CEffectBase pEffect, bool pStartEnd) => {

			}, fDuration, Vector3.zero, fStrength, fAngleRange);
		}
	}

	public void DoStageEffectDashScreen(float fDuration)
	{
		SHManagerUnit.Instance.DoMgrUnitEnemyHideTagOffUnit(false);
		DashScreen.DoEffectStart(Vector3.zero, (CEffectBase pEffect, bool bStart) => { 
			if (bStart == false)
			{
				SHManagerUnit.Instance.DoMgrUnitEnemyHideTagOffUnit(true);
			}
		}, fDuration); 
	}

	public void DoStageEffectKnockBack()
	{
		SHUnitEnemy pEnemy = SHManagerUnit.Instance.GetUnitEnemy();
		if (pEnemy != null)
		{
			if (pEnemy.IsAlive)
			{
				CEffectBase pShake = pEnemy.GetUnitShakeEffect();
				if (pShake is SHEffectNone)
				{
					pEnemy.SetUnitShakeEffect(null);
				}
				else
				{
					SHEffectCharacterShake pShakeChar = pShake as SHEffectCharacterShake;
					if (pShakeChar)
					{
						pShakeChar.DoEffectEnd();
					}
				}

				KnockBack.DoCameraKnockBackStart(pEnemy.transform);
				KnockBackSmoke.DoEffectStart(null);
			}
		}
	}

	public void DoStageEffectRageCombo(bool bOnOff)
	{
		if (bOnOff)
		{
			RageCombo.DoEffectStart(null);
		}
		else
		{
			RageCombo.DoEffectEnd();
		}
	}

	public void DoStageEffectEnemyDisappear()
	{
		EnemyDisappear.DoEffectStart(null);
	}
	//------------------------------------------------------------------------------

}
