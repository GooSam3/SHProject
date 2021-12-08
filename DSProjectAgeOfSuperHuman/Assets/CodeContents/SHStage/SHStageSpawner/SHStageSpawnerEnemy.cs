using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class SHStageSpawnerEnemy : CStageSpawnerSequenceBase
{
    [SerializeField]
    private uint DialogID = 0;

    [SerializeField]
    private Sprite BackGround = null;

	protected bool m_bSpawnEnd = false;
	//---------------------------------------------------------------
	protected override void OnSpawnerNextSpawn()
	{
		base.OnSpawnerNextSpawn();

		ProtStageSpawnerMoveForward(() =>
		{
			if (m_bSpawnEnd)
			{
				SHManagerStageSpawner.Instance.DoMgrStageSpawnerStart(true);
			}
		});
	}

	//-------------------------------------------------------------
	public void SetStageSpawnerTableInfo(Sprite pSpriteBackGround, uint hDialogID)
	{
		BackGround = pSpriteBackGround;
		DialogID = hDialogID;
	}

	public void SetStageSpawnerEnemy(List<CUnitBase> pListEnemy, bool bSpawnEnd)
	{
		ProtStageSpawnerSetUnit(pListEnemy);
		m_bSpawnEnd = bSpawnEnd;
	}

	//--------------------------------------------------------------
	private void PrivStageSpawnerDialogStart(uint hDialogID)
	{

	}

	protected void ProtStageSpawnerMoveForward(UnityAction delFinish)
	{
		if (BackGround)
		{
			SHStageCombatSceneNormal pBattleScene = SHManagerStage.Instance.GetMgrStageCurrent();
			if (pBattleScene)
			{
				pBattleScene.DoStageMoveForward(BackGround, delFinish);
			}
		}
	}

	//---------------------------------------------------------------

}
