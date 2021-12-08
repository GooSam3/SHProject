using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SHManagerUnit : CManagerUnitBase
{	public static new SHManagerUnit Instance { get { return CManagerUnitBase.Instance as SHManagerUnit; } }

	private SHUnitHero  m_pActionHero = null;
	private SHUnitEnemy m_pActionEnemy = null;
	private bool m_bTagHero = false;
	private bool m_bTagEnemy = false;

	private List<SHUnitHero> m_listUnitTeamHero = new List<SHUnitHero>();
	private List<SHUnitEnemy> m_listUnitTeamEnemy = new List<SHUnitEnemy>();
	//-------------------------------------------------------------------------
	protected override void OnMgrUnitClearAll()
	{
		base.OnMgrUnitClearAll();
		m_pActionHero = null;
		m_pActionEnemy = null;
		m_listUnitTeamHero.Clear();
		m_listUnitTeamEnemy.Clear();
		m_bTagEnemy = false;
		m_bTagHero = false;
	}

	protected override void OnUnitUpdate()
	{
		base.OnUnitUpdate();
		UpdateTagOffHeroUpdate();
	}

	//-----------------------------------------------------------------------
	public void DoMgrUnitRegist(SHUnitBase pUnit, bool bBoss = false)
	{		
		if (pUnit.GetUnitRelationForPlayer() == CUnitBase.EUnitRelationType.Hero)
		{
			if (m_listUnitTeamHero.Contains(pUnit as SHUnitHero) == false)
			{
				ProtMgrUnitRegist(pUnit);
				m_listUnitTeamHero.Add(pUnit as SHUnitHero);
			}
		}
		else if (pUnit.GetUnitRelationForPlayer() == CUnitBase.EUnitRelationType.Enemy)
		{
			if (bBoss)
			{
				PrivMgrUnitEnemyBossAdd(pUnit as SHUnitEnemy);
			}
			else
			{
				m_pActionEnemy = pUnit as SHUnitEnemy;
				m_pActionEnemy.DoUnitEnemySpawn(null);
			}
			ProtMgrUnitRegist(pUnit);
		}
	}

	public void DoMgrUnitDisableCurrent()
	{
		if (m_pActionEnemy != null)
		{
			m_pActionEnemy.DoUnitEnemyRemoveForce();
		}
		SHManagerStageSpawner.Instance.DoMgrStageSpawnerDeActive(m_pActionEnemy);
	}

	public void DoMgrUnitHeroTagOn(SHUnitHero pHero, bool bTagCutScene)
	{
		if (pHero == null) return; 
		if (pHero.GetUnitState() == CUnitBase.EUnitState.Remove) return;
		if (pHero.IsUnitSkillPlay()) return;
		
		if (m_bTagHero) return;
		m_bTagHero = true;


		float fCoolTime = SHManagerGameConfig.Instance.GetGameDBTagCoolTime();
		if (m_pActionHero != null)
		{
			m_pActionHero.DoUnitHeroTagOff(() => {
				PrivMgrUnitHeroTagOn(pHero);
			});
		}
		else
		{
			PrivMgrUnitHeroTagOn(pHero);
		}
		pHero.ISHSetTagCoolTime(fCoolTime);

		UIManager.Instance.DoUIMgrFind<SHUIFrameCombatHero>().DoUIFrameCombatTagOn(pHero, bTagCutScene);
	}

	public void DoMgrUnitHeroTagOnFirst()
	{
		uint hReaderID = SHManagerGameDB.Instance.GetGameDBHeroReaderID();
		SHUnitHero pReader = null;
		for (int i = 0; i < m_listUnitTeamHero.Count; i++)
		{
			if (m_listUnitTeamHero[i].GetUnitID() == hReaderID)
			{
				pReader = m_listUnitTeamHero[i];
				break;
			}
		}

		DoMgrUnitHeroTagOn(pReader, false);
	}

	public bool DoMgrUnitEnemyTagOn(SHUnitEnemy pEnemy, UnityAction delFinish)
	{
		if (m_bTagEnemy) return false;
		if (pEnemy.GetUnitState() != CUnitBase.EUnitState.PhaseOut) return false;
		m_bTagEnemy = true;
		if (m_pActionEnemy != null)
		{
			PrivMgrUnitEnemyTagOff(m_pActionEnemy, pEnemy.GetUnitEnemyTagPosition(), false, null);
		}
		PrivMgrUnitEnemyTagOn(pEnemy, ()=> {
			m_bTagEnemy = false;
		});
		return true;
	}

	public void DoMgrUnitUnRegist(SHUnitBase pUnit)
	{
		if (pUnit.GetUnitRelationForPlayer() == CUnitBase.EUnitRelationType.Hero)
		{
			PrivMgrUnitHeroNextTag(pUnit as SHUnitHero);
		}
		else if (pUnit is SHUnitEnemy)
		{
			PrivMgrUnitEnemyNextTag(pUnit as SHUnitEnemy);
		}

		ProtMgrUnitUnRegist(pUnit);
		
	}

	public bool IsUnitBossHasSkillPlay()
	{
		bool bSkillPlay = false;
		if (m_listUnitTeamEnemy.Count > 0)
		{
			bSkillPlay = m_pActionEnemy.IsUnitSkillPlay();
		}

		return bSkillPlay;
	}

	public bool IsUnitEnemyReady()
	{
		bool bReady = false;
		if (m_pActionEnemy != null)
		{
			if (m_pActionEnemy.IsAlive)
			{
				bReady = true;
			}
		}
		return bReady;
	}

	public void DoMgrUnitEnemyHideTagOffUnit(bool bShow)
	{
		for (int i = 0; i < m_listUnitTeamEnemy.Count; i++)
		{
			if (m_listUnitTeamEnemy[i] != m_pActionEnemy)
			{
				m_listUnitTeamEnemy[i].DoUnitShapeShowHide(bShow);
			}
		}
	}

	//------------------------------------------------------------------------
	public List<SHUnitEnemy> GetMgrUnitEnemyBossAll()
	{
		return m_listUnitTeamEnemy;
	}

	public List<SHUnitHero> GetMgrUnitHeroAll()
	{
		return m_listUnitTeamHero;
	}

	//---------------------------------------------------------------------------
	private void PrivMgrUnitHeroTagOn(SHUnitHero pHero)
	{
		m_bTagHero = false;
		m_pActionHero = pHero;
		UIManager.Instance.DoUIMgrFind<SHUIFrameCombatHero>()?.DoUIFrameCombatHeroSkill(m_pActionHero);
		m_pActionHero.DoUnitHeroTagOn();
	}

	private void PrivMgrUnitHeroNextTag(SHUnitHero pHero)
	{
		m_listUnitTeamHero.Remove(pHero);
		bool bAnnihilate = true;
		for (int i = 0; i < m_listUnitTeamHero.Count; i++)
		{
			SHUnitHero pStandByHero = m_listUnitTeamHero[i];
			if (pStandByHero.GetUnitState() == CUnitBase.EUnitState.StandBy || pStandByHero.GetUnitState() == CUnitBase.EUnitState.Exit)
			{
				DoMgrUnitHeroTagOn(pStandByHero, true);
				bAnnihilate = false;
				break;
			}
		}

		if (bAnnihilate)
		{
			pHero.DoUnitHeroTagOff(() => {
				PrivMgrUnitHeroAnnihilation();
			});
		}
	}

	private void PrivMgrUnitEnemyNextTag(SHUnitEnemy pDeathEnemy)
	{
		m_listUnitTeamEnemy.Remove(pDeathEnemy);

		if (m_listUnitTeamEnemy.Count > 0)
		{
			DoMgrUnitEnemyTagOn(m_listUnitTeamEnemy[0], null);
		}
	}

	private void PrivMgrUnitHeroAnnihilation()
	{
		Debug.LogWarning("[Unit] All Death");
	}

	private void UpdateTagOffHeroUpdate()
	{
		float fDelta = Time.deltaTime;
		for(int i = 0; i < m_listUnitTeamHero.Count; i++)
		{
			SHUnitHero pHero = m_listUnitTeamHero[i];
			if (pHero.IsAlive && pHero.GetUnitState() == CUnitBase.EUnitState.Exit)
			{
				pHero.DoUnitForceUpdate(fDelta);
				pHero.DoUnitRecoverHP(fDelta);
			}
		}
	}

	private void PrivMgrUnitEnemyBossAdd(SHUnitEnemy pBoss)
	{
		if (m_listUnitTeamEnemy.Count == 0)
		{
			pBoss.DoUnitEnemySpawn(()=> {
				PrivMgrUnitEnemyTagOn(pBoss, null);
			});
		}
		else
		{
			if (m_listUnitTeamEnemy.Count == 1)
			{
				pBoss.DoUnitEnemySpawn(()=> {
					pBoss.DoUnitEnemyPhaseOut(null);
				});
				PrivMgrUnitEnemyTagOff(pBoss, EUnitTagPosition.Left, false, null);
			}
			else
			{
				pBoss.DoUnitEnemySpawn(() =>
				{
					pBoss.DoUnitEnemyPhaseOut(null);
				});

				PrivMgrUnitEnemyTagOff(pBoss, EUnitTagPosition.Right, false, null);
			}
		}
		m_listUnitTeamEnemy.Add(pBoss);
	}

	private void PrivMgrUnitEnemyTagOn(SHUnitEnemy pEnemy, UnityAction delFinish)
	{
		pEnemy.DoUnitEnemyTagOn(delFinish);
		m_pActionEnemy = pEnemy;
		UIManager.Instance.DoUIMgrFind<SHUIFrameCombatHero>().DoUIFrameCombatEnemyGaugeReset(pEnemy);
	}

	private void PrivMgrUnitEnemyTagOff(SHUnitEnemy pEnemy, EUnitTagPosition eTargetTag, bool bForce, UnityAction delFinish)
	{
		pEnemy.DoUnitEnemyTagOff(eTargetTag, bForce, delFinish);
	}


	//---------------------------------------------------------------------------
	public SHUnitHero			GetUnitHero() { return m_pActionHero; }
	public SHUnitEnemy		GetUnitEnemy() { return m_pActionEnemy; }
	public List<SHUnitHero>	GetUnitHeroDeck() { return m_listUnitTeamHero; }
}
