using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SHUnitEnemy : SHUnitBase
{
	private SHUnitTagTween m_pTagController;

	private CEffectBase m_pCharShake = null; public CEffectBase GetUnitShakeEffect() { return m_pCharShake; } public void SetUnitShakeEffect(CEffectBase pShakeEffect) { m_pCharShake = pShakeEffect; }
	//-----------------------------------------------------------------------------
	protected override void OnUnityAwake()
	{
		base.OnUnityAwake();
		m_pTagController = GetComponentInChildren<SHUnitTagTween>();
	}

	protected override void OnUnitAlive()
	{
		base.OnUnitAlive();
		DoUnitAIEnable(true);
	}

	protected override void OnUnitDeathStart()
	{
		base.OnUnitDeathStart();
		CAnimationBase.SAnimationUsage rAnimation = new CAnimationBase.SAnimationUsage();
		rAnimation.AnimName = "death";
		rAnimation.bLoop = false;
		rAnimation.fAniSpeed = 1f;
		rAnimation.fDuration = 0;
	
		ProtUnitAnimation(ref rAnimation, (AniName, bFinish) =>
		{
			ProtUnitDeathEnd();
		}, null);
		m_pSHAnimationSpine.DoAnimationSpineChangeMaterial(SHAnimationSpineBase.ESpineMaterialType.GayScale);

		if (m_pCharShake)
		{
			m_pCharShake.DoEffectEnd();
			m_pCharShake = null;
		}

		SHStageCombatSceneNormal pSceneNormal = SHManagerStage.Instance.GetMgrStageCurrent();
		pSceneNormal.DoStageEffectEnemyDisappear();

		SHManagerStage.Instance.DoMgrStageEnemyReward(GetUnitID());
	}

	protected override void OnUnitExit(UnityAction delFinish)
	{
		base.OnUnitExit(delFinish);
		
		m_pSHAnimationSpine.DoAnimationSpineChangeMaterial(SHAnimationSpineBase.ESpineMaterialType.GayScale);
		if (m_pCharShake)
		{
			m_pCharShake.DoEffectEnd();
			m_pCharShake = null;
		}
	}

	protected override void OnUnitDeathEnd()
	{
		base.OnUnitDeathEnd();
		ProtUnitRemove(false);
	}

	protected override void OnUnitReset()
	{
		base.OnUnitReset();
		ProtUnitStandBy();
		m_pSHStatComponent.DoStatReset();
	}

	protected override void OnUnitPhaseOut(UnityAction delFinish)
	{
		base.OnUnitPhaseOut(delFinish);
		ProtUnitAnimationIdle();
		m_pSHSkillFSM.DoUnitSkillReset();
		m_pSHAnimationSpine.DoAnimationSpineChangeMaterial(SHAnimationSpineBase.ESpineMaterialType.GayScale);
		m_pSHAnimationSpine.DoAnimationSpineOrderInLayer(-10);
		DoUnitAIEnable(true);
	}

	protected override void OnSHUnitApplyDamage(SDamageResult pDamageResult, EUnitSocket eUnitSocket, string strHitEffectName)
	{
		Vector3 vecWorldPosition = GetUnitSocketTransform(eUnitSocket).position;

		if (pDamageResult.eDamageResult == EDamageResult.Dodge)
		{
			UIManager.Instance.DoUIMgrFind<SHUIFrameNumberTag>().DoNumberTagMiss(vecWorldPosition);
		}
		else if (pDamageResult.eDamageResult == EDamageResult.Block)
		{
			UIManager.Instance.DoUIMgrFind<SHUIFrameNumberTag>().DoNumberTagBlock(vecWorldPosition);
		}
		else if (pDamageResult.eDamageType == NSkill.EDamageType.AttackSkill || pDamageResult.eDamageType == NSkill.EDamageType.AttackNormal)
		{
			UIManager.Instance.DoUIMgrFind<SHUIFrameNumberTag>().DoNumberTagDamage(pDamageResult.fTotalValue, pDamageResult.eDamageResult == EDamageResult.Critical ? true : false, vecWorldPosition);
		}

		if (strHitEffectName != string.Empty && strHitEffectName != null)
		{ 
			SHManagerEffect.Instance.DoMgrEffectRigist(strHitEffectName, (SHEffectParticleNormal pEffect) =>
			{
				pEffect.DoEffectStart(vecWorldPosition, null, 0);
			});
		}
	}

	protected override void OnSHUnitApplyHeal(SDamageResult pDamageResult, EUnitSocket eUnitSocket, string strHitEffectName)
	{
		Vector3 vecWorldPosition = GetUnitSocketTransform(eUnitSocket).position;
		UIManager.Instance.DoUIMgrFind<SHUIFrameNumberTag>().DoNumberTagHeal(pDamageResult.fTotalValue, pDamageResult.eDamageResult == EDamageResult.Critical ? true : false, vecWorldPosition);
	}
	//------------------------------------------------------
	public void DoUnitEnemySpawn(UnityAction delFinish)
	{
		ProtUnitSpawned(delFinish);
	}

	public void DoUnitEnemyPhaseOut(UnityAction delFinish)
	{
		ProtUnitPhaseOut(delFinish);
	}

	public void DoUnitEnemyRemoveForce()
	{
		ProtUnitRemove(true);
	}

	public void DoUnitEnemyTagOn(UnityAction delFinish) // 유닛이 스테이지에 등장할때
	{
		m_eUnitTagPosition = EUnitTagPosition.Center;
		m_pSHAnimationSpine.DoAnimationSpineChangeMaterial(SHAnimationSpineBase.ESpineMaterialType.None);
		m_pSHAnimationSpine.DoAnimationSpineOrderInLayer(0);
		ProtUnitAnimationIdle();
		m_pSHSkillFSM.DoUnitSkillReset();
		PrivUnitEnemyTagTween(EUnitTagPosition.Center, false, () => {
			ProtUnitAlive();
			delFinish?.Invoke();
		});
	}

	public void DoUnitEnemyTagOff(EUnitTagPosition ePositionType, bool bForce, UnityAction delFinish)  // 유닛이 스테이지에서 퇴장할때 
	{
		
		if (bForce == false)
		{
			ProtUnitPhaseOut(null);
		}
		m_eUnitTagPosition = ePositionType;
		PrivUnitEnemyTagTween(ePositionType, bForce, delFinish);
	}
	//-------------------------------------------------------
	public EUnitTagPosition GetUnitEnemyTagPosition()
	{
		return m_pTagController.GetUnitTagTweenCurrentPosition();
	}

	//--------------------------------------------------------
	private void PrivUnitEnemyTagTween(EUnitTagPosition eUnitTagType, bool bForce, UnityAction delFinish)
	{
		m_pTagController.DoUnitTagTweenStart(this.transform, eUnitTagType, bForce, delFinish);
	} 


	//----------------------------------------------------
	public SHUnitEnemy()
	{
        m_eUnitControlType = EUnitControlType.EnemyAI;
        m_eUnitRelation = EUnitRelationType.Enemy;
	}
}
