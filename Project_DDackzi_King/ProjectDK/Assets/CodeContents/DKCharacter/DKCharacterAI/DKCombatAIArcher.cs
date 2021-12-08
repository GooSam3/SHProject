using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DKCombatAIArcher : DKCombatAIHeroBase
{
	//-----------------------------------------------------------

	protected override DKUnitBase OnCombatAIFindTarget(EActiveSkillType eSkillType)
	{
		return ProtCombatAIHeroTarget(EDKStatType.AttackPower, false);
	}

	public DKCombatAIArcher() { m_eClassType = EClassType.Archer; }
}
