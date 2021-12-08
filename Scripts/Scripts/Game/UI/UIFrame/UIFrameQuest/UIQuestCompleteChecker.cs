using GameDB;
using System.Collections.Generic;
using UnityEngine;
using ZDefine;

public class UIQuestCompleteChecker
{
	public class SQuestCompletCondition
	{
		public uint QuestTID = 0;
		public Quest_Table QuestTable = null;
		public uint TargetCount = 0;
	}

	public class SQuestNPCShowInfo
	{
		public uint StageID = 0;
		public uint NPCID = 0;
		public bool Show = false;
	}

	private UIFrameQuest mUIFrameQuest = null;
	private bool mUpdate = false;

	private List<SQuestNPCShowInfo> m_listNPCShowInfo = new List<SQuestNPCShowInfo>();
	private List<SQuestCompletCondition> m_listConditionUpdate = new List<SQuestCompletCondition>();
	private Dictionary<uint, SQuestCompletCondition> m_mapConditionIntance = new Dictionary<uint, SQuestCompletCondition>();
	//------------------------------------------------------
	public void InitializeQuestChecker(UIFrameQuest _uiFrameQuest) { mUIFrameQuest = _uiFrameQuest; }

	public void DoAddCondition(Quest_Table _questTable, uint _targetCount)
	{
		if (m_mapConditionIntance.ContainsKey(_questTable.QuestID)) return;
		
		SQuestCompletCondition newCondition = new SQuestCompletCondition();
		newCondition.QuestTID = _questTable.QuestID;
		newCondition.QuestTable = _questTable;
		newCondition.TargetCount = _targetCount;
		
		m_mapConditionIntance[_questTable.QuestID] = newCondition;

		if (_questTable.CompleteCheck == E_CompleteCheck.MapPos)
		{
			m_listConditionUpdate.Add(newCondition);
		}
		else if (_questTable.CompleteCheck == E_CompleteCheck.DeliveryItem)
		{
			m_listConditionUpdate.Add(newCondition);
		}
	}

	public void DoAddNPCShowHide(uint _stageID, uint _npcID, bool _show)
	{
		if (_stageID == 0 || _npcID == 0) return;

		SQuestNPCShowInfo npcShow = m_listNPCShowInfo.Find(item => (item.NPCID == _npcID) && (item.StageID == _stageID));
		if (npcShow == null)
		{
			npcShow = new SQuestNPCShowInfo();
			npcShow.NPCID = _npcID;
			npcShow.Show = _show;
			npcShow.StageID = _stageID;
			m_listNPCShowInfo.Add(npcShow);

			ZPawnNpc npcEntity = ZPawnManager.Instance.FindEntityByTid(_npcID, E_UnitType.NPC) as ZPawnNpc;
			if (npcEntity)
			{
				npcEntity.SetUseable(false);
			}
		}
	}

	public bool CheckNPCHide(uint _stageID, uint _npcID)
	{
		bool hideNPC = false;
		SQuestNPCShowInfo npcShow = m_listNPCShowInfo.Find(item => (item.NPCID == _npcID) && (item.StageID == _stageID));
		if (npcShow != null)
		{
			if (npcShow.Show == false)
			{
				hideNPC = true;
			}
		}

		return hideNPC;
	}

	//-----------------------------------------------------------------------------------------------
	public void EventStageMove(uint _stageTID)
	{
		mUpdate = true;
		Dictionary<uint, SQuestCompletCondition>.Enumerator it = m_mapConditionIntance.GetEnumerator();
		while(it.MoveNext())
		{
			SQuestCompletCondition condition = it.Current.Value;
			if (condition.QuestTable.CompleteCheck == E_CompleteCheck.MapMove)
			{
				if (condition.QuestTable.MapMoveID == _stageTID)
				{
					QuestReward(condition);
					it = DeleteCondition(condition);
				}
			}
			else if (condition.QuestTable.CompleteCheck == E_CompleteCheck.ClearTemple)
			{
				if (ZGameModeManager.Instance.CurrentGameModeType == E_GameModeType.Field)
				{
					QuestReward(condition);
					it = DeleteCondition(condition);
				}
			}
		}
		mUIFrameQuest.DoUIQuestRefreshOtherTarget();
		mUIFrameQuest.QuestAutoPilot.EventAutoPilotStageMove(_stageTID);
	}


	public bool EventNPCTalk(uint _stageTID, uint _npcTID)
	{
		Dictionary<uint, SQuestCompletCondition>.Enumerator it = m_mapConditionIntance.GetEnumerator();
		while (it.MoveNext())
		{
			SQuestCompletCondition condition = it.Current.Value;
			if (condition.QuestTable.CompleteCheck == E_CompleteCheck.NPCTalk)
			{
				if (condition.QuestTable.TalkNPCMap == _stageTID && condition.QuestTable.TalkNPCID == _npcTID)
				{
					UIManager.Instance.Show<UIFrameQuest>().DoUIQuestDialog(condition.QuestTID, UIQuestDialog.E_DialogType.Reward);
					it = DeleteCondition(condition);
					return true;
				}
			}
			else if (condition.QuestTable.CompleteCheck == E_CompleteCheck.DeliveryItem)
			{
				if (mUIFrameQuest.CheckQuestCount(condition.QuestTID))
				{
					if (condition.QuestTable.TalkNPCMap == _stageTID && condition.QuestTable.TalkNPCID == _npcTID)
					{
						UIManager.Instance.Show<UIFrameQuest>().DoUIQuestDialog(condition.QuestTID, UIQuestDialog.E_DialogType.Reward);
						it = DeleteCondition(condition);
						return true;
					}
				}
			}
		}

		return false;
	}
 

	public void EventInteraction(uint _objectTID)
	{

	}

	public void EventUpdateCondition()
	{
		for (int i = 0; i < m_listConditionUpdate.Count; i++)
		{
			SQuestCompletCondition condition = m_listConditionUpdate[i];
			if (condition.QuestTable.CompleteCheck == E_CompleteCheck.MapPos && condition.QuestTable.MapMoveID == ZGameModeManager.Instance.StageTid)
			{
				if (ZPawnManager.Instance.MyEntity)
				{
					Vector3 charPosition = ZPawnManager.Instance.MyEntity.Position;
					Vector3 mapPosition = Vector3.zero;
					mapPosition.x = condition.QuestTable.MapPos[0];
					mapPosition.y = condition.QuestTable.MapPos[1];
					mapPosition.z = condition.QuestTable.MapPos[2];

					Vector3 distance = mapPosition - charPosition;
					float length = distance.magnitude;
					if (length <= condition.QuestTable.MapPosRange)
					{
						QuestReward(condition);
						DeleteCondition(condition);
						i--;
						// 케릭터를 정지시킨다.
						ZPawnManager.Instance.MyEntity.StopMove();
					}
				}
			}
			else if (condition.QuestTable.CompleteCheck == E_CompleteCheck.DeliveryItem)
			{
				ulong count = ZNet.Data.Me.GetCurrency(condition.QuestTable.DeliveryItemID);
				if (condition.TargetCount != count)
				{
					condition.TargetCount = (uint)count;
					QuestData questData = new QuestData();
					questData.QuestTid = condition.QuestTID;
					questData.Type = condition.QuestTable.QuestType;
					questData.Value1 = condition.TargetCount;
					mUIFrameQuest.HandleQuestUpdate(questData);
				}
			}
		}
	}

	/// <summary> 튜토리얼 완료 </summary>
	public bool EventTutorialClear()
	{
		Dictionary<uint, SQuestCompletCondition>.Enumerator it = m_mapConditionIntance.GetEnumerator();

		bool ret = false;
		while (it.MoveNext())
		{
			SQuestCompletCondition condition = it.Current.Value;
			if (condition.QuestTable.CompleteCheck == E_CompleteCheck.Tutorial)
			{
				UIManager.Instance.Show<UIFrameQuest>().DoUIQuestDialog(condition.QuestTID, UIQuestDialog.E_DialogType.Reward);
				it = DeleteCondition(condition);

				ret = true;
			}
		}

		return ret;
	}

	public void StopEventChecker()
	{
		mUpdate = false;
	}

	//-----------------------------------------------------
	private Dictionary<uint, SQuestCompletCondition>.Enumerator DeleteCondition(SQuestCompletCondition _condition)
	{
		m_listConditionUpdate.Remove(_condition);
		m_mapConditionIntance.Remove(_condition.QuestTID);
		return m_mapConditionIntance.GetEnumerator();
	}

	private void QuestReward(SQuestCompletCondition _condition)
	{
		ZNet.Data.Me.CurCharData.RewardQuest(_condition.QuestTID);
	}
}
