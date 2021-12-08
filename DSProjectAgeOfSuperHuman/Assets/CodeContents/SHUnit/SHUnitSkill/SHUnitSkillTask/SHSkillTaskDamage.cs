using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NSkill;

public class SHSkillTaskDamage : SHSkillTaskBase
{
	private EDamageType m_eDamageType = EDamageType.None;
	private EUnitSocket m_eUnitSocket = EUnitSocket.None;
	private uint m_hPropertyID;
	private string m_strSoundName;
	private string m_strHitEffectName;
	private float m_fDamageRate = 1f;
	private float m_fGaugeLockDelay = 0;

	//--------------------------------------------------------------------------
	public void SetTaskDamage(EDamageType eDamageType, EUnitSocket eUnitSocket, uint hPropertyID, float fDamageRate, float fGaugeLockDelay, string strSoundName, string strHitEffectName)
	{
		m_eDamageType = eDamageType;
		m_eUnitSocket = eUnitSocket;
		m_hPropertyID = hPropertyID;
		m_strSoundName = strSoundName;
		m_strHitEffectName = strHitEffectName;
		m_fDamageRate = fDamageRate;
		m_fGaugeLockDelay = fGaugeLockDelay;
	}
	
	protected override void OnSkillTaskUse(CStateSkillBase pOwnerState, CSkillUsage pSkillUsage, ISkillProcessor pSkillOwner, List<CUnitBase> pListTarget)
	{
		float fPower = ProtSkillTaskUpgradeValue(pSkillOwner.IGetUnit().GetUnitID(), m_hPropertyID);
		fPower *= m_fDamageRate;
		
		ISHSkillProcessor pSHSkillOwner = pSkillOwner as ISHSkillProcessor;
		for (int i = 0; i < pListTarget.Count; i++)
		{
			SHUnitBase pTarget = pListTarget[i] as SHUnitBase;
			pSHSkillOwner.ISHSkillAttackTo(pTarget, m_eDamageType, m_eUnitSocket, m_strHitEffectName, fPower);			
		}

		SHUnitHero pUnitHero = pSkillOwner.IGetUnit() as SHUnitHero;
		if (pUnitHero)
		{
			UIManager.Instance.DoUIMgrFind<SHUIFrameCombatHero>().DoUIFrameCombatEnemyGaugeDelay(m_fGaugeLockDelay);
		}
	}
}


public class SHSkillTaskDamageDecrescence : SHSkillTaskBase
{
	private EDamageType m_eDamageType = EDamageType.None;
	private EUnitSocket m_eUnitSocket = EUnitSocket.None;
	private uint m_hPropertyID;
	private string m_strSoundName;
	private float m_fDamageRatePrime = 0;
	private float m_fDamageRateSecond = 0;
	private float m_fDamageRateThird = 0;
	private float m_fGaugeLockDelay = 0;

	//--------------------------------------------------------------------------
	public void SetTaskDamage(EDamageType eDamageType, EUnitSocket eUnitSocket, uint hPropertyID, float fDamageRatePrime, float fDamageRateSecond, float fDamageRateThird, float fGaugeLockDelay, string strSoundName)
	{
		m_eDamageType = eDamageType;
		m_eUnitSocket = eUnitSocket;
		m_hPropertyID = hPropertyID;
		m_strSoundName = strSoundName;		
		m_fGaugeLockDelay = fGaugeLockDelay;
		m_fDamageRatePrime = fDamageRatePrime;
		m_fDamageRateSecond = fDamageRateSecond;
		m_fDamageRateThird = fDamageRateThird;
	}

	protected override void OnSkillTaskUse(CStateSkillBase pOwnerState, CSkillUsage pSkillUsage, ISkillProcessor pSkillOwner, List<CUnitBase> pListTarget)
	{
		float fStatPower = ProtSkillTaskUpgradeValue(pSkillOwner.IGetUnit().GetUnitID(), m_hPropertyID);
		ISHSkillProcessor pSHSkillOwner = pSkillOwner as ISHSkillProcessor;
		for (int i = 0; i < pListTarget.Count; i++)
		{
			SHUnitBase pTarget = pListTarget[i] as SHUnitBase;
			float fDamageRate = 1f;
			if (pTarget == pSkillUsage.UsageTarget.IGetUnit() || pTarget == pSkillOwner.IGetUnit())
			{
				fDamageRate = m_fDamageRatePrime;
			}
			else
			{
				fDamageRate = m_fDamageRateSecond;
			}  
			
			float fPower = fStatPower * fDamageRate;						
			pSHSkillOwner.ISHSkillAttackTo(pTarget, m_eDamageType, m_eUnitSocket, string.Empty, fPower);
		}

		SHUnitHero pUnitHero = pSkillOwner.IGetUnit() as SHUnitHero;
		if (pUnitHero)
		{
			UIManager.Instance.DoUIMgrFind<SHUIFrameCombatHero>().DoUIFrameCombatEnemyGaugeDelay(m_fGaugeLockDelay);
		}
	}
}