using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SHScriptTableStage : CScriptDataTableBase
{
	public enum EEnterCondtion
	{
		None,
		Scenaroi,
		ClearStage,
	}

	public enum EStageType
	{
		None,
		Adventure,
		Story,
	}

	public class SStageTable
	{
		public uint	StageID = 0;
		public uint	WorldIndex = 0;
		public string SceneName;
		public string StageName;
		public uint	StageIndex = 0;
		public uint	EnterLevel = 0;
		public EEnterCondtion EEnterCondtion = EEnterCondtion.None;
		public int	EnterConditionValue = 0;
		public List<uint> StagePrimeDropItem = new List<uint>();
		public uint	StageClearRewardID = 0;
		public uint  NextStageID = 0;
		public EStageType EStageType = EStageType.None;
		public uint StageDialogID = 0;
		public uint EnemyID1 = 0;
		public uint Level1 = 0;
		public uint HPScale1 = 0;
		public bool IsBoss1 = false;
		public uint PackageRewardID1 = 0; 
		public uint EnemyID2 = 0;
		public uint Level2 = 0;
		public uint HPScale2 = 0;
		public bool IsBoss2 = false;
		public uint PackageRewardID2 = 0;
		public uint EnemyID3 = 0;
		public uint Level3 = 0;
		public uint HPScale3 = 0;
		public bool IsBoss3 = false;
		public uint PackageRewardID3 = 0;
		public uint EnemyID4 = 0;
		public uint Level4 = 0;
		public uint HPScale4 = 0;
		public bool IsBoss4 = false;
		public uint PackageRewardID4 = 0;
		public uint EnemyID5 = 0;
		public uint Level5 = 0;
		public uint HPScale5 = 0;
		public bool IsBoss5 = false;
		public uint PackageRewardID5 = 0;
		public uint EnemyID6 = 0;
		public uint Level6 = 0;
		public uint HPScale6 = 0;
		public bool IsBoss6 = false;
		public uint PackageRewardID6 = 0;
		public uint EnemyID7 = 0;
		public uint Level7 = 0;
		public uint HPScale7 = 0;
		public bool IsBoss7 = false;
		public uint PackageRewardID7 = 0;
		public uint EnemyID8 = 0;
		public uint Level8 = 0;
		public uint HPScale8 = 0;
		public bool IsBoss8 = false;
		public uint PackageRewardID8 = 0;
		public uint EnemyID9 = 0;
		public uint Level9 = 0;
		public uint HPScale9 = 0;
		public bool IsBoss9 = false;
		public uint PackageRewardID9 = 0;
		public uint EnemyID10 = 0;
		public uint Level10 = 0;
		public uint HPScale10 = 0;
		public bool IsBoss10 = false;
		public uint PackageRewardID10 = 0;
	}

	private Dictionary<uint, SStageTable>			m_mapStageTable = new Dictionary<uint, SStageTable>();
	private CMultiSortedDictionary<uint, SStageTable>	m_mapStageTableWorldArrange = new CMultiSortedDictionary<uint, SStageTable>();
	//---------------------------------------------------------------
	protected override void OnScriptDataInitialize(string strTextData)
	{
		base.OnScriptDataInitialize(strTextData);
		PrivScriptTableLoad();
	}
	//----------------------------------------------------------------
	public EStageType GetTableStageType(uint hStageID)
	{
		EStageType eStageType = EStageType.None;
		SStageTable pStageTable = FindTableStage(hStageID);
		if (pStageTable != null)
		{
			eStageType = pStageTable.EStageType;
		}
		return eStageType;
	}

	public SStageTable FindTableStage(uint hStageID)
	{
		SStageTable pStageTable = null;
		if (m_mapStageTable.ContainsKey(hStageID))
		{
			pStageTable = m_mapStageTable[hStageID];
		}
		return pStageTable;
	}

	public SStageTable FindTableStageNext(uint hStageID)
	{
		SStageTable pFindStageTable = null;
		if (m_mapStageTable.ContainsKey(hStageID))
		{
			SStageTable pStageTable = m_mapStageTable[hStageID];
			if (m_mapStageTable.ContainsKey(pStageTable.NextStageID))
			{
				pFindStageTable = m_mapStageTable[pStageTable.NextStageID];
			}
		}
		return pFindStageTable;
	}

	public SStageTable FindTableStageByClearCount(uint iWorldIndex, uint iClearCount)
	{
		SStageTable pStageTable = null;
		if (m_mapStageTableWorldArrange.ContainsKey(iWorldIndex))
		{
			List<SStageTable> pList = m_mapStageTableWorldArrange[iWorldIndex];
			for (int i = 0; i < pList.Count; i++)
			{
				if(pList[i].StageIndex == iClearCount)
				{
					pStageTable = pList[i];
					break;
				}
			}
		}
		return pStageTable;
	}


	public List<SStageExportEnemy> GetTableStageEnemyList(uint hStageID, bool bBossFilter)
	{
		List<SStageExportEnemy> pListEnemy = new List<SStageExportEnemy>();
		SStageTable pStageTable = FindTableStage(hStageID);
		if (pStageTable != null)
		{
			if (pStageTable.EnemyID1 != 0 && bBossFilter == pStageTable.IsBoss1)
			{
				SStageExportEnemy rEnemy = new SStageExportEnemy();
				rEnemy.EnmemyID = pStageTable.EnemyID1;
				rEnemy.EnemyLevel = pStageTable.Level1;
				rEnemy.EnemyGaugeScale = pStageTable.HPScale1;
				pListEnemy.Add(rEnemy);
			}

			if (pStageTable.EnemyID2 != 0 && bBossFilter == pStageTable.IsBoss2)
			{
				SStageExportEnemy rEnemy = new SStageExportEnemy();
				rEnemy.EnmemyID = pStageTable.EnemyID2;
				rEnemy.EnemyLevel = pStageTable.Level2;
				rEnemy.EnemyGaugeScale = pStageTable.HPScale2;
				pListEnemy.Add(rEnemy);
			}

			if (pStageTable.EnemyID3 != 0 && bBossFilter == pStageTable.IsBoss3)
			{
				SStageExportEnemy rEnemy = new SStageExportEnemy();
				rEnemy.EnmemyID = pStageTable.EnemyID3;
				rEnemy.EnemyLevel = pStageTable.Level3;
				rEnemy.EnemyGaugeScale = pStageTable.HPScale3;
				pListEnemy.Add(rEnemy);
			}

			if (pStageTable.EnemyID4 != 0 && bBossFilter == pStageTable.IsBoss4)
			{
				SStageExportEnemy rEnemy = new SStageExportEnemy();
				rEnemy.EnmemyID = pStageTable.EnemyID4;
				rEnemy.EnemyLevel = pStageTable.Level4;
				rEnemy.EnemyGaugeScale = pStageTable.HPScale4;
				pListEnemy.Add(rEnemy);
			}

			if (pStageTable.EnemyID5 != 0 && bBossFilter == pStageTable.IsBoss5)
			{
				SStageExportEnemy rEnemy = new SStageExportEnemy();
				rEnemy.EnmemyID = pStageTable.EnemyID5;
				rEnemy.EnemyLevel = pStageTable.Level5;
				rEnemy.EnemyGaugeScale = pStageTable.HPScale5;
				pListEnemy.Add(rEnemy);
			}

			if (pStageTable.EnemyID6 != 0 && bBossFilter == pStageTable.IsBoss6)
			{
				SStageExportEnemy rEnemy = new SStageExportEnemy();
				rEnemy.EnmemyID = pStageTable.EnemyID6;
				rEnemy.EnemyLevel = pStageTable.Level6;
				rEnemy.EnemyGaugeScale = pStageTable.HPScale6;
				pListEnemy.Add(rEnemy);
			}

			if (pStageTable.EnemyID7 != 0 && bBossFilter == pStageTable.IsBoss7)
			{
				SStageExportEnemy rEnemy = new SStageExportEnemy();
				rEnemy.EnmemyID = pStageTable.EnemyID7;
				rEnemy.EnemyLevel = pStageTable.Level7;
				rEnemy.EnemyGaugeScale = pStageTable.HPScale7;
				pListEnemy.Add(rEnemy);
			}

			if (pStageTable.EnemyID8 != 0 && bBossFilter == pStageTable.IsBoss8)
			{
				SStageExportEnemy rEnemy = new SStageExportEnemy();
				rEnemy.EnmemyID = pStageTable.EnemyID8;
				rEnemy.EnemyLevel = pStageTable.Level8;
				rEnemy.EnemyGaugeScale = pStageTable.HPScale8;
				pListEnemy.Add(rEnemy);
			}

			if (pStageTable.EnemyID9 != 0 && bBossFilter == pStageTable.IsBoss9)
			{
				SStageExportEnemy rEnemy = new SStageExportEnemy();
				rEnemy.EnmemyID = pStageTable.EnemyID9;
				rEnemy.EnemyLevel = pStageTable.Level9;
				rEnemy.EnemyGaugeScale = pStageTable.HPScale9;
				pListEnemy.Add(rEnemy);
			}

			if (pStageTable.EnemyID10 != 0 && bBossFilter == pStageTable.IsBoss10)
			{
				SStageExportEnemy rEnemy = new SStageExportEnemy();
				rEnemy.EnmemyID = pStageTable.EnemyID10;
				rEnemy.EnemyLevel = pStageTable.Level10;
				rEnemy.EnemyGaugeScale = pStageTable.HPScale10;
				pListEnemy.Add(rEnemy);
			}
		}
		return pListEnemy;
	}

	public uint GetTableStageEnemyRewardPackageID(uint hStageID, uint hEnemyID)
	{
		uint iResult = 0;
		SStageTable pStageTable = FindTableStage(hStageID);
		if (pStageTable == null) return 0;

		if (pStageTable.EnemyID1 == hEnemyID)
		{
			iResult = pStageTable.PackageRewardID1;
		}
		else if (pStageTable.EnemyID2 == hEnemyID)
		{
			iResult = pStageTable.PackageRewardID2;
		}
		else if (pStageTable.EnemyID3 == hEnemyID)
		{
			iResult = pStageTable.PackageRewardID3;
		}
		else if (pStageTable.EnemyID4 == hEnemyID)
		{
			iResult = pStageTable.PackageRewardID4;
		}
		else if (pStageTable.EnemyID5 == hEnemyID)
		{
			iResult = pStageTable.PackageRewardID5;
		}
		else if (pStageTable.EnemyID6 == hEnemyID)
		{
			iResult = pStageTable.PackageRewardID6;
		}
		else if (pStageTable.EnemyID7 == hEnemyID)
		{
			iResult = pStageTable.PackageRewardID7;
		}
		else if (pStageTable.EnemyID8 == hEnemyID)
		{
			iResult = pStageTable.PackageRewardID8;
		}
		else if (pStageTable.EnemyID9 == hEnemyID)
		{
			iResult = pStageTable.PackageRewardID9;
		}
		else if (pStageTable.EnemyID10 == hEnemyID)
		{
			iResult = pStageTable.PackageRewardID10;
		}
		return iResult;
	}

	//----------------------------------------------------------------
	private void PrivScriptTableLoad()
	{
		List<SStageTable> pListTable = ProtDataTableRead<SStageTable>();

		for (int i = 0; i < pListTable.Count; i++)
		{
			SStageTable pStageTable = pListTable[i];
			m_mapStageTable[pStageTable.StageID] = pStageTable;

			m_mapStageTableWorldArrange.Add(pStageTable.WorldIndex, pStageTable);
		}  
	}

	
	
}
