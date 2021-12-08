using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NSkill;
//���ϸ��̼� ���¿� ���� ������Ʈ�� ����

public class DKSkillStateAnimation : DKSkillStateBase
{
	public DKSkillTaskAnimation StartAnimation = null;

	//-------------------------------------------------------
	protected override void OnStateEnter(CStateBase pStatePrev)
	{
		base.OnStateEnter(pStatePrev);
		if (StartAnimation != null)
		{
			if (m_pSkillUsage.UsageTarget.IGetUnit().IsAlive)
			{
				if (m_pSkillUsage.UsageTarget != null)
				{
					m_pSkillProcessor.IGetDKUnit().SetDKUnitDirection(m_pSkillUsage.UsageTarget.IGetUnit().GetUnitPosition());
				}
				StartAnimation.DoSkillTaskUse(this, m_pSkillUsage, m_pSkillProcessor);
			}
			else
			{
				ProtStateSelfEnd();
			}
		}
	}

	protected override void OnStateTaskEvent(int iEventType, params object[] aArg)
	{
		base.OnStateTaskEvent(iEventType, aArg);

		if (iEventType == (int)ETaskEventType.TaskEvent_AnimationEnd)
		{
			ProtStateSelfEnd();
		}
	}
}
