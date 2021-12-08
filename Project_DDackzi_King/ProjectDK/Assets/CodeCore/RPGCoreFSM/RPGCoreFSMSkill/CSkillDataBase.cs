using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class CSkillDataBase
{
	public uint hSkillID = 0;
}

public class CSkillDataActive : CSkillDataBase
{	
	public List<CSkillConditionBase> listCondition = new List<CSkillConditionBase>();
	public List<CStateSkillBase> listState = new List<CStateSkillBase>();
}

public class CSkillDataPassive : CSkillDataBase
{
	public List<CSkillTaskBase> listSkillTask = new List<CSkillTaskBase>(); // ��ų�� ���۵ɶ� ����� ����
}

public class CSkillDataAutoCast : CSkillDataBase
{
	public bool SwitchOn = false;
	public List<CSkillTaskBase> listSkillTask = new List<CSkillTaskBase>(); // ��ų�� ���۵ɶ� ����� ����
}