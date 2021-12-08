using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NSkill;

public class SHSkillDataActive : CSkillDataActive
{
	public ETargetingType TargetingType = ETargetingType.None;
	

	public SHSkillDataActive()
	{
		PropertyList = new SHSkillPropertyList();
	}

}

public class SHSkillDataPassive : CSkillDataPassive
{


}

public class SHSkillDataAutoCast : CSkillDataAutoCast
{

}

//--------------------------------------------------------------

public class SHSkillPropertyList : CSkillPropertyListBase
{
	protected override float OnSkillPropertyValue(string strPropertyName, int iLevel)
	{
		float fValue = 1;


		return fValue;
	}

}