using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EDKStatType
{
	//First
	Strength,
	Agility,
	Stamina,
	Intelligence,
	Dexterity,
	Luck,
	//Second
	Attack,
	Defence,
	Accuracy,
	Evade,
	Critical,
	AntiCritical,
	Multiplier,
	AntiMultiplier,

	EnergyRecover,
	HealthRecover,
	HealthConversion,
	//Third
	MaxHealthPoint,
	AttackPower,
	DefenceRate,
	EvasionRate,
	AccuracyRate,
	CriticalRate,
	AntiCriticalRate,
	MultiplierRate,
	AntiMultiplierRate,
	MoveSpeed,
}

abstract public class DKStatBase : CStatBase
{
	public const float MAX_STAT = 100000;

	public EDKStatType GetDKStatType()
	{
		return (EDKStatType)base.GetStatType();
	}

	public float GetDKStatBasic()
	{
		return 0;
	}
}

public class DKStatStrength : DKStatBase
{
	public DKStatStrength() {	m_hStatType = (int)EDKStatType.Strength;}

	protected override void OnStatValueChain(int hStatType, float fApplyValue)
	{
	}
}

public class DKStatAgility : DKStatBase
{
	public DKStatAgility() { m_hStatType = (int)EDKStatType.Agility; }

	protected override void OnStatValueChain(int hStatType, float fApplyValue)
	{

	}
}

public class DKStatStamina : DKStatBase
{
	public DKStatStamina() { m_hStatType = (int)EDKStatType.Stamina; }

	protected override void OnStatValueChain(int hStatType, float fApplyValue)
	{

	}
}

public class DKStatIntelligence : DKStatBase
{
	public DKStatIntelligence() { m_hStatType = (int)EDKStatType.Intelligence; }

	protected override void OnStatValueChain(int hStatType, float fApplyValue)
	{

	}
}

public class DKStatDexterity : DKStatBase
{
	public DKStatDexterity() { m_hStatType = (int)EDKStatType.Dexterity; }

	protected override void OnStatValueChain(int hStatType, float fApplyValue)
	{

	}
}

public class DKStatLuck : DKStatBase
{
	public DKStatLuck() { m_hStatType = (int)EDKStatType.Dexterity; }

	protected override void OnStatValueChain(int hStatType, float fApplyValue)
	{

	}
}

//------------------------------------------------------------------------

public class DKStatAttack : DKStatBase
{
	public DKStatAttack() { m_hStatType = (int)EDKStatType.Attack; }

	protected override void OnStatValueChain(int hStatType, float fApplyValue)
	{

	}
}

public class DKStatDefence : DKStatBase
{
	public DKStatDefence() { m_hStatType = (int)EDKStatType.Defence; }

	protected override void OnStatValueChain(int hStatType, float fApplyValue)
	{

	}
}

public class DKStatAccuracy : DKStatBase
{
	public DKStatAccuracy() { m_hStatType = (int)EDKStatType.Accuracy; }

	protected override void OnStatValueChain(int hStatType, float fApplyValue)
	{

	}
}

public class DKStatEvade : DKStatBase
{
	public DKStatEvade() { m_hStatType = (int)EDKStatType.Evade; }

	protected override void OnStatValueChain(int hStatType, float fApplyValue)
	{

	}
}

public class DKStatCritical : DKStatBase
{
	public DKStatCritical() { m_hStatType = (int)EDKStatType.Critical; }

	protected override void OnStatValueChain(int hStatType, float fApplyValue)
	{

	}
}

public class DKStatAntiCritical : DKStatBase
{
	public DKStatAntiCritical() { m_hStatType = (int)EDKStatType.AntiCritical; }

	protected override void OnStatValueChain(int hStatType, float fApplyValue)
	{

	}
}

public class DKStatCriticalMultiplier : DKStatBase
{
	public DKStatCriticalMultiplier() { m_hStatType = (int)EDKStatType.Multiplier; }

	protected override void OnStatValueChain(int hStatType, float fApplyValue)
	{

	}
}

public class DKStatAntiCriticalMultiplier : DKStatBase
{
	public DKStatAntiCriticalMultiplier() { m_hStatType = (int)EDKStatType.AntiMultiplier; }

	protected override void OnStatValueChain(int hStatType, float fApplyValue)
	{

	}
}

public class DKStatEnergyRecover : DKStatBase
{
	public DKStatEnergyRecover() { m_hStatType = (int)EDKStatType.EnergyRecover; }

	protected override void OnStatValueChain(int hStatType, float fApplyValue)
	{

	}
}

public class DKStatHealthRecover : DKStatBase
{
	public DKStatHealthRecover() { m_hStatType = (int)EDKStatType.HealthRecover; }

	protected override void OnStatValueChain(int hStatType, float fApplyValue)
	{

	}
}

public class DKStatHealthConversion : DKStatBase
{
	public DKStatHealthConversion() { m_hStatType = (int)EDKStatType.HealthConversion; }

	protected override void OnStatValueChain(int hStatType, float fApplyValue)
	{

	}
}

public class DKStatMaxHealthPoint : DKStatBase
{
	public DKStatMaxHealthPoint() { m_hStatType = (int)EDKStatType.MaxHealthPoint; }

	protected override void OnStatValueChain(int hStatType, float fApplyValue)
	{

	}
}

public class DKStatAttackPower : DKStatBase
{
	public DKStatAttackPower() { m_hStatType = (int)EDKStatType.AttackPower; }

	protected override void OnStatValueChain(int hStatType, float fApplyValue)
	{

	}
}

public class DKStatDefenceRate : DKStatBase
{
	public DKStatDefenceRate() { m_hStatType = (int)EDKStatType.DefenceRate; }

	protected override void OnStatValueChain(int hStatType, float fApplyValue)
	{

	}
}

public class DKStatEvasionRate : DKStatBase
{
	public DKStatEvasionRate() { m_hStatType = (int)EDKStatType.EvasionRate; }

	protected override void OnStatValueChain(int hStatType, float fApplyValue)
	{

	}
}

public class DKStatAccuracyRate : DKStatBase
{
	public DKStatAccuracyRate() { m_hStatType = (int)EDKStatType.AccuracyRate; }

	protected override void OnStatValueChain(int hStatType, float fApplyValue)
	{

	}
}

public class DKStatCriticalRate : DKStatBase
{
	public DKStatCriticalRate() { m_hStatType = (int)EDKStatType.CriticalRate; }

	protected override void OnStatValueChain(int hStatType, float fApplyValue)
	{

	}
}

public class DKStatAntiCriticalRate : DKStatBase
{
	public DKStatAntiCriticalRate() { m_hStatType = (int)EDKStatType.AntiCriticalRate; }

	protected override void OnStatValueChain(int hStatType, float fApplyValue)
	{

	}
}

public class DKStatCriticalMultiplierRate : DKStatBase
{
	public DKStatCriticalMultiplierRate() { m_hStatType = (int)EDKStatType.MultiplierRate; }

	protected override void OnStatValueChain(int hStatType, float fApplyValue)
	{

	}
}

public class DKStatAntiCriticalMultiplierRate : DKStatBase
{
	public DKStatAntiCriticalMultiplierRate() { m_hStatType = (int)EDKStatType.AntiMultiplierRate; }

	protected override void OnStatValueChain(int hStatType, float fApplyValue)
	{

	}
}

public class DKStatMoveSpeed : DKStatBase
{
	public DKStatMoveSpeed() { m_hStatType = (int)EDKStatType.MoveSpeed; }

	protected override void OnStatValueChain(int hStatType, float fApplyValue)
	{

	}
}