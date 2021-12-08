using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NSkill;

public class SHSkillStateAnimation : SHSkillStateBase
{
	private SHSkillTaskAnimation m_pStateAnimation = null;

	//----------------------------------------------------------------------------
	public void SetStateAnimation(SHSkillTaskAnimation pStateAnim)
	{
		m_pStateAnimation = pStateAnim;
	}

	protected override void OnStateEnter(CStateBase pStatePrev)
	{
		base.OnStateEnter(pStatePrev);
		if (m_pStateAnimation != null)
		{
			m_pStateAnimation.DoSkillTaskUse(this, m_pSkillUsage, m_pSkillProcessor);
		}
		else
		{
			ProtStateSelfEnd();
		}
	}

	protected override void OnStateTaskEvent(int iEventType, params object[] aArg)
	{
		base.OnStateTaskEvent(iEventType, aArg);

		if (iEventType == (int)NSkill.ETaskEventType.Animation_End)
		{
			ProtStateSelfEnd();
		}
	}

	protected override void OnStateLeaveForce(CStateBase pStatePrev)
	{
		base.OnStateLeaveForce(pStatePrev);
		m_pSkillProcessor.ISkillAnimationReset();
	}

	//---------------------------------------------------------------------------
	public void HandleStateAnimationEnd(string strAnimName, bool bFinish)
	{
		if (bFinish == false)
		{
			Debug.LogError("[StateAnimation] Invalid AniName : " + strAnimName);
		}
		else
		{
			DoStateTaskEvent((int)ETaskEventType.Animation_End, strAnimName);
		}
		ProtStateSelfEnd();
	}

	public void HandleStateAnimationEvent(string strEventName, int iArg, float fArg)
	{
		DoStateTaskEvent((int)ETaskEventType.Animation_Event, strEventName, iArg, fArg);
	}
}
