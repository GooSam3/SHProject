using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NSkill;

public abstract class SHSkillTaskEventConditionBase : CSkillTaskEventConditionBase
{
	
	//--------------------------------------------------------------------
}

public class SHSkillTaskEventConditionEnter : SHSkillTaskEventConditionBase
{

}

public class SHSkillTaskEventConditionExit : SHSkillTaskEventConditionBase
{

}

public class SHSkillTaskEventConditionAnimation : SHSkillTaskEventConditionBase
{
	public EAnimEventType eAnimEventType = EAnimEventType.None;

	protected override bool OnTaskEventCondition(CStateSkillBase pOwnerState, CSkillUsage pUsage, ISkillProcessor pSkillProcessor, params object[] aArg)
	{
		bool bCheck = false;
		if (aArg.Length > 0)
		{
			EAnimEventType eAnimEventTypeArg = ExtensionFunction.ToEnum<EAnimEventType>((string)aArg[0]);

			if (eAnimEventTypeArg == eAnimEventType)
			{
				bCheck = true;
			}
		}

		return bCheck; 
	}

}

public class SHSkillTaskEventConditionAnimationEnd : SHSkillTaskEventConditionBase
{

}

public class SHSkillTaskEventConditionAutoCast : SHSkillTaskEventConditionBase
{

}

public class SHSkillTaskEventConditionCustom : SHSkillTaskEventConditionBase
{
	public ETaskEventCustomType eTaskEventType = ETaskEventCustomType.None;

	protected override bool OnTaskEventCondition(CStateSkillBase pOwnerState, CSkillUsage pUsage, ISkillProcessor pSkillProcessor, params object[] aArg)
	{
		bool bCheck = false;
		if (aArg.Length > 0)
		{
			ETaskEventCustomType eAnimEventTypeArg = (ETaskEventCustomType)aArg[0];

			if (eTaskEventType == eAnimEventTypeArg)
			{
				bCheck = true;
			}
		}

		return bCheck;
	}
}
