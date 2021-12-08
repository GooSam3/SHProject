using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// 테스크 실행 조건 : 자신의 다양한 조건을 체크 
public abstract class CSkillTaskFactorConditionBase 
{   
    public bool DoTaskConditionCheck(ISkillProcessor pSkillProcessor)
	{
		return OnTaskConditionCheck();
	}
	//--------------------------------------------------------------------
	protected virtual bool OnTaskConditionCheck() { return true;}
}

