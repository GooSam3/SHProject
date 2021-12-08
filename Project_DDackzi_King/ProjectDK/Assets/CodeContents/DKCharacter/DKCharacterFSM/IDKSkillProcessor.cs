using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDKSkillProcessor : ISkillProcessor
{
	public DKUnitBase IGetDKUnit();

}

public class DKSkillUsage : CSkillUsage
{
	//--------------------------------------
	public IDKSkillProcessor GetSkillTarget()
	{
		return UsageTarget as IDKSkillProcessor;
	}
}