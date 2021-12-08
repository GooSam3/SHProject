using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NBuff
{
	public class DKBuffEnum { }

	public enum EBuffStatFirst
	{
		None,
		Strength,
		Agility,
		Inteligence,
		Dexterity,
		Stamina,
		Luck,
	}

	public enum EBuffStatSecond
	{
		None,
		Attack,
		Defence,
		Accuracy,
		Evade,
		Critical,
		AniCritical,
		CriticalMuliplier,
		EnergyRecover,
		HealthRecover,
		HealthConversion,
	}

	public enum EBuffStatThird
	{
		None,
		MaxHealthPoint,
		DefenceRate,
		EvasionRate,
		AccuracyRate,
		Speed,
	}

	public enum EDamageType
	{
		None,
		Physical,
		Magical,
	}

	public enum EBuffTaskTarget
	{
		None,
		BuffOwner,
		BuffOrigin,
	}
}
