using GameDB;
using UnityEngine;
public class UIQuestAutoPilot 
{
	private UIFrameQuest	mUIFrameQuest = null;
	private Quest_Table	mQuestTable = null;
	private bool			mWarpAutoPiolot = false;
	private bool			mAutoPiolot = false; public bool pAutoPilot { get { return mAutoPiolot; } }
	//--------------------------------------------------------------------
	public void InitializeAutoPilot(UIFrameQuest _uiFrameQuest)
	{
		mUIFrameQuest = _uiFrameQuest;
		ZPawnManager.Instance.DoAddEventCreateMyEntity(HandleAutoPilotWarpComplete);
		ZPawnManager.Instance.DoAddEventDieEntity(HandleAutoPilotDieMyPC);
	}

	public void RemoveAutoPilot()
	{
		ZPawnManager.Instance.DoRemoveEventCreateMyEntity(HandleAutoPilotWarpComplete);
		ZPawnManager.Instance.DoRemoveEventDieEntity(HandleAutoPilotDieMyPC);
	}

	//---------------------------------------------------------------------
	public void EventAutoPilotStop(uint _questID)
	{
		if (ReloadQuestTable(_questID) == null)
			return;

		StopAutoPilot(mQuestTable);
	}

	public void EventQuestShortCut(uint _questID)
	{
		if (ReloadQuestTable(_questID) == null)
			return;

		if (mQuestTable.UIShortCut == E_UIShortCut.EnchantUI)
		{
			mUIFrameQuest.Close();
		}
		else if (mQuestTable.UIShortCut == E_UIShortCut.BossGaugeUI)
		{
			mUIFrameQuest.Close();
		}
		else if (mQuestTable.UIShortCut == E_UIShortCut.InstanceDungeonUI)
		{
			mUIFrameQuest.Close();
		}
		else if (mQuestTable.UIShortCut == E_UIShortCut.RuneEnchantUI)
		{
			mUIFrameQuest.Close();
		}
		else if (mQuestTable.UIShortCut == E_UIShortCut.NormalShopUI)
		{
			mUIFrameQuest.Close();			
		}
		else if (mQuestTable.UIShortCut == E_UIShortCut.SpecialShopUI)
		{
			mUIFrameQuest.Close();
		}
		else
		{
			if (mQuestTable.UIShortCut == E_UIShortCut.Warp && mQuestTable.AutoProgressType == E_AutoProgressType.None)
			{
				PopupStageMove();
			}
			else
			{
				ShortCutAutoPilot(mQuestTable);
			}
		}
	}

	public void EventAutoPilotStart(uint _questID)
	{
		if (ReloadQuestTable(_questID) == null)
			return;

		ShortCutAutoPilot(mQuestTable);
	}

	public void EventAutoPilotStageMove(uint _stageID)
	{
		if (mWarpAutoPiolot == false && mQuestTable != null)
		{
			StopAutoPilot(mQuestTable);
		}
	}
	
	public void EventWarpAutoPilotStart(uint _questID)
	{
		if (ReloadQuestTable(_questID) == null)
			return;

		UIManager.Instance.Open<UIPopupStageMove>((Name, _popupStageMove) => {

			StopAutoPilot(mQuestTable);

			UIPopupStageMove.E_ChannelType channelType = UIPopupStageMove.E_ChannelType.Normal;

			if (ZGameModeManager.Instance.IsChaosChannel())
			{
				channelType = UIPopupStageMove.E_ChannelType.Chaos;
			}

			_popupStageMove.DoUIStageMove(channelType, mQuestTable.QuestWarpPortalID, (_canMove) => { 
			
				if (_canMove)
				{
	
					mWarpAutoPiolot = true;
				}
			});

		});
	}

	//-------------------------------------------------------------------------
	private void ShortCutAutoPilot(Quest_Table _questTable, bool _showErrorMessage = true)
	{
		if (_questTable.AutoProgressType == E_AutoProgressType.Auto)
		{
			uint stageID = ZGameModeManager.Instance.StageTid;
			if (stageID != _questTable.MapMoveID)
			{
				EventWarpAutoPilotStart(_questTable.QuestID);
			}
			else
			{
				StartAutoPilot(_questTable);
			}
		}
		else
		{
			// 실행불가 메시지 출력
			if (_showErrorMessage && _questTable.InfoMsg != null)
			{
				UICommon.SetNoticeMessage(DBLocale.GetText(_questTable.InfoMsg), new Color(255, 0, 116), 2f, UIMessageNoticeEnum.E_MessageType.BackNotice);
			}
		}
	}

	private void StartAutoPilot(Quest_Table _questTable)
	{
		ZPawnMyPc myPC = ZPawnManager.Instance.MyEntity;
		myPC.DoAddEventStopQuestAI(HandleAutoPilotStop);

		Vector3 position = new Vector3();
		position.x = _questTable.MapPos[0];
		position.y = _questTable.MapPos[1];
		position.z = _questTable.MapPos[2];

		if (_questTable.CompleteCheck == E_CompleteCheck.GetObject)
		{
			mAutoPiolot = true;
			myPC.StartAIForGathering(_questTable.MapMoveID, position, _questTable.ItemGetObjectID);
		}
		else if (_questTable.CompleteCheck == E_CompleteCheck.MonsterKill)
		{
			mAutoPiolot = true;
			myPC.StartAIForBattle(_questTable.MapMoveID, position, _questTable.TermsMonsterID);
		}
		else if (_questTable.CompleteCheck == E_CompleteCheck.NPCTalk)
		{
			mAutoPiolot = true;
			myPC.StartAIForTalkNpc(_questTable.TalkNPCMap, position, _questTable.TalkNPCID);
		}
		else if (_questTable.CompleteCheck == E_CompleteCheck.GetItem)
		{
			mAutoPiolot = true;
			myPC.StartAIForBattle(_questTable.MapMoveID, position, _questTable.ItemGetMonsterID);
		}
		else if (_questTable.CompleteCheck == E_CompleteCheck.MapPos || _questTable.CompleteCheck == E_CompleteCheck.ClearTemple)
		{
			mAutoPiolot = true;
			myPC.StartAIForMoveTo(_questTable.MapMoveID, position);
		}
		else if (_questTable.CompleteCheck == E_CompleteCheck.DeliveryItem)
		{
			if (mUIFrameQuest.CheckQuestCount(mQuestTable.QuestID))
			{
				mAutoPiolot = true;
				myPC.StartAIForTalkNpc(_questTable.TalkNPCMap, position, _questTable.TalkNPCID);
			}
			else
			{
				UICommon.SetNoticeMessage(DBLocale.GetText("아이템 수량이 모잘라 이동못함(로케일 할당필요)"), new Color(255, 0, 116), 2f, UIMessageNoticeEnum.E_MessageType.BackNotice);
			}
		}

		if (mAutoPiolot)
		{
			ShowAutoPilotMark(_questTable.QuestID, true);
		}
	}

	private void ShowAutoPilotMark(uint _questID, bool _start)
	{
		UIManager.Instance.Find<UISubHUDQuest>()?.DoSubHUDQuestAutoPilot(_start, _questID);
	}

	private Quest_Table ReloadQuestTable(uint _questID)
	{
		if (mQuestTable != null)
		{
			if (mQuestTable.QuestID != _questID)
			{
				StopAutoPilot(mQuestTable);
			}
		}

		mQuestTable = DBQuest.GetQuestData(_questID);
		return mQuestTable;
	}

	private void StopAutoPilot(Quest_Table _questTable)
	{
		ZPawnMyPc myPC = ZPawnManager.Instance.MyEntity;
		if (myPC == null) return;

		if (_questTable.CompleteCheck == E_CompleteCheck.GetObject)
		{
			myPC.StopAI(E_PawnAIType.QuestGathering);
		}
		else if (_questTable.CompleteCheck == E_CompleteCheck.MonsterKill)
		{
			myPC.StopAI(E_PawnAIType.QuestBattle);
		}
		else if (_questTable.CompleteCheck == E_CompleteCheck.NPCTalk || _questTable.CompleteCheck == E_CompleteCheck.DeliveryItem)
		{
			myPC.StopAI(E_PawnAIType.TalkNpc);
		}
		else if (_questTable.CompleteCheck == E_CompleteCheck.GetItem)
		{
			myPC.StopAI(E_PawnAIType.QuestBattle);
		}
		else if (_questTable.CompleteCheck == E_CompleteCheck.MapPos)
		{
			myPC.StopAI(E_PawnAIType.MoveTo);
		}
		mAutoPiolot = false;
		ShowAutoPilotMark(_questTable.QuestID, false);
	}

	private void PopupStageMove()
	{
		UIManager.Instance.Open<UIPopupStageMove>((Name, _popupStageMove) => {

			UIPopupStageMove.E_ChannelType channelType = UIPopupStageMove.E_ChannelType.Normal;
			if (ZGameModeManager.Instance.IsChaosChannel())
			{
				channelType = UIPopupStageMove.E_ChannelType.Chaos;
			}
			_popupStageMove.DoUIStageMove(channelType, mQuestTable.QuestWarpPortalID, (_canMove) => {

			});
		});
	}

	//-------------------------------------------------------------------
	private void HandleAutoPilotStop(E_PawnAIType _aiType)
	{
		if (_aiType == E_PawnAIType.QuestBattle || _aiType == E_PawnAIType.QuestGathering || _aiType == E_PawnAIType.MoveTo || _aiType == E_PawnAIType.TalkNpc)
		{
			ZPawnMyPc myPC = ZPawnManager.Instance.MyEntity;
			myPC.DoRemoveEventStopQuestAI(HandleAutoPilotStop);
			mAutoPiolot = false;
			ShowAutoPilotMark(mQuestTable.QuestID, false);
		}
	}

	private void HandleAutoPilotWarpComplete()
	{
		if (mWarpAutoPiolot)
		{
			mWarpAutoPiolot = false;
			ShortCutAutoPilot(mQuestTable, false);
		}
	}

	private void HandleAutoPilotDieMyPC(uint _entityID, ZPawn _diePawn)
	{
		if (_diePawn == ZPawnManager.Instance.MyEntity)
		{
			mWarpAutoPiolot = false;
			UIManager.Instance.Find<UISubHUDQuest>().DoSubHUDQuestAutoPilotAllOff();
			mUIFrameQuest.Close();
		}
	}
}
