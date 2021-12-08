using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SHStatComponentBase : CStatComponentBase, IStatOwner
{
	public const float c_FeverMax = 1000;
	[SerializeField]
	protected uint UnitStatID = 0;
	[SerializeField]
	protected int UnitLevel = 0;

	[SerializeField]
	protected SHStatGroupCombat	ShowStat = new SHStatGroupCombat();

	protected SHStatGroupCombat	CombatStat = new SHStatGroupCombat();            // 실제 전투계산에 사용되는 수치 
	[SerializeField]
	protected SHStatRuntime		RuntimeStat = new SHStatRuntime();               // ToDo 이 부분은 암호화 할것

	private SHUnitBase m_pSHUnit = null;
	//----------------------------------------------------------------------------------
	protected override void OnStatComponentArrange(CUnitStatBase pStatOwner, List<CStatBase> pLitOutInstance)
	{
		CombatStat.DoStatGroupInitialize(this, pLitOutInstance);
		m_pSHUnit = pStatOwner as SHUnitBase;
	}

	protected override void OnStatComponentReset()
	{
		CombatStat.DoStatGroupRefresh(this);
		ProtStatCompResetRuntimeStat();  
	}

	//-----------------------------------------------------------------------------------

	public SDamageResult DoStatDamageCalculation(SHStatComponentBase pDefender, NSkill.EDamageType eDamageType, float fPower, List<SHStatModifier> pListAdditionalStat)
	{
		SDamageResult rResult = new SDamageResult();
		rResult.pAttacker = m_pStatOwner as SHUnitBase;
		rResult.pDefender = pDefender.m_pStatOwner as SHUnitBase;
		rResult.eDamageType = eDamageType;

		switch (eDamageType)
		{
			case NSkill.EDamageType.AttackNormal:
				PrivStatCompCalculationNormal(rResult, pDefender, fPower, pListAdditionalStat);
				break;
			case NSkill.EDamageType.AttackSkill:
				PrivStatCompCalculationSkill(rResult, pDefender, fPower, pListAdditionalStat);
				break;
			case NSkill.EDamageType.Heal:
				PrivStatCompCalculationHeal(rResult, pDefender, fPower, pListAdditionalStat);
				break;
		}

		return rResult;
	}

	public void DoStatApplyDamage(SDamageResult pDamageResult)
	{
		if (pDamageResult.eDamageResult == EDamageResult.Dodge)
		{
			return;
		}
		
		RuntimeStat.HP -= (int)pDamageResult.fTotalValue;
		if (RuntimeStat.HP <= 0)
		{
			RuntimeStat.HP = 0;
			m_pSHUnit.DoUnitStatDeathStart();
		}
	}

	public void DoStatApplyHeal(SDamageResult pDamageResult)
	{
		RuntimeStat.HP += pDamageResult.fTotalValue;
		if (RuntimeStat.HP > RuntimeStat.MaxHP)
		{
			RuntimeStat.HP = RuntimeStat.MaxHP;
		}
	}

	public void DoStatRecoverHP(float fDelta)
	{
		float fRecoverStat = GetStatValue(ESHStatType.RecoverPerSecond);
		float fRecoverValue = fRecoverStat * fDelta;

		RuntimeStat.HP += fRecoverValue;
		if (RuntimeStat.HP > RuntimeStat.MaxHP)
		{
			RuntimeStat.HP = RuntimeStat.MaxHP;
		}

	}

	public float GetStatValue(ESHStatType eStatType)
	{
		float fValue = 0;
		CStatBase pStat = FindStatInstance((uint)eStatType);
		if (pStat != null)
		{
			fValue = pStat;
		}
		return fValue;
	}

	public float GetStatValueBasic(ESHStatType eStatType)
	{
		float fValue = 0;
		CStatBase pStat = FindStatInstance((uint)eStatType);
		if (pStat != null)
		{
			fValue = pStat.GetStatValueBasic();
		}
		return fValue;
	}

	public Vector2 GetStatHP()
	{
		Vector2 HP = new Vector2();
		HP.x = (int)RuntimeStat.HP;
		HP.y = (int)RuntimeStat.MaxHP;
		return HP;
	}

	public void DoStatID(uint hUnitStatID, int iLevel)
	{
		UnitStatID = hUnitStatID;
		UnitLevel = iLevel;
		DoStatReset();
	}

	public int GetStatLevel()
	{
		return UnitLevel;
	}

	//------------------------------------------------------------------------------------
	public void IStatUpdate(uint hStatType, float fStatValue)
	{
		PrivStatComRefreshCombatStat((ESHStatType)hStatType, (int)fStatValue);
	}
	public uint IStatLevel()
	{
		return 0;
	}

	public CStatBase IStatFind(uint hStatType)
	{
		return FindStatInstance(hStatType);
	}

	public float IStatValue(uint hStatType)
	{
		return GetStatValue((ESHStatType)hStatType);
	}

	//------------------------------------------------------------------------------------
	private void PrivStatCompCalculationNormal(SDamageResult rDamageResult, SHStatComponentBase pDefender, float fPower, List<SHStatModifier> pListAdditionalStat)
	{
		if (CheckStatCompHit(pDefender, pListAdditionalStat) == false)
		{
			rDamageResult.eDamageResult = EDamageResult.Dodge;
		}
		else
		{
			PrivStatCompCalculationToDamage(rDamageResult, pDefender, fPower, false, pListAdditionalStat);
		}
	}

	private void PrivStatCompCalculationSkill(SDamageResult rDamageResult, SHStatComponentBase pDefender, float fPower, List<SHStatModifier> pListAdditionalStat)
	{
		if (CheckStatCompHit(pDefender, pListAdditionalStat) == false)
		{
			rDamageResult.eDamageResult = EDamageResult.Dodge;
		}
		else
		{
			PrivStatCompCalculationToDamage(rDamageResult, pDefender, fPower, false, pListAdditionalStat);
		}
	}

	private void PrivStatCompCalculationHeal(SDamageResult rDamageResult, SHStatComponentBase pDefender, float fPower, List<SHStatModifier> pListAdditionalStat)
	{
		PrivStatCompCalculationToHeal(rDamageResult, pDefender, fPower, pListAdditionalStat);
	}

	private bool CheckStatCompHit(SHStatComponentBase pDefender, List<SHStatModifier> pListAdditionalStat)
	{
		bool bHit = false;
		int iHitRate = (int)(GetStatValue(ESHStatType.HitRate) - pDefender.GetStatValue(ESHStatType.DodgeRate));
		int iHitSeed = Random.Range(0, (int)c_GlobalChance);
		if (iHitSeed <= iHitRate)
		{
			bHit = true;
		}
		else
		{
			bHit = false;
		}
		return bHit;
	}
	
	private bool CheckStatCompCritical(SHStatComponentBase pDefender, List<SHStatModifier> pListAdditionalStat)
	{
		bool bCritical = false;

		//int iCriticalRate = (int)(GetStatValue(ESHStatType.CriticalRate) - pDefender.GetStatValue(ESHStatType.CriticalRateAnti));
		//int iCriticalSeed = Random.Range(0, (int)c_GlobalChance);
		//if (iCriticalSeed <= iCriticalRate)
		//{
		//	bCritical = true;
		//}

		int Temp = Random.Range(0, 2);
		if (Temp == 0)
		{
			bCritical = true;
		}
		return bCritical;
	}

	private void PrivStatComRefreshCombatStat(ESHStatType eStatType, int fValue)
	{
		switch(eStatType)
		{
			case ESHStatType.AttackPower:
				ShowStat.AttackPower = fValue;
				break;
			case ESHStatType.DefensePower:
				ShowStat.DefencePower = fValue;
				break;
			case ESHStatType.DamagePercent:
				ShowStat.DamagePercent = fValue;
				break;
			case ESHStatType.ReducePercent:
				ShowStat.ReducePercent = fValue;
				break;
			case ESHStatType.CriticalRate:
				ShowStat.CriticalRate = fValue;
				break;
			case ESHStatType.CriticalRateAnti:
				ShowStat.CriticalRateAnti = fValue;
				break;
			case ESHStatType.CriticalDamageRate:
				ShowStat.CriticalDamageRate = fValue;
				break;
			case ESHStatType.CriticalDamageRateAnti:
				ShowStat.CriticalDamageRateAnti = fValue;
				break;
			case ESHStatType.HitRate:
				ShowStat.HitRate = fValue;
				break;
			case ESHStatType.DodgeRate:
				ShowStat.DodgeRate = fValue;
				break;
			case ESHStatType.BlockRate:
				ShowStat.BlockRate = fValue;
				break;
			case ESHStatType.BlockAntiRate:
				ShowStat.BlockAntiRate = fValue;
				break;
			case ESHStatType.MaxHitPoint:
				ShowStat.MaxHitPoint = fValue;
				break;
			case ESHStatType.SkillDamagePercent:
				ShowStat.SkillDamagePercent = fValue;
				break;
			case ESHStatType.SkillReducePercent:
				ShowStat.SkillReducePercent = fValue;
				break;
		}
	}

	private void PrivStatCompCalculationToDamage(SDamageResult rDamageResult, SHStatComponentBase pDefender, float fPower, bool bSkill, List<SHStatModifier> pListAdditionalStat)
	{
		float fAttack = GetStatValue(ESHStatType.AttackPower);
		float fDefance = pDefender.GetStatValue(ESHStatType.DefensePower);

		if (fAttack < fDefance)
		{
			fDefance = fAttack - 1;
		}

		if (bSkill)
		{
			fAttack += GetStatValue(ESHStatType.AttackSkillRate);
			fDefance += pDefender.GetStatValue(ESHStatType.DefenseSkillRate);
		}
		
		float fDamageAdjust = (GetStatValue(ESHStatType.DamagePercent) - pDefender.GetStatValue(ESHStatType.ReducePercent)) / c_GlobalChance;
		float fDamageSkillAdjust = 0f;

		if (bSkill)
		{
			fDamageSkillAdjust = (GetStatValue(ESHStatType.SkillDamagePercent) - pDefender.GetStatValue(ESHStatType.SkillReducePercent)) / c_GlobalChance;
		}

		float fRandomDamage = Random.Range(0f, 0.05f);
		float fBaseDamage = (fAttack - fDefance) * fPower;
		fBaseDamage += (fBaseDamage * fRandomDamage);
		float fCriticalDamageMulti = 1f;
		float fTotalDamage = fBaseDamage + (fBaseDamage * fDamageAdjust) + (fBaseDamage * fDamageSkillAdjust);

		if (CheckStatCompCritical(pDefender, pListAdditionalStat))
		{
			fCriticalDamageMulti = (GetStatValue(ESHStatType.CriticalDamageRate) - pDefender.GetStatValue(ESHStatType.CriticalDamageRateAnti)) / c_GlobalChance;
			rDamageResult.eDamageResult = EDamageResult.Critical;
		}
		else
		{
			rDamageResult.eDamageResult = EDamageResult.Normal;
		}

		rDamageResult.fTotalValue = fTotalDamage * fCriticalDamageMulti;
	}

	private void PrivStatCompCalculationToHeal(SDamageResult rDamageResult, SHStatComponentBase pDefender, float fPower, List<SHStatModifier> pListAdditionalStat)
	{
		float fHeal = GetStatValue(ESHStatType.AttackPower);
		float fHealSkillAdjust = pDefender.GetStatValue(ESHStatType.SkillDamagePercent) / c_GlobalChance;
		float fRandomDamage = Random.Range(0f, 0.05f);
		float fCriticalDamageMulti = 1f;
		float fBaseHeal = fHeal * fPower * fHealSkillAdjust;
		fBaseHeal += (fBaseHeal * fRandomDamage);

		if (CheckStatCompCritical(pDefender, pListAdditionalStat))
		{
			fCriticalDamageMulti = (GetStatValue(ESHStatType.CriticalDamageRate) - pDefender.GetStatValue(ESHStatType.CriticalDamageRateAnti)) / c_GlobalChance;
			rDamageResult.eDamageResult = EDamageResult.Critical;
		}
		else
		{
			rDamageResult.eDamageResult = EDamageResult.Normal;
		}

		rDamageResult.fTotalValue = fBaseHeal * fCriticalDamageMulti;
	}


	//-----------------------------------------------------------
	protected SHStatBase FindSHStatInstance(ESHStatType eStatType)
	{
		return FindStatInstance((uint)eStatType) as SHStatBase;
	}

	protected void ProtStatCompResetRuntimeStat()
	{
		RuntimeStat.MaxHP = (int)GetStatValue(ESHStatType.MaxHitPoint);
		RuntimeStat.HP = RuntimeStat.MaxHP;
	}
}

[System.Serializable]
public class SHStatRuntime
{
	public float HP = 1;
	public float MaxHP = 1;
}

[System.Serializable]
public class SHStatGroupCombat : CStatGroupBase
{
	public int AttackPower = 0;
	public int DefencePower = 0;
	public int MaxHitPoint = 0;

	public int AttackSkillRate = 0;
	public int DefenceSkillRate = 0;
	public int CriticalRate = 0;
	public int CriticalRateAnti = 0;
	public int CriticalDamageRate = 0;
	public int CriticalDamageRateAnti = 0;
	public int AttackSpeed = 0;
	public int HitRate = 0;
	public int DodgeRate = 0;
	public int BlockRate = 0;
	public int BlockAntiRate = 0;
	
	public int DamagePercent = 0;
	public int ReducePercent = 0;
	public int SkillDamagePercent = 0;
	public int SkillReducePercent = 0;

	protected override void OnStatGroupInitialize(IStatOwner pStatOwner, List<CStatBase> pListOutInstance)
	{
		SHStatBase pStat = new SHStatAttackPower();
		pListOutInstance.Add(pStat);

		pStat = new SHStatAttackSkillPower();
		pListOutInstance.Add(pStat);

		pStat = new SHStatDefencePower();
		pListOutInstance.Add(pStat);

		pStat = new SHStatDefenceSkillPower();
		pListOutInstance.Add(pStat);

		pStat = new SHStatCriticalRate();
		pListOutInstance.Add(pStat);

		pStat = new SHStatCriticalRateAnti();
		pListOutInstance.Add(pStat);

		pStat = new SHStatCriticalDamageRate();
		pStat.DoStatValueConstant(15000f);
		pListOutInstance.Add(pStat);

		pStat = new SHStatCriticalDamageRateAnti();
		pListOutInstance.Add(pStat);

		pStat = new SHStatHitRate();
		pStat.DoStatValueConstant(9500f);
		pListOutInstance.Add(pStat);

		pStat = new SHStatDodgeRate();
		pListOutInstance.Add(pStat);

		pStat = new SHStatMaxHitPoint();
		pListOutInstance.Add(pStat);

		pStat = new SHStatAttackSpeed();
		pStat.DoStatValueConstant(1f);
		pListOutInstance.Add(pStat);

		pStat = new SHStatDamagePercent();
		pStat.DoStatValueConstant(10000f);
		pListOutInstance.Add(pStat);

		pStat = new SHStatReducePercent();
		pStat.DoStatValueConstant(10000f);
		pListOutInstance.Add(pStat);

		pStat = new SHStatSkillDamagePercent();
		pStat.DoStatValueConstant(10000f);
		pListOutInstance.Add(pStat);

		pStat = new SHStatSkillReducePercent();
		pStat.DoStatValueConstant(10000f);
		pListOutInstance.Add(pStat);

		pStat = new SHStatBlockRate();
		pListOutInstance.Add(pStat);

		pStat = new SHStatBlockAntiRate();
		pListOutInstance.Add(pStat);

	}

	protected override void OnStatGroupRefresh(IStatOwner pStatOwner)
	{
		base.OnStatGroupRefresh(pStatOwner);
		CStatBase pStat = pStatOwner.IStatFind((int)ESHStatType.AttackPower);
		pStat.DoStatValueBasicReset(AttackPower, pStatOwner);

		pStat = pStatOwner.IStatFind((int)ESHStatType.DamagePercent);
		pStat.DoStatValueBasicReset(DamagePercent, pStatOwner);

		pStat = pStatOwner.IStatFind((int)ESHStatType.DefensePower);
		pStat.DoStatValueBasicReset(DefencePower, pStatOwner);

		pStat = pStatOwner.IStatFind((int)ESHStatType.ReducePercent);
		pStat.DoStatValueBasicReset(ReducePercent, pStatOwner);

		pStat = pStatOwner.IStatFind((int)ESHStatType.CriticalRate);
		pStat.DoStatValueBasicReset(CriticalRate, pStatOwner);

		pStat = pStatOwner.IStatFind((int)ESHStatType.CriticalRateAnti);
		pStat.DoStatValueBasicReset(CriticalRateAnti, pStatOwner);

		pStat = pStatOwner.IStatFind((int)ESHStatType.CriticalDamageRate);
		pStat.DoStatValueBasicReset(CriticalDamageRate, pStatOwner);

		pStat = pStatOwner.IStatFind((int)ESHStatType.CriticalDamageRateAnti);
		pStat.DoStatValueBasicReset(CriticalDamageRateAnti, pStatOwner);

		pStat = pStatOwner.IStatFind((int)ESHStatType.AttackSpeed);
		pStat.DoStatValueBasicReset(AttackSpeed, pStatOwner);

		pStat = pStatOwner.IStatFind((int)ESHStatType.HitRate);
		pStat.DoStatValueBasicReset(HitRate, pStatOwner);

		pStat = pStatOwner.IStatFind((int)ESHStatType.DodgeRate);
		pStat.DoStatValueBasicReset(DodgeRate, pStatOwner);

		pStat = pStatOwner.IStatFind((int)ESHStatType.MaxHitPoint);
		pStat.DoStatValueBasicReset(MaxHitPoint, pStatOwner);

		pStat = pStatOwner.IStatFind((int)ESHStatType.SkillDamagePercent);
		pStat.DoStatValueBasicReset(SkillDamagePercent, pStatOwner);

		pStat = pStatOwner.IStatFind((int)ESHStatType.SkillReducePercent);
		pStat.DoStatValueBasicReset(SkillReducePercent, pStatOwner);

		pStat = pStatOwner.IStatFind((int)ESHStatType.BlockRate);
		pStat.DoStatValueBasicReset(BlockRate, pStatOwner);

		pStat = pStatOwner.IStatFind((int)ESHStatType.BlockAntiRate);
		pStat.DoStatValueBasicReset(BlockAntiRate, pStatOwner);
	}
}

