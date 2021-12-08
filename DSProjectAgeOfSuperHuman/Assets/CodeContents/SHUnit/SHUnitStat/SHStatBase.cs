using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum ESHStatType  
{
	None,
	//-----------------------------------------1차스텟 ----------------------------------
	Attack,
	Defense,
	Stamina,

	AttackPercent,			// 공격력 배율 
	DefencePercent,			
	StaminaPercent,

	//-----------------------------------------1 차스텟 
	AttackSkill,	
	DefenseSkill,

	Critical,
	CriticalAnti,

	CriticalDamage,
	CriticalDamageAnti,

	Hit,
	Dodge,

	Block,
	BlockAnti,
	
	RecoverPerSecond, // 태그 off시 초당회복율  100 = 1% 최대체력에서 1%회복.  

	// 비 전투스텟
	ExtraGold,	
	ExtraEXP,	
	ExtraItem,

	//----------------------------------2차스텟 : 전투에서 계산되는 값. 1차스텟 기준으로 산출. 전투중 버프의 타겟
	AttackPower,
	DefensePower,
	MaxHitPoint,

	//------------------------2차 퍼센트
	AttackSkillRate,
	DefenseSkillRate,
	
	CriticalRate,
	CriticalRateAnti,

	CriticalDamageRate,
	CriticalDamageRateAnti,

	HitRate,
	DodgeRate,

	BlockRate,
	BlockAntiRate,

	AttackSpeed,    // 미 구현

	SkillDamagePercent,
	SkillReducePercent,
	DamagePercent,
	ReducePercent,

}


public abstract class SHStatBase : CStatBase, IStatModifierOwner
{
	//-------------------------------------------------------
	protected SHStatBase FindSHStat(ESHStatType eStatType)
	{
		return m_pStatOwner.IStatFind((uint)eStatType) as SHStatBase; 
	}
}


public class SHStatAttack : SHStatBase 
{
	public SHStatAttack() { m_hStatType = (uint)ESHStatType.Attack; }


}

public class SHStatAttackSkill : SHStatBase
{
	public SHStatAttackSkill() { m_hStatType = (uint)ESHStatType.AttackSkill; }

	protected override void OnStatValueRefresh(float fApplyValue)
	{
		SHStatBase pStat = FindSHStat(ESHStatType.AttackSkillRate);
		pStat.DoStatValueBasicReset(fApplyValue);
	}
}

public class SHStatDefence : SHStatBase
{
	public SHStatDefence() { m_hStatType = (uint)ESHStatType.Defense; }

}

public class SHStatDefenceSkill : SHStatBase
{
	public SHStatDefenceSkill() { m_hStatType = (uint)ESHStatType.DefenseSkill; }

	protected override void OnStatValueRefresh(float fApplyValue)
	{
		SHStatBase pStat = FindSHStat(ESHStatType.DefenseSkillRate);
		pStat.DoStatValueBasicReset(fApplyValue);
	}
}

public class SHStatCritical : SHStatBase
{
	public SHStatCritical() { m_hStatType = (uint)ESHStatType.Critical; }

	protected override void OnStatValueRefresh(float fApplyValue)
	{
		SHStatBase pStat = FindSHStat(ESHStatType.CriticalRate);
		pStat.DoStatValueBasicReset(fApplyValue);
	}
}

public class SHStatCriticalAnti : SHStatBase
{
	public SHStatCriticalAnti() { m_hStatType = (uint)ESHStatType.CriticalAnti; }

	protected override void OnStatValueRefresh(float fApplyValue)
	{
		SHStatBase pStat = FindSHStat(ESHStatType.CriticalRateAnti);
		pStat.DoStatValueBasicReset(fApplyValue);
	}
}

public class SHStatCriticalDamage : SHStatBase
{
	public SHStatCriticalDamage() { m_hStatType = (uint)ESHStatType.CriticalDamage; }

	protected override void OnStatValueRefresh(float fApplyValue)
	{
		SHStatBase pStat = FindSHStat(ESHStatType.CriticalDamageRate);
		pStat.DoStatValueBasicReset(fApplyValue);
	}
}

public class SHStatCriticalDamageAnti : SHStatBase
{
	public SHStatCriticalDamageAnti() { m_hStatType = (uint)ESHStatType.CriticalDamageAnti; }

	protected override void OnStatValueRefresh(float fApplyValue)
	{
		SHStatBase pStat = FindSHStat(ESHStatType.CriticalDamageRateAnti);
		pStat.DoStatValueBasicReset(fApplyValue);
	}
}

public class SHStatHit : SHStatBase
{
	public SHStatHit() { m_hStatType = (uint)ESHStatType.Hit; }

	protected override void OnStatValueRefresh(float fApplyValue)
	{
		SHStatBase pStat = FindSHStat(ESHStatType.HitRate);
		pStat.DoStatValueBasicReset(fApplyValue);
	}
}

public class SHStatDodge : SHStatBase
{
	public SHStatDodge() { m_hStatType = (uint)ESHStatType.Dodge; }

	protected override void OnStatValueRefresh(float fApplyValue)
	{
		SHStatBase pStat = FindSHStat(ESHStatType.DodgeRate);
		pStat.DoStatValueBasicReset(fApplyValue);
	}
}

public class SHStatBlock : SHStatBase
{
	public SHStatBlock() { m_hStatType = (uint)ESHStatType.Block; }

	protected override void OnStatValueRefresh(float fApplyValue)
	{
		SHStatBase pStat = FindSHStat(ESHStatType.BlockRate);
		pStat.DoStatValueBasicReset(fApplyValue);
	}
}

public class SHStatBlockAnti : SHStatBase
{
	public SHStatBlockAnti() { m_hStatType = (uint)ESHStatType.BlockAnti; }

	protected override void OnStatValueRefresh(float fApplyValue)
	{
		SHStatBase pStat = FindSHStat(ESHStatType.BlockAntiRate);
		pStat.DoStatValueBasicReset(fApplyValue);
	}
}

public class SHStatRecoverPerSecond : SHStatBase
{
	public SHStatRecoverPerSecond() { m_hStatType = (uint)ESHStatType.RecoverPerSecond; }

	protected override void OnStatValueRefresh(float fApplyValue)
	{
	}
}

public class SHStatStamina : SHStatBase
{
	public SHStatStamina() { m_hStatType = (uint)ESHStatType.Stamina; }

}

public class SHStatExtraGold : SHStatBase
{
	public SHStatExtraGold() { m_hStatType = (uint)ESHStatType.ExtraGold; }
}

public class SHStatExtraEXP : SHStatBase
{
	public SHStatExtraEXP() { m_hStatType = (uint)ESHStatType.ExtraEXP; }
}

public class SHStatExtraItem : SHStatBase
{
	public SHStatExtraItem() { m_hStatType = (uint)ESHStatType.ExtraItem; }
}

public class SHStatAttackPercent : SHStatBase
{
	public SHStatAttackPercent() { m_hStatType = (uint)ESHStatType.AttackPercent; }
}

public class SHStatDefencePercent : SHStatBase
{
	public SHStatDefencePercent() { m_hStatType = (uint)ESHStatType.DefencePercent; }
}

public class SHStatStaminaPercent : SHStatBase
{
	public SHStatStaminaPercent() { m_hStatType = (uint)ESHStatType.StaminaPercent; }
}


//------------------------------------------------------------------------------------
public class SHStatAttackPower : SHStatBase
{
	public SHStatAttackPower() { m_hStatType = (uint)ESHStatType.AttackPower; }

	protected override void OnStatValueChain(uint hStatType, float fApplyValue)
	{
		SHStatBase pAttack = FindSHStat(ESHStatType.Attack);
		SHStatBase pAttackPercent = FindSHStat(ESHStatType.AttackPercent);
		float fAttackPercent = (pAttackPercent.GetStatValue() / CStatComponentBase.c_GlobalChance);
		float fAttack = pAttack.GetStatValue();
		float fValue = fAttack + (fAttack * fAttackPercent);

		DoStatValueBasicReset(fValue);
	}
}

public class SHStatAttackSkillPower : SHStatBase
{
	public SHStatAttackSkillPower() { m_hStatType = (uint)ESHStatType.AttackSkillRate; }

	protected override void OnStatValueChain(uint hStatType, float fApplyValue)
	{
	}
}

public class SHStatDefencePower : SHStatBase
{
	public SHStatDefencePower() { m_hStatType = (uint)ESHStatType.DefensePower; }

	protected override void OnStatValueChain(uint hStatType, float fApplyValue)
	{
		SHStatBase pAttack = FindSHStat(ESHStatType.Defense);
		SHStatBase pAttackPercent = FindSHStat(ESHStatType.DefencePercent);
		float fPercent = (pAttackPercent.GetStatValue() / CStatComponentBase.c_GlobalChance);
		float fValue = pAttack.GetStatValue();
		float fPower = fValue + (fValue * fPercent);

		DoStatValueBasicReset(fPower);
	}
}

public class SHStatDefenceSkillPower : SHStatBase
{
	public SHStatDefenceSkillPower() { m_hStatType = (uint)ESHStatType.DefenseSkillRate; }

	protected override void OnStatValueChain(uint hStatType, float fApplyValue)
	{
	}
}


public class SHStatReducePercent : SHStatBase
{
	public SHStatReducePercent() { m_hStatType = (uint)ESHStatType.ReducePercent; }

	protected override void OnStatValueChain(uint hStatType, float fApplyValue)
	{
	}
}

public class SHStatDamagePercent : SHStatBase
{
	public SHStatDamagePercent() { m_hStatType = (uint)ESHStatType.DamagePercent; }

	protected override void OnStatValueChain(uint hStatType, float fApplyValue)
	{
	}
}

public class SHStatCriticalRate : SHStatBase
{
	public SHStatCriticalRate() { m_hStatType = (uint)ESHStatType.CriticalRate; }

	protected override void OnStatValueChain(uint hStatType, float fApplyValue)
	{
	}
}

public class SHStatCriticalRateAnti : SHStatBase
{
	public SHStatCriticalRateAnti() { m_hStatType = (uint)ESHStatType.CriticalRateAnti; }

	protected override void OnStatValueChain(uint hStatType, float fApplyValue)
	{
	}
}

public class SHStatCriticalDamageRate : SHStatBase
{
	public SHStatCriticalDamageRate() { m_hStatType = (uint)ESHStatType.CriticalDamageRate; }

	protected override void OnStatValueChain(uint hStatType, float fApplyValue)
	{
	}
}

public class SHStatCriticalDamageRateAnti : SHStatBase
{
	public SHStatCriticalDamageRateAnti() { m_hStatType = (uint)ESHStatType.CriticalDamageRateAnti; }

	protected override void OnStatValueChain(uint hStatType, float fApplyValue)
	{
	}
}

public class SHStatHitRate : SHStatBase
{
	public SHStatHitRate() { m_hStatType = (uint)ESHStatType.HitRate; }

	protected override void OnStatValueChain(uint hStatType, float fApplyValue)
	{
	}
}

public class SHStatDodgeRate : SHStatBase
{
	public SHStatDodgeRate() { m_hStatType = (int)ESHStatType.DodgeRate; }

	protected override void OnStatValueChain(uint hStatType, float fApplyValue)
	{
	}
}

public class SHStatAttackSpeed : SHStatBase
{
	public SHStatAttackSpeed() { m_hStatType = (uint)ESHStatType.AttackSpeed; }

	protected override void OnStatValueChain(uint hStatType, float fApplyValue)
	{
	}
}

public class SHStatMaxHitPoint : SHStatBase
{
	public SHStatMaxHitPoint() { m_hStatType = (uint)ESHStatType.MaxHitPoint; }

	protected override void OnStatValueChain(uint hStatType, float fApplyValue)
	{
		SHStatBase pAttack = FindSHStat(ESHStatType.Stamina);
		SHStatBase pAttackPercent = FindSHStat(ESHStatType.StaminaPercent);
		float fPercent = (pAttackPercent.GetStatValue() / CStatComponentBase.c_GlobalChance);
		float fValue = pAttack.GetStatValue();
		float fPower = fValue + (fValue * fPercent);

		DoStatValueBasicReset(fPower);
	}
}

public class SHStatSkillDamagePercent : SHStatBase
{
	public SHStatSkillDamagePercent() { m_hStatType = (uint)ESHStatType.SkillDamagePercent; }

	protected override void OnStatValueChain(uint hStatType, float fApplyValue)
	{
	}
}

public class SHStatSkillReducePercent : SHStatBase
{
	public SHStatSkillReducePercent() { m_hStatType = (uint)ESHStatType.SkillReducePercent; }

	protected override void OnStatValueChain(uint hStatType, float fApplyValue)
	{
	}
}

public class SHStatBlockRate : SHStatBase
{
	public SHStatBlockRate() { m_hStatType = (uint)ESHStatType.BlockRate; }

	protected override void OnStatValueChain(uint hStatType, float fApplyValue)
	{
	}
}

public class SHStatBlockAntiRate : SHStatBase
{
	public SHStatBlockAntiRate() { m_hStatType = (uint)ESHStatType.BlockAntiRate; }

	protected override void OnStatValueChain(uint hStatType, float fApplyValue)
	{
	}
}