using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NSkill;

public class DKSkillTaskAnimation : DKSkillTaskBase
{
    private EAnimationType m_eAnimationType = EAnimationType.None;
    private bool m_bLoop = false;
    private float m_fDuration = 0;
    private float m_fAniSpeed = 0;

    //----------------------------------------------------------------
    public void SetSkillTaskAnimation(EAnimationType eAnimType, bool bLoop, float fDuration, float fAniSpeed)
	{
        m_eAnimationType = eAnimType;
        m_bLoop = bLoop;
        m_fDuration = fDuration;
        m_fAniSpeed = fAniSpeed;
	}

	//------------------------------------------------------------------
	protected override void OnSkillTaskUse(CSkillUsage pSkillUsage, ISkillProcessor pSkillOwner, List<CUnitBase> pListTarget)
	{
		base.OnSkillTaskUse(pSkillUsage, pSkillOwner, pListTarget);
        pSkillOwner.ISkillAnimation(m_eAnimationType.ToString(), m_bLoop, m_fDuration, m_fAniSpeed);
	}
}

public class DKSkillTaskEffect : DKSkillTaskBase
{
    private EUnitSocket m_eSocket = EUnitSocket.None;
    private string m_strPrefabName;
    private float m_fDuration = 0;
    //-----------------------------------------------------------------
    public void SetSkillTaskEffect(EUnitSocket eSocket, string strPrefabName, float fDuration)
	{
        m_eSocket = eSocket;
        m_strPrefabName = strPrefabName;
        m_fDuration = fDuration;
	}
}
