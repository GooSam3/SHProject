using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SHManagerStage : CManagerStageBase
{ public static new SHManagerStage Instance { get { return CManagerStageBase.Instance as SHManagerStage; } }

	private uint m_hStageID = 0;

	//---------------------------------------------------------------
	public void DoMgrStageEnter(uint hStageID)
	{
		SHScriptTableStage.SStageTable pTable = SHManagerScriptData.Instance.ExtractTableStage().FindTableStage(hStageID);
		if (pTable != null)
		{
			m_hStageID = hStageID;
			PrivMgrStageEnter(pTable);
		}
		else
		{
			Debug.LogError("[Stage Load] Invalid ID " + hStageID);
		}
	}

	public void DoMgrStageEnemyReward(uint hEnemyID)
	{
		SHManagerGameSession.Instance.RequestStageRewardEnemy(m_hStageID, hEnemyID);
	}

	//---------------------------------------------------------------
	private void PrivMgrStageEnter(SHScriptTableStage.SStageTable pTable)
	{
		if (SHManagerGameDB.Instance.CheckGameDBStageEnter(pTable.WorldIndex, pTable.StageIndex))
		{
			UIManager.Instance.DoUIMgrSceneLoadingStart(pTable.SceneName, ()=> {
				ProtStageEnter(pTable.StageID);
			});
		}
		else
		{	//Error!
			
		}
	}

	public SHStageCombatSceneNormal GetMgrStageCurrent() { return GetStageCurrent() as SHStageCombatSceneNormal; }
	public uint GetMgrStageCurrentID() { return m_hStageID; }

}
