using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// �׽�ũ ���� ���� : �ڽ��� �پ��� ������ üũ 
public abstract class CSkillTaskFactorConditionBase 
{   
    public bool DoTaskConditionCheck(ISkillProcessor pSkillProcessor)
	{
		return OnTaskConditionCheck();
	}
	//--------------------------------------------------------------------
	protected virtual bool OnTaskConditionCheck() { return true;}
}

