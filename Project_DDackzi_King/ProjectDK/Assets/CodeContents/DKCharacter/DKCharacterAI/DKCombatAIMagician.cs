using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DKCombatAIMagician : DKCombatAIHeroBase
{
	//----------------------------------------------------
	protected override DKUnitBase OnCombatAIFindTarget(EActiveSkillType eSkillType)
	{
		return ProtCombatAIHeroTarget(EDKStatType.DefenceRate, false);
	}

	public DKCombatAIMagician() { m_eClassType = EClassType.Magician; }
}
