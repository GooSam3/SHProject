using GameDB;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ZNet.Data;

public class UIEventContentBattlePass : UIEventContentBase
{
	[Serializable]
	private class BPDateSlot
	{
		public GameObject objRoot;

		public Text txtTitle;
		public GameObject objRedDot;

		// 0 is deactive
		public void SetSlot(uint groupId, uint date)
		{
			objRoot.gameObject.SetActive(date > 0);

			if (date <= 0)
				return;

			int maxNum = 0;
			int clearNum = 0;

			if (DBIngameEvent.GetBattlePassTargetDate(groupId, (E_EventOpenDay)date, out var list))
			{
				var myList = Me.CurCharData.GetBattlePassDataList(groupId, (byte)date);

				maxNum = list.Count;

				bool isRewarded = false;

				foreach (var iter in myList)
				{
					if (iter.State != WebNet.E_QuestState.None)
						clearNum++;

					if (!isRewarded && iter.State == WebNet.E_QuestState.Reward)
						isRewarded = true;
				}

				objRedDot.SetActive(isRewarded);
			}

			txtTitle.text = $"{DBLocale.GetText("QuestEvent_Day", date)}\n{UICommon.GetProgressText(clearNum, maxNum)}";
			objRedDot.SetActive(false);
		}
	}

	[Serializable]
	private class BPItemInfo
	{
		public UIItemSlot slot;
		public Text txt;

		/// <summary>
		/// 아이템설정
		/// </summary>
		/// <param name="itemTid"></param>
		/// <param name="cnt">아이템의 갯수, useTxtByCnt == true 일시 아이템의 목표 갯수</param>
		/// <param name="useTxtByCnt">true 일시 텍스트 갯수로 설정 (재료아이템)</param>
		public void SetItem(uint itemTid, ulong cnt, bool useTxtByCnt = false)
		{
			if (DBItem.GetItem(itemTid, out var table) == false)
			{
				slot.SetEmpty();
				txt.text = string.Empty;
				return;

			}

			slot.SetItem(itemTid, useTxtByCnt ? 0 : cnt);
			slot.SetPostSetting(UIItemSlot.E_Item_PostSetting.ShadowOff | UIItemSlot.E_Item_PostSetting.LockOff);

			if (useTxtByCnt)
				txt.text = UICommon.GetProgressText((int)Me.CurCharData.GetItem(itemTid).cnt, (int)cnt, false);
			else
				txt.text = UICommon.GetItemText(table);
		}
	}

	[SerializeField] private BPItemInfo rewardClear;        // 클리어 보상 아이템

	[SerializeField] private Text txtExchangeDesc;          // 교환 설명 // {0} 교환 완료시 
	[SerializeField] private BPItemInfo rewardExchange;     // 교환 보상 아이템
	[SerializeField] private ZButton btnExchange;           // 교환

	[SerializeField] private Text txtOwnMatDesc;            // 재료 설명 // 보유중인 {0}
	[SerializeField] private BPItemInfo matItem;            // 재료 아이템

	[SerializeField] private Text txtEventTimeRange;        // 이벤트 기간 // ~ 다음 점검 전까지

	[SerializeField] private ZScrollRect srDate;            // 날짜탭 스크롤 // -일차
	[SerializeField] private List<BPDateSlot> listDateSlot = new List<BPDateSlot>(); // 날짜탭 슬롯
	[SerializeField] private ZToggle firstToggle;           // 처음 슬롯

	[SerializeField] private UIBattlePassListAdapter osaBattlePass; // 배틀패스 컨텐츠 osa

	private List<QuestEvent_Table> listBattlePass = new List<QuestEvent_Table>(); // 현재 선택된 날에 대한 데이터임

	private uint battlePassGroupId = 0; // 현재 배틀패스 그룹아이디

	protected override bool SetContent(IngameEventInfoConvert _eventData)
	{
		// TEST
		battlePassGroupId = 10000;
		//battlePassGroupId = _eventData.groupId;

		listBattlePass.Clear();

		// togglegroup 초기화때문에 임의의 위치에 이동 후 초기화
		transform.localPosition = new Vector2(10000, 10000);
		this.gameObject.SetActive(true);

		// 보상정보
		RefreshRewardUI();

		// 슬롯 및 컨텐츠 리스트
		var bpMaxDay = DBIngameEvent.GetBattlePassDayMax(battlePassGroupId);

		for (int i = 0; i < listDateSlot.Count; i++)
		{
			if (i >= bpMaxDay)
				listDateSlot[i].SetSlot(0, 0);
			else
				listDateSlot[i].SetSlot(battlePassGroupId, (uint)i + 1);
		}

		if (osaBattlePass.IsInitialized == false)
		{
			ZPoolManager.Instance.Spawn(E_PoolType.UI, nameof(UIBattlePassListItem), delegate
			{
				osaBattlePass.Initialize(OnClickBPConfirm);
				listBattlePass.Clear();//혹시모르니
				osaBattlePass.ResetListData(listBattlePass, 0);
				firstToggle.SelectToggle(false);             // --> OnClickDate
				OnClickDate(0);

				transform.localPosition = Vector2.zero;

				IsLoadSuccess = true;
			});
			return false;

		}
		else
		{
			transform.localPosition = Vector2.zero;
			firstToggle.SelectToggle(false);             // --> OnClickDate
			OnClickDate(0);
		}
		return true;
	}

	IEnumerator CoFrameSkip(Action act)
	{
		yield return null;
		act?.Invoke();
	}

	private void RefreshRewardUI()
	{
		if (DBIngameEvent.GetBattlePassDataFirst(battlePassGroupId, out var bpData))
		{
			rewardClear.SetItem(bpData.RewardItemID[0], bpData.RewardItemCount[0]);
		}

		if (DBIngameEvent.GetBattlePassMainRewardData(battlePassGroupId, out var mainReward))
		{
			uint mainRewardID = mainReward.RewardItemID.Count > 0 ? mainReward.RewardItemID[0] : 0;
			uint mainRewardCnt = mainReward.RewardItemCount.Count > 0 ? mainReward.RewardItemCount[0] : 0;

			bool hasItem = DBItem.GetItem(mainRewardID, out var rewardItem);
			string targetText = hasItem ? DBLocale.GetText(rewardItem.ItemTextID) : string.Empty;

			txtExchangeDesc.text = DBLocale.GetText("Event_BattlePass_Exchange_Desc", targetText);
			rewardExchange.SetItem(mainRewardID, mainRewardCnt);

			hasItem = DBItem.GetItem(mainReward.TargetItemID, out var mItem);
			targetText = hasItem ? DBLocale.GetText(mItem.ItemTextID) : string.Empty;

			txtOwnMatDesc.text = DBLocale.GetText("Event_BattlePass_OwnMaterial_Desc", targetText);
			matItem.SetItem(bpData.TargetItemID, bpData.TargetCount, true);

			btnExchange.interactable = Me.CurCharData.GetEventData(mainReward.GroupID, mainReward.EventQuestID)?.State == WebNet.E_QuestState.Reward;
		}
	}

	protected override void ReleaseContent()
	{
	}

	public void OnClickDate(int i)
	{
		listBattlePass.Clear();

		if (DBIngameEvent.GetBattlePassTargetDate(battlePassGroupId, (E_EventOpenDay)i + 1, out listBattlePass) == false)
		{
			ZLog.Log(ZLogChannel.Event, $"데이터 없음 questEvent groupId : {battlePassGroupId} , date : {(E_EventOpenDay)i + 1}");
		}

		osaBattlePass.ResetListData(listBattlePass, 0);
	}

	public void OnClickRewardExchange()
	{
		if (DBIngameEvent.GetBattlePassMainRewardData(battlePassGroupId, out var mainReward) == false)
		{
			ZLog.Log(ZLogChannel.Event, $"메인보상 없는데 보상눌림, groupid : {battlePassGroupId}");
			return;
		}
		OnClickBPConfirm(mainReward);
	}

	private void OnClickBPConfirm(QuestEvent_Table table)
	{
		var data = Me.CurCharData.GetEventData(table.GroupID, table.EventQuestID);

		if (data == null)
		{
			ZLog.Log(ZLogChannel.Event, $"데이터 없는데 보상눌림,quest tid : {table.EventQuestID}");
			return;
		}

		if (data.State != WebNet.E_QuestState.Reward)
		{
			ZLog.Log(ZLogChannel.Event, $"보상 받을수 없는데 보상버튼눌림,quest tid : {table.EventQuestID}, state : {data.State}");
			return;
		}

		ZWebManager.Instance.WebGame.REQ_GetEventQuestReward(data.QuestTid, delegate
		{
			osaBattlePass.Refresh();
			RefreshRewardUI();
		});
	}

#if UNITY_EDITOR

	[ContextMenu("SET_DATE_SLOTS")]
	private void SetDateSlot_Editor()
	{
		listDateSlot.Clear();

		var comps = this.transform.GetComponentsInChildren<ZToggleGroup>();
		ZToggleGroup group = null;
		foreach (var iter in comps)
		{
			if (iter.name.Equals("Contents"))
			{
				group = iter;
				break;
			}
		}

		var togs = group.GetComponentsInChildren<ZToggle>();
		var idx = 0;
		foreach (var iter in togs)
		{
			var dateSlot = new BPDateSlot();

			listDateSlot.Add(dateSlot);

			iter.group = group;
			iter.ZToggleGroup = group;

			dateSlot.objRoot = iter.gameObject;

			int cnt = 0;
			iter.onValueChanged.RemoveAllListeners();
			UnityEditor.Events.UnityEventTools.AddIntPersistentListener(iter.onValueChanged, OnClickDate, idx++);

			foreach (var obj in iter.GetComponentsInChildren<Transform>())
			{
				if (cnt == 2)
					break;

				if (obj.name.Equals("Txt"))
				{
					dateSlot.txtTitle = obj.GetComponent<ZText>();
					cnt++;
					continue;
				}

				if (obj.name.Equals("Alarm_RedDot"))
				{
					dateSlot.objRedDot = obj.gameObject;
					cnt++;
					continue;
				}
			}
		}
	}

#endif
}
