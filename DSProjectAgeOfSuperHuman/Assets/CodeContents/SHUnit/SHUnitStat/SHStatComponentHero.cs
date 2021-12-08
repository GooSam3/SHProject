using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SHStatComponentHero : SHStatComponentBase
{
	[SerializeField]
	private SHStatGroupBasic   HeroStat = new SHStatGroupBasic();            // 영웅 고유의 능력치  

	//----------------------------------------------------------------
	protected override void OnStatComponentArrange(CUnitStatBase pStatOwner, List<CStatBase> pListOutInstance)
	{
		base.OnStatComponentArrange(pStatOwner, pListOutInstance);
		HeroStat.DoStatGroupInitialize(this, pListOutInstance);		
	}

	protected override void OnStatComponentChainLink()
	{
		base.OnStatComponentChainLink();
		PrivStatHeroChainLink();
	}

	protected override void OnStatComponentReset()
	{
		base.OnStatComponentReset();	
		
		HeroStat.DoStatGroupRefresh(this);
		ProtStatCompResetRuntimeStat();
	}

	//-----------------------------------------------------------------
	public void SetStatComponentUpgrade(SHStatGroupBasic pUpgrade)
	{
		HeroStat = pUpgrade;
		DoStatReset();
	}

	//--------------------------------------------------------------------
	private void PrivStatHeroChainLink()
	{
		SHStatBase pStatPower = FindSHStatInstance(ESHStatType.AttackPower);
		SHStatBase pStatValue = FindSHStatInstance(ESHStatType.Attack);
		SHStatBase pStatPercent = FindSHStatInstance(ESHStatType.AttackPercent);

		pStatValue.SetStatValueChain(pStatPower);
		pStatPercent.SetStatValueChain(pStatPower);
		pStatPower.DoStatValueRefresh();

		pStatPower = FindSHStatInstance(ESHStatType.DefensePower);
		pStatValue = FindSHStatInstance(ESHStatType.Defense);
		pStatPercent = FindSHStatInstance(ESHStatType.DefencePercent);

		pStatValue.SetStatValueChain(pStatPower);
		pStatPercent.SetStatValueChain(pStatPower);
		pStatPower.DoStatValueRefresh();

		pStatPower = FindSHStatInstance(ESHStatType.MaxHitPoint);
		pStatValue = FindSHStatInstance(ESHStatType.Stamina);
		pStatPercent = FindSHStatInstance(ESHStatType.StaminaPercent);

		pStatValue.SetStatValueChain(pStatPower);
		pStatPercent.SetStatValueChain(pStatPower);
		pStatPower.DoStatValueRefresh();
	}

}
[System.Serializable]
public class SHStatGroupBasic : CStatGroupBase
{
	[SerializeField]
	public uint Attack = 0;
	[SerializeField]
	public uint Defence = 0;
	[SerializeField]
	public uint Stamina = 0;

	[SerializeField]
	public uint AttackPercent = 0;
	[SerializeField]
	public uint DefencePercent = 0;
	[SerializeField]
	public uint StaminaPercent = 0;

	[SerializeField]
	public uint AttackSkill = 0;
	[SerializeField]
	public uint DefenceSkill = 0;
	[SerializeField]
	public uint Critical = 0;
	[SerializeField]
	public uint CriticalAnti = 0;
	[SerializeField]
	public uint CriticalDamage = 0;
	[SerializeField]
	public uint CriticalDamageAnti = 0;
	[SerializeField]
	public uint Hit = 0;
	[SerializeField]
	public uint Dodge = 0;
	[SerializeField]
	public uint Block = 0;
	[SerializeField]
	public uint BlockAnti = 0;
	[SerializeField]
	public uint RecoverPerSecond = 0;
	[SerializeField]
	public uint ExtraGold = 0;
	[SerializeField]
	public uint ExtraEXP = 0;
	[SerializeField]
	public uint ExtraItem = 0;

	public void MergerBasicValue(SHStatGroupBasic pOther)
	{
		Attack += pOther.Attack;
		AttackSkill += pOther.AttackSkill;
		Defence += pOther.Defence;
		DefenceSkill += pOther.DefenceSkill;
		Critical += pOther.Critical;
		CriticalAnti += pOther.CriticalAnti;
		CriticalDamage += pOther.CriticalDamage;
		CriticalDamageAnti += pOther.CriticalDamageAnti;
		Hit += pOther.Hit;
		Dodge += pOther.Dodge;
		Block += pOther.Block;
		BlockAnti += pOther.BlockAnti;
		RecoverPerSecond += pOther.RecoverPerSecond;
		Stamina += pOther.Stamina;
		ExtraGold += pOther.ExtraGold;
		ExtraEXP += pOther.ExtraEXP;
	}

	protected override void OnStatGroupInitialize(IStatOwner pStatOwner, List<CStatBase> pListOutInstance)
	{
		SHStatBase pStat = new SHStatAttack();
		pListOutInstance.Add(pStat);

		pStat = new SHStatAttackSkill();
		pListOutInstance.Add(pStat);

		pStat = new SHStatDefence();
		pListOutInstance.Add(pStat);

		pStat = new SHStatDefenceSkill();
		pListOutInstance.Add(pStat);

		pStat = new SHStatCritical();
		pListOutInstance.Add(pStat);

		pStat = new SHStatCriticalAnti();
		pListOutInstance.Add(pStat);

		pStat = new SHStatCriticalDamage();
		pListOutInstance.Add(pStat);

		pStat = new SHStatCriticalDamageAnti();
		pListOutInstance.Add(pStat);

		pStat = new SHStatHit();
		pListOutInstance.Add(pStat);

		pStat = new SHStatDodge();
		pListOutInstance.Add(pStat);

		pStat = new SHStatBlock();
		pListOutInstance.Add(pStat);

		pStat = new SHStatBlockAnti();
		pListOutInstance.Add(pStat);

		pStat = new SHStatRecoverPerSecond();
		pListOutInstance.Add(pStat);

		pStat = new SHStatStamina();
		pListOutInstance.Add(pStat);

		pStat = new SHStatExtraGold();
		pListOutInstance.Add(pStat);

		pStat = new SHStatExtraEXP();
		pListOutInstance.Add(pStat);

		pStat = new SHStatExtraItem();
		pListOutInstance.Add(pStat);

		pStat = new SHStatAttackPercent();
		pListOutInstance.Add(pStat);

		pStat = new SHStatDefencePercent();
		pListOutInstance.Add(pStat);

		pStat = new SHStatStaminaPercent();
		pListOutInstance.Add(pStat);
	}

	protected override void OnStatGroupRefresh(IStatOwner pStatOwner)
	{
		CStatBase pStat = pStatOwner.IStatFind((int)ESHStatType.Attack);
		pStat.DoStatValueBasicReset(Attack, pStatOwner);

		pStat = pStatOwner.IStatFind((int)ESHStatType.AttackSkill);
		pStat.DoStatValueBasicReset(AttackSkill, pStatOwner);

		pStat = pStatOwner.IStatFind((int)ESHStatType.Defense);
		pStat.DoStatValueBasicReset(Defence, pStatOwner);

		pStat = pStatOwner.IStatFind((int)ESHStatType.DefenseSkill);
		pStat.DoStatValueBasicReset(DefenceSkill, pStatOwner);

		pStat = pStatOwner.IStatFind((int)ESHStatType.Critical);
		pStat.DoStatValueBasicReset(Critical, pStatOwner);

		pStat = pStatOwner.IStatFind((int)ESHStatType.CriticalAnti);
		pStat.DoStatValueBasicReset(CriticalAnti, pStatOwner);

		pStat = pStatOwner.IStatFind((int)ESHStatType.CriticalDamage);
		pStat.DoStatValueBasicReset(CriticalDamage, pStatOwner);

		pStat = pStatOwner.IStatFind((int)ESHStatType.CriticalDamageAnti);
		pStat.DoStatValueBasicReset(CriticalDamageAnti, pStatOwner);

		pStat = pStatOwner.IStatFind((int)ESHStatType.Hit);
		pStat.DoStatValueBasicReset(Hit, pStatOwner);

		pStat = pStatOwner.IStatFind((int)ESHStatType.Dodge);
		pStat.DoStatValueBasicReset(Dodge, pStatOwner);

		pStat = pStatOwner.IStatFind((int)ESHStatType.Block);
		pStat.DoStatValueBasicReset(Block, pStatOwner);

		pStat = pStatOwner.IStatFind((int)ESHStatType.BlockAnti);
		pStat.DoStatValueBasicReset(BlockAnti, pStatOwner);

		pStat = pStatOwner.IStatFind((int)ESHStatType.RecoverPerSecond);
		pStat.DoStatValueBasicReset(RecoverPerSecond, pStatOwner);

		pStat = pStatOwner.IStatFind((int)ESHStatType.Stamina);
		pStat.DoStatValueBasicReset(Stamina, pStatOwner);

		pStat = pStatOwner.IStatFind((int)ESHStatType.AttackPercent);
		pStat.DoStatValueBasicReset(AttackPercent, pStatOwner);

		pStat = pStatOwner.IStatFind((int)ESHStatType.DefencePercent);
		pStat.DoStatValueBasicReset(DefencePercent, pStatOwner);

		pStat = pStatOwner.IStatFind((int)ESHStatType.StaminaPercent);
		pStat.DoStatValueBasicReset(StaminaPercent, pStatOwner);
	}
}

[System.Serializable]
public class SHStatLevel : CObjectInstanceBase //ToDo 암호화 할것
{
	[SerializeField]
	public uint LevelAttack = 0;
	[SerializeField]
	public uint LevelAttackSkill = 0;
	[SerializeField]
	public uint LevelDefence = 0;
	[SerializeField]
	public uint LevelDefenceSkill = 0;
	[SerializeField]
	public uint LevelCritical = 0;
	[SerializeField]
	public uint LevelCriticalAnit = 0;
	[SerializeField]
	public uint LevelCriticalDamage = 0;
	[SerializeField]
	public uint LevelCriticalDamageAnti = 0;
	[SerializeField]
	public uint LevelHit = 0;
	[SerializeField]
	public uint LevelDodge = 0;
	[SerializeField]
	public uint LevelBlock = 0;
	[SerializeField]
	public uint LevelBlockAnti = 0;
	[SerializeField]
	public uint LevelRecoverPerSecond = 0;
	[SerializeField]
	public uint LevelStamina = 0;
}

