using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DKCombatAIDefender : DKCombatAIHeroBase
{
	// ������� ���� ���ݷ��� ���� ��븦 �����ϴ� ������ ���ϴ�. 
	protected override DKUnitBase OnCombatAIFindTarget(EActiveSkillType eSkillType)
	{
		return ProtCombatAIHeroTarget(EDKStatType.AttackPower, true);
	}

	public DKCombatAIDefender() { m_eClassType = EClassType.Defender; }
}
