using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DKCombatAIHealer : DKCombatAIHeroBase
{
	//------------------------------------------------------------------

	protected override DKUnitBase OnCombatAIFindTarget(EActiveSkillType eSkillType)
	{		
		return ProtCombatAIHeroTargetRandom();
	}

	public DKCombatAIHealer() { m_eClassType = EClassType.Healer; }
}
