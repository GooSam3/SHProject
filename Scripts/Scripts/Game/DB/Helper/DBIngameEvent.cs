using GameDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebNet;
using ZNet.Data;
using E_ServerEventSubCategory = WebNet.E_ServerEventSubCategory;

/// <summary>
/// 인게임 이벤트에서 사용하는 테이블 전부 관리
/// </summary>

[UnityEngine.Scripting.Preserve]
class DBIngameEvent : IGameDBHelper
{
	// ATTEND { AttendEventID : Table }
	private static Dictionary<uint, AttendEvent_Table> dicAttendEvent = new Dictionary<uint, AttendEvent_Table>();

	// ATTEND { GroupID : Table }
	private static Dictionary<uint, List<AttendEvent_Table>> dicAttendEventGroup = new Dictionary<uint, List<AttendEvent_Table>>();

	// CASH { RewardID : Table }
	private static Dictionary<uint, EventReward_Table> dicCashEvent = new Dictionary<uint, EventReward_Table>();

	// CASH { groupID : Table }
	private static Dictionary<uint, List<EventReward_Table>> dicCashEventGroup = new Dictionary<uint, List<EventReward_Table>>();

	// QUEST_EVENT { QuestEventID : Table }
	private static Dictionary<uint, QuestEvent_Table> dicQuestEvent = new Dictionary<uint, QuestEvent_Table>();

	// QUEST_EVENT { GroupID : Table }
	private static Dictionary<uint, List<QuestEvent_Table>> dicQuestEventGroup = new Dictionary<uint, List<QuestEvent_Table>>();

	// EVENT_ICON { type : spriteID }
	private static Dictionary<E_ServerEventSubCategory, string> dicEventIcon = new Dictionary<E_ServerEventSubCategory, string>();

	public void OnReadyData()
	{
		dicAttendEvent.Clear();
		dicAttendEventGroup.Clear();

		dicCashEvent.Clear();
		dicCashEventGroup.Clear();

		dicQuestEvent.Clear();
		dicQuestEventGroup.Clear();

		dicEventIcon.Clear();

		// att
		foreach (var iter in GameDBManager.Container.AttendEvent_Table_data.Values)
		{
			if (dicAttendEvent.ContainsKey(iter.AttendEventID) == false)
				dicAttendEvent.Add(iter.AttendEventID, iter);

			if (dicAttendEventGroup.ContainsKey(iter.GroupID) == false)
				dicAttendEventGroup.Add(iter.GroupID, new List<AttendEvent_Table>());

			dicAttendEventGroup[iter.GroupID].Add(iter);
		}

		// cash
		foreach(var iter in GameDBManager.Container.EventReward_Table_data.Values)
		{
			if (dicCashEvent.ContainsKey(iter.RewardID) == false)
				dicCashEvent.Add(iter.RewardID, iter);

			if (dicCashEventGroup.ContainsKey(iter.RewardGroupID) == false)
				dicCashEventGroup.Add(iter.RewardGroupID, new List<EventReward_Table>());

			dicCashEventGroup[iter.RewardGroupID].Add(iter);
		}

		foreach(var iter in dicCashEventGroup.Values)
		{
			iter.Sort((x, y) => x.TypeCount.CompareTo(y.TypeCount));
		}

		//questEvent
		foreach (var iter in GameDBManager.Container.QuestEvent_Table_data.Values)
		{
			if (dicQuestEvent.ContainsKey(iter.EventQuestID) == false)
				dicQuestEvent.Add(iter.EventQuestID, iter);

			if (dicQuestEventGroup.ContainsKey(iter.GroupID) == false)
				dicQuestEventGroup.Add(iter.GroupID, new List<QuestEvent_Table>());

			dicQuestEventGroup[iter.GroupID].Add(iter);
		}

		//EventIcon
		foreach(var iter in GameDBManager.Container.EventList_Table_data.Values)
		{
			if (Enum.TryParse<E_ServerEventSubCategory>(iter.EventKey, out var type) == false)
				continue;

			if (dicEventIcon.ContainsKey(type) == false)
				dicEventIcon.Add(type, iter.EventIcon);
		}

	}

	//----------ATTEND
	public static bool GetAttendData(uint tid, out AttendEvent_Table table)
	{
		return dicAttendEvent.TryGetValue(tid, out table);
	}

	public static bool GetAttendDataGroup(uint groupId, out List<AttendEvent_Table> listTable)
	{
		return dicAttendEventGroup.TryGetValue(groupId, out listTable);
	}

	public static int GetAttendGroupCount(uint groupId)
	{
		if (dicAttendEventGroup.TryGetValue(groupId, out var list) == false)
			return 0;
		else
			return list.Count;
	}

	/// <summary>
	/// 가장 큰 목표 출석일을 가져옴, OneWeek만
	/// </summary>
	/// <param name="groupId"></param>
	/// <param name=""></param>
	/// <returns></returns>
	public static uint GetAttendMaxPurposeDay(uint groupId)
	{
		if (dicAttendEventGroup.TryGetValue(groupId, out var list) == false)
			return 0;

		if (list[0].AttendBoardType != E_AttendBoardType.OneWeek)
			return 0;

		return list.Max(item => item.PurposeDay);
	}

	/// <summary>
	/// 그룹아이디를 참조하는 테이블 1개를 가져옴(타입 및 보드타입 판별용)
	/// </summary>
	public static bool GetAttendGroupDataFirst(uint groupId, out AttendEvent_Table table)
	{
		table = null;
		if (dicAttendEventGroup.TryGetValue(groupId, out var list) == false)
			return false;

		table = list[0];
		return true;
	}

	/// <summary>
	/// 따로 출석 일 출력해주나 여부 ex)연속출석 1 3 5 7 9 -> true(2일차에 보상안받지만 확인용)
	/// OneWeek만 사용
	/// </summary>
	/// <returns></returns>
	public static bool IsViewAttendCount(uint groupId)
	{
		var max = GetAttendMaxPurposeDay(groupId);

		if (max <= 0)
			return false;

		if (dicAttendEventGroup.TryGetValue(groupId, out var list) == false)
			return false;

		return max != list.Count;
	}

	//-----------CASH
	public static bool GetCashEventData(uint tid, out EventReward_Table table)
	{
		return dicCashEvent.TryGetValue(tid, out table);
	}

	public static bool GetCashEventDataGroup(uint tid, out List<EventReward_Table> listTable)
	{
		return dicCashEventGroup.TryGetValue(tid, out listTable);
	}

	public static bool GetCashEventDataFirst(uint groupId, out EventReward_Table table)
	{
		table = null;
		if (GetCashEventDataGroup(groupId, out var list) == false)
			return false;

		table = list[0];
		return true;
	}

	public static bool GetCashEventDataNext(EventReward_Table table, out EventReward_Table nextTable)
	{
		nextTable = null;

		if(GetCashEventDataGroup(table.RewardGroupID, out var list))
			return false;

		var curIdx = list.FindIndex(item => item.TypeCount == table.TypeCount);

		if (curIdx<-1 || curIdx >= list.Count - 1)// 해당 값이 없거나 마지막임
			return false;

		nextTable = list[curIdx + 1];
		return true;
	}

	// 레벨업 이벤트의 대상 테이블 가져옴
	// 디폴트 첫번째 테이블
	public static EventReward_Table GetCashEventTargetLevel(uint groupId)
	{
		EventReward_Table table = null;

		uint targetLv = Me.CurCharData.Level;

		if(GetCashEventDataGroup(groupId, out var list))
		{
			table = list[0];

			foreach (var iter in list)
			{
				if (iter.TypeCount <= targetLv)
					table = iter;
				else
					break;
			}
		}

		return table;
	}

	//----------------QUEST_EVENT

	public static bool GetQuestEventData(uint tid, out QuestEvent_Table table)
	{
		return dicQuestEvent.TryGetValue(tid, out table);
	}

	public static bool GetQuestEventDataGroup(uint groupId, out List<QuestEvent_Table> listTable)
	{
		return dicQuestEventGroup.TryGetValue(groupId, out listTable);
	}

	// 배틀패스의 첫번째 데이터를 가져옴, 재료등등 배틀패스 정보 참조용
	public static bool GetBattlePassDataFirst(uint groupId,out QuestEvent_Table table)
	{
		table = null;
		if (GetBattlePassTargetDate(groupId, E_EventOpenDay.FirstDay, out var list) == false)
			return false;

		table = list[0];
		return true;
	}

	public static bool GetBattlePassMainRewardData(uint groupId, out QuestEvent_Table table)
	{
		table = null;
		if (GetBattlePassTargetDate(groupId, E_EventOpenDay.None, out var list) == false)
			return false;

		foreach(var iter in list)
		{
			if (iter.CompleteCheck != E_EventCompleteCheck.TargetItemCollect)
				continue;

			table = iter;
			return true;
		}
		return false;
	}

	// 배틀패스의 총 길이(일)를 가져옴
	public static int GetBattlePassDayMax(uint groupId)
	{ 
		if(GetQuestEventDataGroup(groupId, out var list)==false)
		{
			return 0;
		}

		Dictionary<E_EventOpenDay, bool> dicEventOpenDay = new Dictionary<E_EventOpenDay, bool>();

		foreach(var iter in list)
		{
			if (iter.EventOpenDay == E_EventOpenDay.None)
				continue;

			if(dicEventOpenDay.ContainsKey(iter.EventOpenDay) == false)
			{
				dicEventOpenDay.Add(iter.EventOpenDay, true);
			}
		}

		return dicEventOpenDay.Keys.Count;
	}


	// 그룹 못찾거나, 데이터 없을시 false
	public static bool GetBattlePassTargetDate(uint groupId, E_EventOpenDay dateType, out List<QuestEvent_Table> listTable)
	{
		listTable = new List<QuestEvent_Table>();

		List<QuestEvent_Table> listEvent = new List<QuestEvent_Table>();
		if (GetQuestEventDataGroup(groupId, out listEvent) == false)
			return false;

		foreach(var iter in listEvent)
		{
			if (iter.EventOpenDay == dateType)
				listTable.Add(iter);
		}

		return listEvent.Count > 0;
	}


	// ----- ICON
	public static bool GetEventIcon(WebNet.E_ServerEventSubCategory type, out string iconID)
	{
		return dicEventIcon.TryGetValue(type, out iconID);
	}
}