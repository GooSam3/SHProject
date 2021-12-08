using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DKCombatAIWarrior : DKCombatAIHeroBase
{
	// Àü»ç´Â 
	protected override DKUnitBase OnCombatAIFindTarget(EActiveSkillType eSkillType)
	{
		return ProtCombatAIHeroTarget(EDKStatType.AttackPower, false);
	}
	//----------------------------------------------------
	public DKCombatAIWarrior() { m_eClassType = EClassType.Warrior; } 
}
