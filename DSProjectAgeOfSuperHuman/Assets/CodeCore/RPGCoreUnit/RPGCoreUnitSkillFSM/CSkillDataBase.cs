using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class CSkillDataBase
{
	public uint hSkillID = 0;
	public string SkillName;

	public CSkillPropertyListBase PropertyList = null;	
}

public class CSkillDataActive : CSkillDataBase
{	
	public List<CSkillConditionBase> listCondition = new List<CSkillConditionBase>();
	public List<CStateSkillBase> listState = new List<CStateSkillBase>();
}

public class CSkillDataPassive : CSkillDataBase
{
	public List<CSkillTaskBase> listSkillTask = new List<CSkillTaskBase>(); // 스킬이 시작될때 적용될 내용
}

public class CSkillDataAutoCast : CSkillDataBase
{
	public bool SwitchOn = false;
	public List<CSkillTaskBase> listSkillTask = new List<CSkillTaskBase>(); // 스킬이 시작될때 적용될 내용
}

//---------------------------------------------------------------
public abstract class CSkillPropertyListBase
{
	private int m_iSkillLevel = 0;

	public void SetSkillPropertyLevel(int iLevel) { m_iSkillLevel = iLevel; }

	public float GetSkillPropertyValue(string strPropertyName)
	{
		return OnSkillPropertyValue(strPropertyName, m_iSkillLevel);
	}

	//------------------------------------------------------------------
	protected virtual float OnSkillPropertyValue(string strPropertyName, int iLevel) { return 0; }
}