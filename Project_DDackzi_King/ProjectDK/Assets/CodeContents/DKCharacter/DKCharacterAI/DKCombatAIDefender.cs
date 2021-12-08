using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DKCombatAIDefender : DKCombatAIHeroBase
{
	// 디펜더는 가장 공격력이 높은 상대를 선택하는 경향이 강하다. 
	protected override DKUnitBase OnCombatAIFindTarget(EActiveSkillType eSkillType)
	{
		return ProtCombatAIHeroTarget(EDKStatType.AttackPower, true);
	}

	public DKCombatAIDefender() { m_eClassType = EClassType.Defender; }
}
