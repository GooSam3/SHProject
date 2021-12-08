using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SHUnitHero : SHUnitBase
{
	[SerializeField]
	private uint UnitID = 0;
	protected SHUnitRageComboCount m_pUnitRageComboCount = null;
    //-------------------------------------------------------------------
    protected override void OnUnityAwake()
    {
        base.OnUnityAwake();
		SetUnitID(UnitID, "None");
	}

	protected override void OnUnitInitialize()
	{
		base.OnUnitInitialize();
		m_pUnitRageComboCount = GetComponentInChildren<SHUnitRageComboCount>();
		m_pUnitRageComboCount.InitializeUnitRageCombo(HandleUnitHeroComboReady, HandleUnitHeroComboEnd);
	}

	protected override void OnUnitExit(UnityEngine.Events.UnityAction delFinish)
	{
		base.OnUnitExit(delFinish);
		CAnimationBase.SAnimationUsage rAnimation = new CAnimationBase.SAnimationUsage();
		rAnimation.AnimName = "death";
		rAnimation.bLoop = false;
		rAnimation.fAniSpeed = 1f;
		rAnimation.fDuration = 0;
		ProtUnitAnimation(ref rAnimation, (AniName, bFinish) =>
		{
			m_pSHAnimationSpine.SetMonoActive(false);
			if (GetUnitHP().x <= 0)
			{
				ProtUnitRemove(false);
			}
			delFinish?.Invoke();
		}, null);

		DoUnitAIEnable(false);
	}

	protected override void OnUnitDeathStart()
	{
		base.OnUnitDeathStart();
		ProtUnitDeathEnd();
	}

	protected override void OnUnitDeathEnd()
	{
		base.OnUnitDeathEnd();
		SetMonoActive(true);
	}

	protected override void OnSHUnitApplyHeal(SDamageResult pDamageResult, EUnitSocket eUnitSocket, string strHitEffectName)
	{
		UIManager.Instance.DoUIMgrFind<SHUIFrameCombatHero>().DoUIFrameCombatHeroDamageHeal(pDamageResult);
	}

	protected override void OnSHUnitGainRage(float fRagePoint)
	{
		if(SHManagerUnit.Instance.IsUnitEnemyReady())
		{
			m_pUnitRageComboCount.DoUnitRageGain(fRagePoint);
		}
	}

	//-----------------------------------------------------------------
	public void DoUnitHeroSkillNormal(bool bLeft, SHUnitBase pTarget)
	{
		if (m_bAlive == false) return;

		if (m_pUnitRageComboCount.IsComboStart())
		{
			m_pUnitRageComboCount.DoUnitComboCountHit();
		}
		else
		{
			SHUnitSkillFSMHero pFSMHero = m_pSHSkillFSM as SHUnitSkillFSMHero;
			if (pFSMHero.DoUnitSkillNormal(bLeft, pTarget) == 0)
			{
				m_pUnitRageComboCount.DoUnitComboCountHit();
			}
		}
	}

	public void DoUnitHeroSkillActive(int iSlotIndex, SHUnitBase pTarget)
	{
		if (m_bAlive == false) return;
		if (pTarget.IsAlive == false) return;

		
		if (m_pSHSkillFSM.DoUnitSkillActive(iSlotIndex, pTarget) == 0)
		{
			m_pUnitRageComboCount.DoUnitComboCountReset();
		}
	}

	public void DoUnitHeroTagOn() // 유닛이 스테이지에 등장할때
	{
		m_eUnitTagPosition = EUnitTagPosition.Center;
		ProtUnitSpawned(null);
		m_pUnitRageComboCount.DoUnitComboCountResetAll();
	}

	public void DoUnitHeroTagOff(UnityEngine.Events.UnityAction delFinish)	// 유닛이 스테이지에서 퇴장할때 
	{
		m_eUnitTagPosition = EUnitTagPosition.None;
		ProtUnitAnimationIdle();
		m_pSHSkillFSM.DoUnitSkillReset();
		ProtUnitExit(delFinish);
	}

	public void SetUnitHeroCombatStat(NPacketData.SPacketStatValue pStat)
	{
		SHStatComponentHero pHeroStat = m_pSHStatComponent as SHStatComponentHero;
		SHStatGroupBasic pHeroUpgradeStat = new SHStatGroupBasic();
		pHeroUpgradeStat.Attack = pStat.LevelAttack;
		pHeroUpgradeStat.AttackSkill = pStat.LevelAttackSkill;
		pHeroUpgradeStat.Defence = pStat.LevelDefense;
		pHeroUpgradeStat.DefenceSkill = pStat.LevelDefenseSkill;
		pHeroUpgradeStat.Critical = pStat.LevelCritical;
		pHeroUpgradeStat.CriticalAnti = pStat.LevelCriticalAnti;
		pHeroUpgradeStat.CriticalDamage = pStat.LevelCriticalDamage;
		pHeroUpgradeStat.CriticalDamageAnti = pStat.LevelCriticalDamageAnti;
		pHeroUpgradeStat.Hit = pStat.LevelHit;
		pHeroUpgradeStat.Dodge = pStat.LevelDodge;
		pHeroUpgradeStat.RecoverPerSecond = pStat.LevelRecoverPerSecond;
		pHeroUpgradeStat.Stamina = pStat.LevelStamina;
		
		pHeroUpgradeStat.AttackPercent = pStat.LevelAttackPercent;
		pHeroUpgradeStat.DefencePercent = pStat.LevelDefensePercent;
		pHeroUpgradeStat.StaminaPercent = pStat.LevelStaminaPercent;

		pHeroStat.SetStatComponentUpgrade(pHeroUpgradeStat);
	}

	public float GetUnitHeroRage()
	{
		return m_pUnitRageComboCount.GetUnitHeroRage();
	}

	//------------------------------------------------------------------
	public void HandleUnitHeroComboReady()
	{
		if (IsAlive == false) return;
		SHUnitEnemy pEnemy = SHManagerUnit.Instance.GetUnitEnemy();
		if (pEnemy.IsAlive == false) return;
	
		SHUnitSkillFSMHero pFSMHero = m_pSHSkillFSM as SHUnitSkillFSMHero;
		if (pFSMHero.DoUnitHeroSkillCombo(pEnemy) == 0)
		{
			m_pUnitRageComboCount.DoUnitRageStart();
		}
	}

	public void HandleUnitHeroComboEnd()
	{
		m_pSHSkillFSM.DoUnitSkillReset();
		UIManager.Instance.DoUIMgrFind<SHUIFrameCombatHero>().DoUIFrameCombatEnemyGaugeDelay(0);
	}

	//-------------------------------------------------------------------
	public SHUnitHero()
	{
		m_eUnitControlType = EUnitControlType.PlayerControl;
		m_eUnitRelation = EUnitRelationType.Hero;
	}
}
