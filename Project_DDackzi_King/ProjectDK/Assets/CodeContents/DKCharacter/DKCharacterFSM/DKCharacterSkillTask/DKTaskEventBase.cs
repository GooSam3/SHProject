using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NSkill;

public abstract class DKTaskEventBase : CTaskEventConditionBase
{

}

public class DKTaskEventEnter : DKTaskEventBase
{
	

}

public class DKTaskEventExit : DKTaskEventBase
{


}

public class DKTaskEventAnimation : DKTaskEventBase
{
	private EAnimEventType m_eAnimEventType = EAnimEventType.None;
	private int m_iIndex = 0;


	public void SetTaskEventAnimation(EAnimEventType eAnimEventType, uint iIndex)
	{
		m_eAnimEventType = eAnimEventType;
		m_iIndex = (int)iIndex;
	}

	protected override bool OnTaskEventCondition(CStateSkillBase pOwnerState, CSkillUsage pUsage, ISkillProcessor pSkillProcessor, params object[] aArg) 
	{
		bool bCondition = false;
		if (aArg.Length >= 2)
		{
			EAnimEventType eAnimEventType = (EAnimEventType)aArg[0];
			int iIndex = (int)aArg[1];
			if (m_eAnimEventType == eAnimEventType && m_iIndex == iIndex)
			{
				bCondition = true;
			}
		}

		return bCondition; 
	}

}

public class DKTaskEventAutoCast : DKTaskEventBase
{
	private bool m_bAutoCastOn = false;
	
	public void SetTaskEventAutoCast(bool bOn) { m_bAutoCastOn = bOn; }
}
