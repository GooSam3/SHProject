using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SHUIFrameCombatHero : SHUIFrameBase
{
	[SerializeField]
	private CText				EnemyName = null;
	[SerializeField]
	private SHUIGaugeEnemy		EnemyGauge = null;
	[SerializeField]
	private SHUIGaugeHeroFever HeroFever = null;
	[SerializeField]
	private SHUIButtonPunch	LeftPunch = null;
	[SerializeField]
	private SHUIButtonPunch	RightPunch = null;
	[SerializeField]
	private SHUIButtonSkillSlot SkillSlot1 = null;
	[SerializeField]
	private SHUIButtonSkillSlot SkillSlot2 = null;
	[SerializeField]
	private SHUIButtonSkillSlot SkillSlot3 = null;
	[SerializeField]
	private CText StageName = null;
	[SerializeField]
	private SHUIWidgetCombatMenuBar MenuBar = null;
	[SerializeField]
	private SHUIWidgetTagHeroCutScene TagCutScene = null;
	
	[SerializeField]
	private List<SHUIWidgetHeroTag> HeroTagSlot = null;

	private bool m_bHeroTagStart = false;				public bool GetCombatHeroTagStart() { return m_bHeroTagStart; }
	private SHUIWidgetHeroTag m_pCurrentHeroTagSlot = null;
	//----------------------------------------------------------------
	protected override void OnUIFrameInitialize()
	{
		base.OnUIFrameInitialize();
	}

	//---------------------------------------------------------------------
	public void DoUIFrameCombatEnemyGaugeReset(SHUnitBase pGaugeTarget)
	{
		EnemyGauge.DoUIGaugeTarget(pGaugeTarget);
		if (pGaugeTarget != null)
		{
			int iLevle = pGaugeTarget.GetUnitLevel();
			EnemyName.text = string.Format("{0} {1}", iLevle.ToString(), pGaugeTarget.GetUnitName());
		}
	}

	public void DoUIFrameCombatHeroSkill(SHUnitBase pHero)
	{
		int Count = 0;
		List<SHUnitSkillFSMBase.SSkillExportInfo> pListSkill = pHero.ExportUnitSkillInfo();
		for (int i = 0; i < pListSkill.Count; i++)
		{
			if (pListSkill[i].SkillType == ESkillType.SKillSlot)
			{
				if (Count == 0)
				{
					SkillSlot1.DoUIButtonSkillInfo(pHero, pListSkill[i].SkillID, pListSkill[i].CoolTimeName);
					Count++;
				}
				else if (Count == 1)
				{
					SkillSlot2.DoUIButtonSkillInfo(pHero, pListSkill[i].SkillID, pListSkill[i].CoolTimeName);
					Count++;
				}
				else if (Count == 2)
				{
					SkillSlot3.DoUIButtonSkillInfo(pHero, pListSkill[i].SkillID, pListSkill[i].CoolTimeName);
					Count++;
				}
			}
		}
	}

	public void DoUIFrameCombatHeroFocusSkillSlot(uint hSkillID)
	{
		SkillSlot1.DoUIButtonSkillFocus(hSkillID);
		SkillSlot2.DoUIButtonSkillFocus(hSkillID);
		SkillSlot3.DoUIButtonSkillFocus(hSkillID);
	}

	public void DoUIFrameCombatEnemyGaugeDelay(float fDelay)
	{
		EnemyGauge.DoUIGaugeDelay(fDelay);
	}

	public void DoUIFrameCombatHeroAttack(bool bLeft)
	{
		if (bLeft)
		{
			LeftPunch.DoButtonPunch();
		}
		else
		{
			RightPunch.DoButtonPunch();
		}
	}

	public void DoUIFrameCombatStageClear()
	{

	} 

	public void DoUIFrameCombatStageName(string strStageName)
	{
		StageName.text = strStageName;
	}

	public void DoUIFrameCombatStageHeroList(List<SHUnitHero> pListHero)
	{
		for (int i = 0; i < pListHero.Count; i++)
		{
			if (i == 0)
			{
				m_pCurrentHeroTagSlot = HeroTagSlot[0];
			}
			HeroTagSlot[i].DoHeroTagSetting(pListHero[i]);
		}
	}

	public void DoUIFrameCombatTagOn(SHUnitHero pHero, bool bTagCutScene)
	{
		HeroFever.DoUIGaugeHeroRage(pHero);
		PrivCombatHeroChangeHeroTag(pHero);
		if (bTagCutScene)
		{
			TagCutScene.DoTagHeroCutSceneStart(pHero);
		}
	}

	public void DoUIFrameCombatScreenIdle(CUIFrameBase pScreenFrame, bool bAttach)
	{
		if (bAttach)
		{
			InternalWidgetDelete(MenuBar);
			pScreenFrame.InternalWidgetAdd(MenuBar);
			HeroFever.DoUIGaugeHeroAutoPilot(true);
		}
		else
		{
			InternalWidgetAdd(MenuBar);
			pScreenFrame.InternalWidgetDelete(MenuBar);
			HeroFever.DoUIGaugeHeroAutoPilot(false);
		}
	}

	public void DoUIFrameCombatHeroDamageHeal(SDamageResult pDamageResult)
	{
		if (pDamageResult.eDamageType == NSkill.EDamageType.AttackNormal || pDamageResult.eDamageType == NSkill.EDamageType.AttackSkill)
		{
			for (int i = 0; i < HeroTagSlot.Count; i++)
			{
				SHUnitHero pHero = HeroTagSlot[i].GetHeroTagUnit();
				if (pHero == pDamageResult.pDefender)
				{
					HeroTagSlot[i].DoHeroDamageHeal(pDamageResult.fTotalValue, true, pDamageResult.eDamageResult == EDamageResult.Critical ? true : false);
				}
			}
		}
		else if (pDamageResult.eDamageType == NSkill.EDamageType.Heal)
		{
			for (int i = 0; i < HeroTagSlot.Count; i++)
			{
				SHUnitHero pHero = HeroTagSlot[i].GetHeroTagUnit();
				if (pHero == pDamageResult.pDefender)
				{
					HeroTagSlot[i].DoHeroDamageHeal(pDamageResult.fTotalValue, false, pDamageResult.eDamageResult == EDamageResult.Critical ? true : false);
				}
			}
		}
	}

	//------------------------------------------------------------
	private SHUIWidgetHeroTag FindCombatHeroTag(SHUnitHero pHero)
	{
		SHUIWidgetHeroTag pHeroTag = null;
		for (int i = 0; i < HeroTagSlot.Count; i++)
		{
			if (HeroTagSlot[i].GetHeroTagUnit() == pHero)
			{
				pHeroTag = HeroTagSlot[i];
				break;
			}
		}

		return pHeroTag;
	}

	private void PrivCombatHeroChangeHeroTag(SHUnitHero pHero)
	{
		SHUIWidgetHeroTag pHeroTag = HeroTagSlot[0];
		SHUnitHero pHeroOld = pHeroTag.GetHeroTagUnit();
		SHUIWidgetHeroTag pHeroTagOld = FindCombatHeroTag(pHero);
		pHeroTagOld.DoHeroTagSetting(pHeroOld);
		pHeroTag.DoHeroTagSetting(pHero);
		m_pCurrentHeroTagSlot = pHeroTag;
	}

	//---------------------------------------------------------------------
	public void HandleHeroAttackLeft()
	{
		SHUnitHero  pHero = SHManagerUnit.Instance.GetUnitHero();
		SHUnitEnemy pEnemy = SHManagerUnit.Instance.GetUnitEnemy();
		pHero?.DoUnitHeroSkillNormal(true, pEnemy);
		UIManager.Instance.DoUIMgrInputRefresh();
	}

    public void HandleHeroAttackRight()
	{
		SHUnitHero pHero = SHManagerUnit.Instance.GetUnitHero();
		SHUnitEnemy pEnemy = SHManagerUnit.Instance.GetUnitEnemy();
		pHero?.DoUnitHeroSkillNormal(false, pEnemy);
		UIManager.Instance.DoUIMgrInputRefresh();
	}

	public void HandleCombatMenu()
	{
		UIManager.Instance.DoUIMgrGotoLobby();
	}

	public void HandleUpgrade()
	{
		UIManager.Instance.DoUIMgrShow<SHUIFrameNavigationBar>().DoUINavigationTabHero();
	}

	//----------------------------------------------------------------
	public void HandleCombatSkillSlot1()
	{
		SHUnitHero pHero = SHManagerUnit.Instance.GetUnitHero();
		SHUnitEnemy pEnemy = SHManagerUnit.Instance.GetUnitEnemy();
		pHero?.DoUnitHeroSkillActive(0, pEnemy);
		UIManager.Instance.DoUIMgrInputRefresh();
	}

	public void HandleCombatSkillSlot2()
	{
		SHUnitHero pHero = SHManagerUnit.Instance.GetUnitHero();
		SHUnitEnemy pEnemy = SHManagerUnit.Instance.GetUnitEnemy();
		pHero?.DoUnitHeroSkillActive(1, pEnemy);
		UIManager.Instance.DoUIMgrInputRefresh();
	}

	public void HandleCombatSkillSlot3()
	{
		SHUnitHero pHero = SHManagerUnit.Instance.GetUnitHero();
		SHUnitEnemy pEnemy = SHManagerUnit.Instance.GetUnitEnemy();
		pHero?.DoUnitHeroSkillActive(2, pEnemy);
		UIManager.Instance.DoUIMgrInputRefresh();
	}

	//-------------------------------------------------------------------
	public void HandleCombatScreenIdle()
	{
		UIManager.Instance.DoUIMgrScreenIdle(true);
	}

	public void HandleCombatBossPopup()
	{
		if (SHManagerStage.Instance.GetMgrStageCurrent().IsStageMoveForward() == false)
		{
			SHManagerStageSpawner.Instance.DoMgrStageSapwnerBossPopup();
		}
	}
}
