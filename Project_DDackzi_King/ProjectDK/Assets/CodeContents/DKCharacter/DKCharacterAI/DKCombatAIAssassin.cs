using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DKCombatAIAssassin : DKCombatAIHeroBase
{
	//-----------------------------------------------------------

	protected override DKUnitBase OnCombatAIFindTarget(EActiveSkillType eSkillType)
	{
		return ProtCombatAIHeroTarget(EDKStatType.MaxHealthPoint, false);
	}

	public DKCombatAIAssassin() { m_eClassType = EClassType.Assassin; }
}
