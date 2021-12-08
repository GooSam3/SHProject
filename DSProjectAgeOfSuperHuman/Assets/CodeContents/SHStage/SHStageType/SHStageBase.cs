using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class SHStageBase : CStageBase
{
	[SerializeField]
	private uint StageTableSimulationID = 0;

	private string m_strStageName;	 public string GetStageName() { return m_strStageName; }
	private SHScriptTableStage.EStageType m_eStageType = SHScriptTableStage.EStageType.None; public SHScriptTableStage.EStageType GetStageType() { return m_eStageType; }
	//----------------------------------------------------------
	protected override void OnStageEnter(uint hStageID)
	{
		base.OnStageEnter(hStageID);
		SHManagerGameConfig.Instance.SetGameMode(SHManagerGameConfig.EGameModeType.Combat);
	}

	protected override void OnStageStart()
	{
		base.OnStageStart();
		UIManager.Instance.DoUIMgrHidePanelAll();
	}

	protected override void OnStageLoading(uint hStageID, UnityAction delFinish)
	{
		base.OnStageLoading(hStageID, delFinish);

		SHManagerStageSpawner.Instance.DoMgrStageSpawnerInitialize();

		if (hStageID != 0)
		{
			DoStageTableLoad(hStageID, delFinish);
		}
		else
		{
			if (StageTableSimulationID != 0)
			{
				DoStageTableLoadSimulation(StageTableSimulationID, delFinish);
			}
			else
			{
				delFinish?.Invoke();
			}
		}
	}

	protected override void OnStageEnd() // 보스 격파로 스테이지가 종료되었다.
	{
		SHManagerGameSession.Instance.RequestStageClear(pStageID, (uint hNextStageID) => {
			SHManagerStage.Instance.DoMgrStageEnter(hNextStageID);
		});
	}

	//------------------------------------------------------------------------
	public void DoStageTableLoad(uint hStageID, UnityAction delFinish)
	{
		SHScriptTableStage pTableStage = SHManagerScriptData.Instance.ExtractTableStage();
		SHScriptTableStage.SStageTable pStageInfo = pTableStage.FindTableStage(hStageID);

		m_strStageName = pStageInfo.StageName;
		m_eStageType = pTableStage.GetTableStageType(hStageID);
		UIManager.Instance.DoUIMgrFind<SHUIFrameCombatHero>().DoUIFrameCombatStageName(m_strStageName);

		PrivStageTablePrepareHero(() => {
			PrivStageTablePrepareEenemy(hStageID, pTableStage, delFinish);
		});
	}

	public void DoStageTableLoadSimulation(uint hStageID, UnityAction delFinish) // 영웅을 따로 로드하지 않는다. 로컬 프리팹 데이터로 전투한다.
	{
		SHScriptTableStage pTableStage = SHManagerScriptData.Instance.ExtractTableStage();
		SHScriptTableStage.SStageTable pStageInfo = pTableStage.FindTableStage(hStageID);

		m_strStageName = pStageInfo.StageName;
		m_eStageType = pTableStage.GetTableStageType(hStageID);
		UIManager.Instance.DoUIMgrFind<SHUIFrameCombatHero>().DoUIFrameCombatStageName(m_strStageName);

		PrivStageTablePrepareEenemy(hStageID, pTableStage, delFinish);
	}

	//-------------------------------------------------------------------------
	private void PrivStageTableLoadEnemy(List<SStageExportEnemy> pListTableEnemy, bool bBossFilter, UnityAction delFinish)
	{
		SHScriptTableDescriptionEnemy pTableEnemy = SHManagerScriptData.Instance.ExtractTableEnemy();

		List<SHUnitEnemy> pListLoadedEnemy = new List<SHUnitEnemy>();
		for (int i = 0; i < pListTableEnemy.Count; i++)
		{
			pTableEnemy.DoTableLoadEnemy(pListTableEnemy[i].EnmemyID, (int)pListTableEnemy[i].EnemyLevel, pListTableEnemy[i].EnemyGaugeScale, (SHUnitEnemy pEnemy, uint hEnemyID) =>
			{
				pListLoadedEnemy.Add(pEnemy);
				if (pListLoadedEnemy.Count == pListTableEnemy.Count)
				{
					SHManagerStageSpawner.Instance.DoMgrStageSpawnerSetEnemy(pListLoadedEnemy, bBossFilter);
					SHManagerEffect.Instance.DoMgrEffectPreLoadListStart(delFinish);
				}
			});
		}
	}

	private void PrivStageTableLoadHero(List<uint> pHeroList, UnityAction delFinish)
	{
		SHScriptTableDescriptionHero pTableHero = SHManagerScriptData.Instance.ExtractTableHero();
		List<SHUnitHero> pListLoadedHero = new List<SHUnitHero>();
		for(int i = 0; i < pHeroList.Count; i++)
		{
			pTableHero.DoTableLoadHero(pHeroList[i], (SHUnitHero pHero, uint hHeroID) => {
				pListLoadedHero.Add(pHero);
				SHScriptTableDescriptionHero.SDescriptionHero pTableDescHero = pTableHero.GetTableDescriptionHero(hHeroID);
				NPacketData.SPacketStatValue pStat = SHManagerGameDB.Instance.GetGameDBHeroStatCache(hHeroID);
				pHero.DoUnitIniailize();
				pHero.SetUnitHeroCombatStat(pStat);
				pHero.SetUnitInfo(hHeroID, pTableDescHero.UnitName, SHManagerGameDB.Instance.GetGameDBHeroLevel(hHeroID));				
				if (pHeroList.Count == pListLoadedHero.Count)
				{
					SHManagerStageSpawner.Instance.DoMgrStageSpanwerSetHero(pListLoadedHero);
					delFinish?.Invoke();
				}
			});
		}
	}

	private void PrivStageTablePrepareHero(UnityAction delFinish)
	{
		List<uint> pHeroList = SHManagerGameDB.Instance.GetGameDBHeroDeck();
		PrivStageTableLoadHero(pHeroList, delFinish);
	}

	private void PrivStageTablePrepareEenemy(uint hStageID, SHScriptTableStage pTableStage, UnityAction delFinish)
	{
		List<SStageExportEnemy> pListEnemy = pTableStage.GetTableStageEnemyList(hStageID, false);
		List<SStageExportEnemy> pListEnemyBoss = pTableStage.GetTableStageEnemyList(hStageID, true);

		PrivStageTableLoadEnemy(pListEnemy, false, () =>
		{
			PrivStageTableLoadEnemy(pListEnemyBoss, true, () =>
			{
				delFinish?.Invoke();
			});
		});
	}
}
