using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SHManagerStageSpawner : CManagerStageSpawnerBase
{	public static new SHManagerStageSpawner Instance { get { return CManagerStageSpawnerBase.Instance as SHManagerStageSpawner; } }

	private bool m_bBossPopup = false;
	//-----------------------------------------------------------------------
	protected override void OnStageSpawnerEnd()
	{
		// 스테이지 클리어 이후 루프
		SHScriptTableStage.EStageType eStageType = SHManagerStage.Instance.GetMgrStageCurrent().GetStageType();
		if (eStageType == SHScriptTableStage.EStageType.Adventure)
		{
			
		}
		else if (eStageType == SHScriptTableStage.EStageType.Story) // 스테이지 아웃
		{
			 
		}
	}

	protected override void OnStageSpawnerStart()
	{
		base.OnStageSpawnerStart();
		m_bBossPopup = false;
	}

	//-----------------------------------------------------------------------
	public void DoMgrStageSpanwerSetHero(List<SHUnitHero> pListHero)
	{
		SortedDictionary<int, CStageSpawnerBase>.ValueCollection.Enumerator it = m_mapStageSpawnerInstance.Values.GetEnumerator();
		while (it.MoveNext())
		{
			SHStageSpawnerHero pSpanwHero = it.Current as SHStageSpawnerHero;
			if (pSpanwHero)
			{
				pSpanwHero.DoStageSpawnerHero(pListHero);
			}
		}
	}

	public void DoMgrStageSpawnerSetEnemy(List<SHUnitEnemy> pListEnemy, bool bBossFilter)
	{
		SortedDictionary<int, CStageSpawnerBase>.ValueCollection.Enumerator it = m_mapStageSpawnerInstance.Values.GetEnumerator();

		int iListCount = 0;
		while(it.MoveNext())
		{
			SHStageSpawnerEnemy pSpawnerEnermy = null;
			if (bBossFilter)
			{
				pSpawnerEnermy = it.Current as SHStageSpawnerEnemyBoss;
			}
			else
			{
				pSpawnerEnermy = it.Current as SHStageSpawnerEnemy;
			}

			if (pSpawnerEnermy != null)
			{
				int iSpotCount = pSpawnerEnermy.GetStageSpawnerSpotCount();
				int iInsertCount = iListCount + iSpotCount;

				List<CUnitBase> pListInsert = new List<CUnitBase>();
				for (int i = iListCount; i < iInsertCount; i++)
				{
					if (i < pListEnemy.Count)
					{
						pListInsert.Add(pListEnemy[i]);
					}
					else
					{ 
						pSpawnerEnermy.SetStageSpawnerEnemy(pListInsert, true);
						return;
					}
				}
				iListCount += iSpotCount;
				if (iListCount >= pListEnemy.Count)
				{
					pSpawnerEnermy.SetStageSpawnerEnemy(pListInsert, true);
					break;
				} 
				else
				{
					pSpawnerEnermy.SetStageSpawnerEnemy(pListInsert, false);
				}
			}
		}
	}

	public void DoMgrStageSapwnerBossPopup()
	{
		if (m_bBossPopup) return;
		SHUnitHero pHero = SHManagerUnit.Instance.GetUnitHero();
		if (pHero == null) return;

		m_bBossPopup = true;
		pHero.ISkillCancle();

		UIManager.Instance.DoUIMgrScreenIdle(false);		
		SHStageSpawnerEnemyBoss pSpawnBoss = FindSpawnBoss();
		if (pSpawnBoss)
		{
			pSpawnBoss.DoStageSpawnerBossStart();
		}
	}
	//------------------------------------------------------------------------------
	private SHStageSpawnerEnemyBoss FindSpawnBoss()
	{
		SHStageSpawnerEnemyBoss pFindBoss = null;
		SortedDictionary<int, CStageSpawnerBase>.ValueCollection.Enumerator it = m_mapStageSpawnerInstance.Values.GetEnumerator();

		while (it.MoveNext())
		{
			if (it.Current is SHStageSpawnerEnemyBoss)
			{
				pFindBoss = it.Current as SHStageSpawnerEnemyBoss;
				break;
			}
		}

		return pFindBoss;
	}
}
