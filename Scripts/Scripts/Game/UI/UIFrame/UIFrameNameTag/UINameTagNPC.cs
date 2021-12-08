using GameDB;
using UnityEngine;
using UnityEngine.UI;

public class UINameTagNPC : UINameTagBase
{
	[SerializeField] private Image NPCIcon = null;
	//----------------------------------------------
	protected override void OnNameTagInitialize(ZPawn _followPawn) 
	{
		base.OnNameTagInitialize(_followPawn);
		
	}

	protected override void OnNameTagRefreshTarget()
	{
		base.OnNameTagRefreshTarget();
		RefreshQuestTarget();
	}

	protected override void OnNameTagModelLoaded() 
	{
		base.OnNameTagModelLoaded();
		RefreshQuestHide(mFollowPawn as ZPawnNpc);
	}

	//----------------------------------------------------------------------------
	private void RefreshQuestTarget()
	{
		SetNameTagQuestNPCJob(ZGameModeManager.Instance.StageTid, mFollowPawn.TableId);	
	}

	private void RefreshQuestHide(ZPawnNpc _npcPawn)
	{
		if (_npcPawn == null) return;
		UIFrameQuest frameQuest = UIManager.Instance.Find<UIFrameQuest>();
		if (frameQuest)
		{
			if (frameQuest.QuestChecker.CheckNPCHide(ZGameModeManager.Instance.StageTid, _npcPawn.TableId))
			{
				_npcPawn.SetUseable(false);
				DoNameTagActive(false);
			}
		}
	}

	private void RefreshNPCIcon()
	{
		if (mFollowPawn == null)
		{
			ZLog.LogError(ZLogChannel.UI, string.Format("[NameTag] ===== RefreshNPCIcon "));
			return;
		}

		ZPawnNpc npcInstance = mFollowPawn as ZPawnNpc;

		if (string.IsNullOrEmpty(npcInstance.NpcData.TableData.Icon))
		{
			NPCIcon.gameObject.SetActive(false);
		}
		else
		{
			SetNameTagIcon(npcInstance.NpcData.TableData.Icon);
		}
	}


	private void SetNameTagIcon(string _iconName)
	{
		Sprite iconSprite = ZManagerUIPreset.Instance.GetSprite(_iconName);
		if (iconSprite)
		{
			NPCIcon.gameObject.SetActive(true);
			NPCIcon.sprite = iconSprite;
		}	
	}

	private void SetNameTagQuestNPCJob(uint _stageID, uint _npcID)
	{
		UIFrameQuest questFrame = UIManager.Instance.Find<UIFrameQuest>();
		if (questFrame.CheckQuestNPCAccept(_stageID, _npcID) || questFrame.CheckQuestNPCProgress(_stageID, _npcID))
		{			
			SetNameTagIcon("icon_hud_npc_aim_quest");
		}
		else if (questFrame.CheckQuestNPCReward(_stageID, _npcID))
		{
			SetNameTagIcon("icon_hud_npc_aim_questcomplete");
		}
		else
		{
			RefreshNPCIcon();
		}
	}

}
