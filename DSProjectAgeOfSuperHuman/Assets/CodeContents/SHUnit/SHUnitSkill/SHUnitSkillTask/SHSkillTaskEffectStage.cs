using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NSkill;

public class SHSkillTaskEffectStage : SHSkillTaskBase
{
    private EStageEffectType m_eStageEffect = EStageEffectType.None;
    private int m_iArg = 0;
    private float m_fArg = 0;
    private float m_fDuration = 0;
    //-----------------------------------------------------------------
    public void SetTaskEffectStage(EStageEffectType eStageEffect, int iArg, float fArg, float fDuration)
	{
        m_eStageEffect = eStageEffect;
        m_iArg = iArg;
        m_fArg = fArg;
        m_fDuration = fDuration;
	}

	//------------------------------------------------------------------
	protected override void OnSkillTaskUse(CStateSkillBase pOwnerState, CSkillUsage pSkillUsage, ISkillProcessor pSkillOwner, List<CUnitBase> pListTarget)
	{
		if (pSkillUsage.UsageTarget.IGetUnit().IsAlive == false) return;
		if (pSkillOwner.IGetUnit().IsAlive == false) return;

		SHStageCombatSceneNormal pCombatScene = SHManagerStage.Instance.GetMgrStageCurrent() as SHStageCombatSceneNormal;

		if (pCombatScene == null) return;

		if (m_eStageEffect == EStageEffectType.MoveForward)
		{

		}
        else if (m_eStageEffect == EStageEffectType.CameraKnockBack)
		{
			pCombatScene.DoStageEffectKnockBack();
		}
        else if (m_eStageEffect == EStageEffectType.CameraJumpKick)
		{
			pCombatScene.DoStageEffectCameraJumpKick(0);
		}
		else if (m_eStageEffect == EStageEffectType.CameraShake)
		{
			pCombatScene.DoStageEffectCameraShake(m_fArg, (int)m_iArg, m_fDuration);
		}
		else if (m_eStageEffect == EStageEffectType.RageComboType1)
		{
			pCombatScene.DoStageEffectRageCombo(true);
		}
	}
}

public class SHSkillTaskEffectStageEnd : SHSkillTaskBase
{
	private EStageEffectType m_eStageEffect = EStageEffectType.None;
	//------------------------------------------------------
	public void SetTaskEffectStageEnd(EStageEffectType eStageEffect)
	{
		m_eStageEffect = eStageEffect;
	}

	protected override void OnSkillTaskUse(CStateSkillBase pOwnerState, CSkillUsage pSkillUsage, ISkillProcessor pSkillOwner, List<CUnitBase> pListTarget)
	{
		SHStageCombatSceneNormal pCombatScene = SHManagerStage.Instance.GetMgrStageCurrent() as SHStageCombatSceneNormal;
		if (pCombatScene == null) return;

		if (m_eStageEffect == EStageEffectType.RageComboType1)
		{
			pCombatScene.DoStageEffectRageCombo(false);
		}
	}
}