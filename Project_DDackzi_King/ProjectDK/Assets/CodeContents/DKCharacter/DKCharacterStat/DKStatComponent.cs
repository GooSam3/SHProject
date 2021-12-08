using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DKStatComponent : CStatComponentBase
{
	[SerializeField]
	private DKStatGroupFirst First = new DKStatGroupFirst();		// 레벨과 장비, 버프등의 합산수치 
	[SerializeField]
	private DKStatGroupSecond Second = new DKStatGroupSecond();	// 1차 스텟에 의해 산출된 수치
	[SerializeField]
	private DKStatGroupThird Third = new DKStatGroupThird();		// 2차 스텟에 의해 산출된 수치


	[SerializeField]
	private DKStatNormal Normal = new DKStatNormal();  // ToDo 이 부분은 암호화 할것

	//--------------------------------------------
	protected override void OnStatComponentArrange(IStatOwner pStatOwner, List<CStatBase> pListOutInstance)
	{
		First.DoStatGroupInitialize(pStatOwner, this, pListOutInstance);
		Second.DoStatGroupInitialize(pStatOwner, this, pListOutInstance);
		Third.DoStatGroupInitialize(pStatOwner, this, pListOutInstance);

		// 각 스텟 연결 
	}

	protected override void OnStatComponentReset()
	{
		PrivStatCompResetStat();
	}

	//--------------------------------------------
	public void DoStatApplyDamage(CUnitBuffBase.SDamageResult rDamageResult)
	{
		Normal.HP -= rDamageResult.fDamage;
		if (Normal.HP <= 0)
		{
			Normal.HP = 0;
			m_pStatOwner.IStatDie(rDamageResult.pAttacker);
		}
	}

	//--------------------------------------------
	public float GetDKUnitStat(EDKStatType eStatType)
	{
		float fResult = 0;
		CStatBase pStat = FindStatInstance((int)eStatType);
		if (pStat != null)
		{
			fResult = pStat;
		}
		return fResult;
	}

	public float GetDKUnitHP()
	{
		return Normal.HP;
	}

	public float GetDKUnitEnergy()
	{
		return Normal.Energy;
	}

	//---------------------------------------------
	private void PrivStatCompResetStat()
	{
		CStatBase pStat = FindStatInstance((int)EDKStatType.MaxHealthPoint);
		Normal.HP = pStat;
		Normal.Energy = 0;
	}

}

[System.Serializable]
public class DKStatNormal
{
	public float HP;
	public float Energy;
	public uint Level;
	public uint Experience;
	public uint MaxExperience;
}

[System.Serializable]
public class DKStatGroupFirst : CStatGroupBase
{
	public float Strength = 100;
	public float Agility = 100;
	public float Stamina = 100;
	public float Intelligence = 100;
	public float Dexterity = 100;
	public float Luck = 100;

	protected override void OnStatGroupInitialize(IStatOwner pStatOwner, List<CStatBase> pLitOutInstance)
	{
		DKStatStrength pStrength = new DKStatStrength();
		pStrength.DoStatValueReset(Strength, DKStatBase.MAX_STAT, pStatOwner);
		pLitOutInstance.Add(pStrength);

		DKStatAgility pAgility = new DKStatAgility();
		pAgility.DoStatValueReset(Agility, DKStatBase.MAX_STAT, pStatOwner);
		pLitOutInstance.Add(pAgility);

		DKStatStamina pHealth = new DKStatStamina();
		pHealth.DoStatValueReset(Stamina, DKStatBase.MAX_STAT, pStatOwner);
		pLitOutInstance.Add(pHealth);

		DKStatIntelligence pIntelligence = new DKStatIntelligence();
		pIntelligence.DoStatValueReset(Intelligence, DKStatBase.MAX_STAT, pStatOwner);
		pLitOutInstance.Add(pIntelligence);

		DKStatDexterity pDexterity = new DKStatDexterity();
		pDexterity.DoStatValueReset(Dexterity, DKStatBase.MAX_STAT, pStatOwner);
		pLitOutInstance.Add(pDexterity);

		DKStatLuck pLuck = new DKStatLuck();
		pLuck.DoStatValueReset(Luck, DKStatBase.MAX_STAT, pStatOwner);
		pLitOutInstance.Add(pLuck);
	}
}

public class DKStatGroupSecond : CStatGroupBase
{
	public float Attack = 100;
	public float Defence = 100;
	public float Accuracy = 100;
	public float Evade = 100;
	public float Critical = 100;
	public float AntiCritical = 100;
	public float CriticalMultiplier = 100;
	public float AntiCriticalMultiplier = 100;
	public float EnergyRecover = 100;
	public float HealthRecover = 10;
	public float HealthConversion = 0;

	protected override void OnStatGroupInitialize(IStatOwner pStatOwner, List<CStatBase> pLitOutInstance)
	{
		DKStatBase pStat = new DKStatAttack();
		pStat.DoStatValueReset(Attack, DKStatBase.MAX_STAT, pStatOwner);
		pLitOutInstance.Add(pStat);

		pStat = new DKStatDefence();
		pStat.DoStatValueReset(Defence, DKStatBase.MAX_STAT, pStatOwner);
		pLitOutInstance.Add(pStat);

		pStat = new DKStatAccuracy();
		pStat.DoStatValueReset(Accuracy, DKStatBase.MAX_STAT, pStatOwner);
		pLitOutInstance.Add(pStat);

		pStat = new DKStatEvade();
		pStat.DoStatValueReset(Evade, DKStatBase.MAX_STAT, pStatOwner);
		pLitOutInstance.Add(pStat);

		pStat = new DKStatCritical();
		pStat.DoStatValueReset(Critical, DKStatBase.MAX_STAT, pStatOwner);
		pLitOutInstance.Add(pStat);

		pStat = new DKStatAntiCritical();
		pStat.DoStatValueReset(AntiCritical, DKStatBase.MAX_STAT, pStatOwner);
		pLitOutInstance.Add(pStat);

		pStat = new DKStatCriticalMultiplier();
		pStat.DoStatValueReset(CriticalMultiplier, DKStatBase.MAX_STAT, pStatOwner);
		pLitOutInstance.Add(pStat);

		pStat = new DKStatAntiCriticalMultiplier();
		pStat.DoStatValueReset(AntiCriticalMultiplier, DKStatBase.MAX_STAT, pStatOwner);
		pLitOutInstance.Add(pStat);

		pStat = new DKStatEnergyRecover();
		pStat.DoStatValueReset(EnergyRecover, DKStatBase.MAX_STAT, pStatOwner);
		pLitOutInstance.Add(pStat);

		pStat = new DKStatHealthRecover();
		pStat.DoStatValueReset(HealthRecover, DKStatBase.MAX_STAT, pStatOwner);
		pLitOutInstance.Add(pStat);

		pStat = new DKStatHealthConversion();
		pStat.DoStatValueReset(HealthConversion, DKStatBase.MAX_STAT, pStatOwner);
		pLitOutInstance.Add(pStat);
	}
}


public class DKStatGroupThird : CStatGroupBase
{
	public float MaxHealthPoint = 1000;
	public float AttackPower = 100;
	public float DefenceRate = 0;
	public float EvasionRate = 0;
	public float AccuracyRate = 0;
	public float CriticalRate = 0;
	public float AntiCriticalRate = 0;
	public float CriticalMultiplier = 0;
	public float AntiCriticalMultiplier = 0;
	public float MoveSpeed = 0;

	protected override void OnStatGroupInitialize(IStatOwner pStatOwner, List<CStatBase> pLitOutInstance)
	{
		DKStatBase pStat = new DKStatMaxHealthPoint();
		pStat.DoStatValueReset(MaxHealthPoint, DKStatBase.MAX_STAT, pStatOwner);
		pLitOutInstance.Add(pStat);

		pStat = new DKStatAttackPower();
		pStat.DoStatValueReset(AttackPower, DKStatBase.MAX_STAT, pStatOwner);
		pLitOutInstance.Add(pStat);

		pStat = new DKStatDefenceRate();
		pStat.DoStatValueReset(DefenceRate, DKStatBase.MAX_STAT, pStatOwner);
		pLitOutInstance.Add(pStat);

		pStat = new DKStatEvasionRate();
		pStat.DoStatValueReset(EvasionRate, DKStatBase.MAX_STAT, pStatOwner);
		pLitOutInstance.Add(pStat);

		pStat = new DKStatAccuracyRate();
		pStat.DoStatValueReset(AccuracyRate, DKStatBase.MAX_STAT, pStatOwner);
		pLitOutInstance.Add(pStat);

		pStat = new DKStatCriticalRate();
		pStat.DoStatValueReset(CriticalRate, DKStatBase.MAX_STAT, pStatOwner);
		pLitOutInstance.Add(pStat);

		pStat = new DKStatAntiCriticalRate();
		pStat.DoStatValueReset(AntiCriticalRate, DKStatBase.MAX_STAT, pStatOwner);
		pLitOutInstance.Add(pStat);

		pStat = new DKStatCriticalMultiplierRate();
		pStat.DoStatValueReset(CriticalMultiplier, DKStatBase.MAX_STAT, pStatOwner);
		pLitOutInstance.Add(pStat);

		pStat = new DKStatAntiCriticalMultiplierRate();
		pStat.DoStatValueReset(AntiCriticalMultiplier, DKStatBase.MAX_STAT, pStatOwner);
		pLitOutInstance.Add(pStat);

		pStat = new DKStatMoveSpeed();
		pStat.DoStatValueReset(MoveSpeed, DKStatBase.MAX_STAT, pStatOwner);
		pLitOutInstance.Add(pStat);

	}
}