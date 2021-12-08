using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NSkill;

public class SHSkillTaskAnimation : SHSkillTaskBase
{
	private EAnimationType m_eAnimType = EAnimationType.None;  public EAnimationType GetAnimType() { return m_eAnimType; }
	private bool m_bLoop = false;
	private float m_fDuration = 0f;
	private float m_fAnimSpeed = 0f;
	private string m_strSoundName = "None";

    public void SetSkillTaskAnimation(EAnimationType eAnimType, bool bLoop, float fDuration, float fAnimSpeed, string strSoundName)
	{
		m_eAnimType = eAnimType;
		m_bLoop = bLoop;
		m_fDuration = fDuration;
		m_fAnimSpeed = fAnimSpeed;
		m_strSoundName = strSoundName;
	}

	protected override void OnSkillTaskUse(CStateSkillBase pOwnerState, CSkillUsage pSkillUsage, ISkillProcessor pSkillOwner, List<CUnitBase> pListTarget)
	{
		base.OnSkillTaskUse(pOwnerState, pSkillUsage, pSkillOwner, pListTarget);

		CAnimationBase.SAnimationUsage rUsage = new CAnimationBase.SAnimationUsage();
		rUsage.AnimName = m_eAnimType.ToString();
		rUsage.bLoop = m_bLoop;
		rUsage.fDuration = m_fDuration;
		rUsage.fAniSpeed = m_fAnimSpeed;

		SHSkillStateAnimation pStateAnimation = pOwnerState as SHSkillStateAnimation;
		pSkillOwner.ISkillAnimation(ref rUsage, pStateAnimation.HandleStateAnimationEnd, pStateAnimation.HandleStateAnimationEvent);
	}
}
