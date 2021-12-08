using GameDB;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ZDefine;
using ZNet.Data;

public class ChangeContentDispatch : PCRContentBase
{
	[SerializeField] private UIDispatchScrollAdapter osaDispatch;
	[SerializeField] private UIPopupDispatchTeamInfo popupTeamInfo;
	[SerializeField] private DispatchDetailInfo dispatchDetail;

	[SerializeField] private GameObject objDispatchInfo;

	[SerializeField] private Text txtTeamInfo;
	[SerializeField] private GameObject objTeamLvUp;
	[SerializeField] private Slider sliderTeamExp;

	[SerializeField] private GameObject objEmptyNotice;

	[SerializeField] protected MileageChangePetPopup popupDetail;

	private UIFramePetChangeBase.C_ContentUseObject Checker;

	private List<OSA_DispatchData> listDispatchData = new List<OSA_DispatchData>();

	public void Initialize(UIFramePetChangeBase.C_ContentUseObject checker)
	{
		Checker = checker;

		ZPoolManager.Instance.Spawn(E_PoolType.UI, nameof(UIDispatchListItem), (obj)=>
		{
			osaDispatch.Initialize(OnClickSlot, OnClickReward);

			ZPoolManager.Instance.Return(obj);
		});
		dispatchDetail.Initialize(OpenDetail);
	}

	public override void ShowContent()
	{
		base.ShowContent();

		ResetDispatchData();
		RefreshUI();

		if (listDispatchData.Count > 0)
			InvokeRepeating(nameof(RefreshRemainTime), 0, 1f);
	}

	private void ResetDispatchData()
	{
		listDispatchData.Clear();

		foreach (var iter in Me.CurCharData.GetChangeQuestDataList())
		{
			if (iter.RewardDt > 0)
				continue;

			listDispatchData.Add(new OSA_DispatchData(iter));
		}

		listDispatchData.Sort((x, y) => x.data.QuestTid.CompareTo(y.data.QuestTid));

		osaDispatch.ResetListData(listDispatchData);

		if (listDispatchData.Count > 0)
			OnClickSlot(listDispatchData[0]);


		bool hasValue = listDispatchData.Count > 0;

		objEmptyNotice.SetActive(!hasValue);
		objDispatchInfo.SetActive(hasValue);
	}

	private void RefreshUI()
	{
		var curQuestLevel = Me.CurUserData.ChangeQuestLv;
		var curExp = Me.CurUserData.ChangeQuestExp;
		var curTotalExp = DBChangeQuest.GetChangeQuestExpByCurrentLevel(curQuestLevel);

		var table = DBChangeQuest.GetChangeQuestLevelByLevel(curQuestLevel);

		objTeamLvUp.gameObject.SetActive(false);

		if (table != null)
		{
			if (table.LevelUpType == E_LevelUpType.Up)
			{
				if (curExp >= table.LevelUpCount)
					objTeamLvUp.gameObject.SetActive(true);

				curExp -= DBChangeQuest.GetChangeQuestExpByPreLevel(curQuestLevel);
			}
			else
			{
				curExp = curTotalExp;
			}

		}

		txtTeamInfo.text = DBLocale.GetText("Change_Dispatch_Team", curQuestLevel);
		sliderTeamExp.value = (float)curExp / (float)curTotalExp;
	}

	public override void HideContent()
	{
		CancelInvoke();

		popupTeamInfo.SetState(false);

		UIManager.Instance.Close<UIPopupRegistDispatch>(true);

		if (popupDetail.gameObject.activeSelf)
		{
			popupDetail.OnClose();
			popupDetail.gameObject.SetActive(false);
		}

		base.HideContent();
	}

	private void OnClickSlot(OSA_DispatchData data)
	{
		osaDispatch.SetSelectedData(data);
		osaDispatch.RefreshData();
		dispatchDetail.SetDetailInfo(data.data);
	}

	private void RefreshRemainTime()
	{
		dispatchDetail.RefreshRemainTime();
		osaDispatch.RefreshRemainTime();
	}

	public void OnClickTeamInfo()
	{
		popupTeamInfo.SetState(true, OnClickLevelUpDispatchTeam);
	}

	public void OnClickAutoRegist()
	{
		dispatchDetail.OnClickAutoRegist();
	} 

	public void OpenDetail(uint tid)
	{
		var data = new MileageBaseDataIdentifier(tid, MileageDataEvaluateTargetDataType.Change);

		popupDetail.Open(data, () => {
			popupDetail.OnClose();
			popupDetail.gameObject.SetActive(false);
		});
		popupDetail.gameObject.SetActive(true);

	}

	private void OnClickReward(ChangeQuestData data)
	{
		var changeList = Me.CurCharData.GetChangeDataList().FindAll(item => item.ChangeQuestTid == data.QuestTid);
		List<uint> listTid = new List<uint>();

		foreach (var iter in changeList)
		{
			listTid.Add(iter.ChangeTid);
		}

		ZWebManager.Instance.WebGame.REQ_ChangeQuestReward(data.QuestTid, listTid, (recvPacket, recvMsgPacket) =>
		{
			UIManager.Instance.Open<UIFrameItemRewardShot>((str, frame) =>
			{
				frame.AddItem(recvMsgPacket.GetItems);
			});

			ResetDispatchData();
			RefreshUI();
			RefreshRemainTime();
		});
	}

	public void OnClickDispatch()
	{
		var data = dispatchDetail.GetRegistedList(-1);

		ZWebManager.Instance.WebGame.REQ_ChangeQuestRegistChange(dispatchDetail.data.QuestTid, data, (recvPacket, recvMsgPacket) =>
		{
			OnClickSlot(osaDispatch.selectedData);
			RefreshRemainTime();
		});
	}

	public void OnClickRegist(int i)
	{
		dispatchDetail.OnClickRegist(i);
	}

	public void OnClickLevelUpDispatchTeam()
	{
		UIMessagePopup.ShowPopupOkCancel(DBLocale.GetText("Change_Quest_LevelUp_Alert"), onOk: () =>
	   {
		   ZWebManager.Instance.WebGame.REQ_ChangeQuestLevelUp((recvPacket, recvMsgPacket) =>
		   {
			   popupTeamInfo.SetState(false);
			   RefreshUI();
		   });
	   });
	}

	public void OnClickAutoDispatchAll()
	{
		var qQuestData = new Queue<ChangeQuestData>();

		var now = TimeManager.NowSec;
		foreach (var iter in Me.CurCharData.GetChangeQuestDataList())
		{
			if (iter.StartDt > 0)
				continue;
			if (iter.EndDt > 0)
				continue;

			if (iter.RewardDt > 0)
				continue;

			qQuestData.Enqueue(iter);
		}
		REQ_AutoDispatchAll(qQuestData, qQuestData.Count,() =>
		{
			ResetDispatchData();
			RefreshRemainTime();
		});
	}

	public void OnClickAutoRewardAll()
	{
		var qQuestData = new Queue<ChangeQuestData>();

		var now = TimeManager.NowSec;
		foreach (var iter in Me.CurCharData.GetChangeQuestDataList())
		{

			if (iter.EndDt > now || iter.EndDt <= 0)
				continue;

			if (iter.RewardDt > 0)
				continue;

			qQuestData.Enqueue(iter);
		}

		if(qQuestData.Count<=0)
		{
			UIMessagePopup.ShowPopupOk(DBLocale.GetText("Change_Dispatch_Error_NoReward_Auto"));
			return;
		}

		List<WebNet.ItemInfo> listGainInfo = new List<WebNet.ItemInfo>();

		REQ_RewardDispatchAll(qQuestData, (reward) =>
		{
			listGainInfo.AddRange(reward);
		}, () =>
		{
			UIManager.Instance.Open<UIFrameItemRewardShot>((str, frame) =>
			{
				frame.AddItem(listGainInfo);
			});
			ResetDispatchData();
			RefreshRemainTime();
		});
	}

	private void REQ_AutoDispatchAll(Queue<ChangeQuestData> queueDispatch,int countCheker,  Action onRecvAll)
	{
		if (queueDispatch.Count <= 0)
		{
			if(countCheker==0)
			{
				UIMessagePopup.ShowPopupOk(DBLocale.GetText("Change_Dispatch_Error_NotEnough_Auto"));
				return;
			}

			onRecvAll?.Invoke();
			return;
		}

		var data = queueDispatch.Dequeue();

		if (DBChangeQuest.GetChangeQuest(data.QuestTid, out var table) == false)
		{
			REQ_AutoDispatchAll(queueDispatch, countCheker-1, onRecvAll);
			return;
		}

		List<uint> listRegist = new List<uint>();

		if (SetListRegist(table.ChangeType1, table.ChangeGrade1, table.ChangeCount1) == false ||
		    SetListRegist(table.ChangeType2, table.ChangeGrade2,table.ChangeCount2) == false ||
		    SetListRegist(table.ChangeType3, table.ChangeGrade3, table.ChangeCount3) == false)
		{
			if (listRegist.Count < table.QuestSlotCount)
			{
				REQ_AutoDispatchAll(queueDispatch, countCheker - 1, onRecvAll);
				return;
			}
		}

		ZWebManager.Instance.WebGame.REQ_ChangeQuestRegistChange(data.QuestTid, listRegist, delegate
		{
			REQ_AutoDispatchAll(queueDispatch,countCheker, onRecvAll);
		});


		bool SetListRegist(E_ChangeQuestType ty, byte grade, int count)
		{
			if (ty == E_ChangeQuestType.None || grade == 0)
				return true;

			for (int i = 0; i < count; i++)
			{
				var registTarget = DBChangeQuest.GetDisPatchableChange(ty, grade, listRegist);
				if (registTarget == null)
					return false;

				listRegist.Add(registTarget.ChangeID);
			}
			return true;
		}
	}


	private void REQ_RewardDispatchAll(Queue<ChangeQuestData> queueDispatch, Action<List<WebNet.ItemInfo>> onReward, Action onRecvAll)
	{
		if (queueDispatch.Count <= 0)
		{
			onRecvAll?.Invoke();
			return;
		}

		var data = queueDispatch.Dequeue();
		var capt = osaDispatch.selectedData;

		var changeList = Me.CurCharData.GetChangeDataList().FindAll(item => item.ChangeQuestTid == data.QuestTid);
		List<uint> listTid = new List<uint>();

		foreach (var iter in changeList)
		{
			listTid.Add(iter.ChangeTid);
		}

		List<WebNet.ItemInfo> listGainInfo = new List<WebNet.ItemInfo>();

		ZWebManager.Instance.WebGame.REQ_ChangeQuestReward(data.QuestTid, listTid, (recvPacket, recvMsgPacket) =>
		{
			if (recvMsgPacket.ResultQuest.HasValue)
			{
				var resData = recvMsgPacket.ResultQuest.Value;

				var osaData = osaDispatch.Data.List.Find(item => item.data.QuestTid == resData.QuestTid);
				if (osaData != null)
					osaData.data.Reset(resData);
			}

			if (recvMsgPacket.GetItems.HasValue)
			{
				listGainInfo.Add(recvMsgPacket.GetItems.Value);
			}
			onReward?.Invoke(listGainInfo);

			//if (capt.data.QuestTid == osaDispatch.selectedData.data.QuestTid)
			//{
			//osaDispatch.SetSelectedData(capt);
			//OnClickSlot(capt);
			//}

			REQ_RewardDispatchAll(queueDispatch, onReward, onRecvAll);
		});
	}
}

[Serializable]
public class DispatchDetailInfo
{
	[Serializable]
	public class DispatchSlotPair
	{
		[SerializeField] private int idx;

		//본체
		[SerializeField] private GameObject obj;

		//등록 요구 조건
		[SerializeField] private Image imgClass;
		[SerializeField] private Image imgGrade;

		//등록된놈
		[SerializeField] private UIPetChangeListItem registedChange;

		public bool isRegisted = false;

		public uint registedTid = 0;

		private byte grade;
		private E_ChangeQuestType registType;

		[NonSerialized]
		private DispatchDetailInfo sender;

		private bool isDispatched = false;

		public bool IsOn => grade > 0;

		public void Initialize(DispatchDetailInfo _sender)
		{
			sender = _sender;
		}

		// 슬롯 초기화, 0일시 비활성화
		public void ResetSlot(byte _grade, E_ChangeQuestType _type)
		{
			isDispatched = false;
			grade = _grade;
			registType = _type;

			obj.SetActive(IsOn);
			registedChange.gameObject.SetActive(false);
			isRegisted = false;

			if (grade <= 0)
				return;

			imgClass.sprite = UICommon.GetChangeQuestTypeSprite(registType);
			imgGrade.sprite = UICommon.GetGradeSprite(grade);
		}

		public void SetSlot(uint changeTid, bool _isDispatched)
		{
			registedTid = changeTid;
			isDispatched = _isDispatched;
			SetSlot();
		}

		public void SetSlot()
		{
			isRegisted = true;

			registedChange.SetSlotSimple(E_PetChangeViewType.Change, registedTid, OnClickRegist, E_PCR_PostSetting.GainStateOff);
			registedChange.gameObject.SetActive(true);
		}

		public void OnConfirm(C_PetChangeData data)
		{
			registedTid = data?.Tid ?? 0;

			SetSlot();
			sender.RefreshRegistCount();
		}

		// 등록되있느놈클릭시
		public void OnClickRegist(C_PetChangeData data)
		{
			if(isDispatched)
			{
				sender.ShowDetail(registedTid);

				return;
			}

			if (isRegisted)
			{
				//선택 취소 처리
				ResetSlot(grade, registType);
				return;
			}

			OnClickRegist();
		}

		public void OnClickRegist()
		{
			UIManager.Instance.Open<UIPopupRegistDispatch>((str, popup) =>
			{
				popup.SetPopup(sender.GetRegistedList(-1), grade, registType, OnConfirm);
			});
		}

		public bool SetAuto()
		{
			var registTarget = DBChangeQuest.GetDisPatchableChange(registType, grade, sender.GetRegistedList(idx));

			if (registTarget == null)
				return false;

			registedTid = registTarget.ChangeID;

			SetSlot();
			sender.RefreshRegistCount();

			return true;
		}
	}

	[SerializeField] private List<DispatchSlotPair> listSlotPair;
	[SerializeField] private UIItemSlot slotReward;

	[SerializeField] private Text txtTitle;
	[SerializeField] private Text txtState;

	[SerializeField] private Text txtRemainTime;
	[SerializeField] private Text txtRegistCount;

	[SerializeField] private List<Image> imgStar;

	[SerializeField] private GameObject objButtonGroup;

	[SerializeField] private ZButton btnDispatch;

	[SerializeField] private Text txtDispatchTimeDesc;

	public ChangeQuestData data;

	public ChangeQuest_Table table;

	public Action<uint> onClickDetail;
	public void Initialize(Action<uint> _onClickDetail)
	{
		onClickDetail = _onClickDetail;

		foreach (var iter in listSlotPair)
			iter.Initialize(this);
	}

	public void SetDetailInfo(ChangeQuestData _data)
	{
		data = _data;

		if (DBChangeQuest.GetChangeQuest(data.QuestTid, out table) == false)
		{
			ZLog.LogError(ZLogChannel.UI, $"ChangeQuest테이블 누락~~ Tid : {data.QuestTid}");
			return;
		}

		txtTitle.text = DBUIResouce.GetGradeFormat(DBLocale.GetText(table.QuestTitle), (byte)table.QuestGrade);
		slotReward.SetItem(table.RewardItem[0], table.RewardItemCount[0]);

		for (int i = 0; i < imgStar.Count; i++)
		{
			imgStar[i].sprite = UICommon.GetStarSprite(i < table.QuestGrade);
		}

		btnDispatch.interactable = data.StartDt < 0;
		RefreshRemainTime();
		RefreshRegistSlot();
		RefreshRegistCount();
	}

	public void ShowDetail(uint tid)
	{
		onClickDetail?.Invoke(tid);
	}

	public void RefreshRemainTime()
	{
		ulong now = TimeManager.NowSec;


		if (data.EndDt > 0 && data.EndDt <= now)// ㅇ놔료
		{
			if (objButtonGroup.activeSelf)
				objButtonGroup.SetActive(false);

			if (txtState.gameObject.activeSelf == false)
				txtState.gameObject.SetActive(true);

			txtState.text = DBLocale.GetText("Change_Quest_SendEnd");
			txtDispatchTimeDesc.text = DBLocale.GetText("Class_Dispatch_Time");
			txtRemainTime.text = DBLocale.GetText("Change_Quest_SendEnd");
		}
		else if (data.StartDt > 0)//진행중
		{
			ulong total = data.EndDt - data.StartDt;
			ulong progress = total - (now - data.StartDt);

			if (objButtonGroup.activeSelf)
				objButtonGroup.SetActive(false);

			if (txtState.gameObject.activeSelf == false)
				txtState.gameObject.SetActive(true);

			txtState.text = DBLocale.GetText("Change_Quest_Sending");

			txtDispatchTimeDesc.text = DBLocale.GetText("Class_Dispatch_Time");
			txtRemainTime.text = TimeHelper.GetRemainTime(progress);
		}
		else//안함
		{


			if (objButtonGroup.activeSelf == false)
				objButtonGroup.SetActive(true);

			if (txtState.gameObject.activeSelf)
				txtState.gameObject.SetActive(false);
			txtState.text = string.Empty;
			txtDispatchTimeDesc.text = DBLocale.GetText("Change_Quest_SendTime");
			txtRemainTime.text = TimeHelper.GetRemainTime(table.CostTime);
		}
	}

	private void RefreshRegistSlot()
	{
		List<(byte, E_ChangeQuestType)> listGrade = new List<(byte, E_ChangeQuestType)>();

		AddGradeList(table.ChangeGrade1, table.ChangeCount1, table.ChangeType1);
		AddGradeList(table.ChangeGrade2, table.ChangeCount2, table.ChangeType2);
		AddGradeList(table.ChangeGrade3, table.ChangeCount3, table.ChangeType3);

		var listChange = Me.CurCharData.GetChangeDataList().FindAll(item => item.ChangeQuestTid == table.ChangeQuestID);

		listGrade.Sort(SortComparison);

		for (int i = 0; i < listSlotPair.Count; i++)
		{
			if (listGrade.Count <= i)
			{
				listSlotPair[i].ResetSlot(0, E_ChangeQuestType.None);
				continue;
			}

			if (data.StartDt > 0)
			{
				listSlotPair[i].SetSlot(listChange[i].ChangeTid, true);
			}
			else
			{
				listSlotPair[i].ResetSlot(listGrade[i].Item1, listGrade[i].Item2);
			}
		}

		void AddGradeList(byte grade, byte count, E_ChangeQuestType type)
		{
			for (int i = 0; i < count; i++)
				listGrade.Add((grade, type));
		}
	}

	private int SortComparison((byte, E_ChangeQuestType) x, (byte, E_ChangeQuestType) y)
	{
		// 등급비교
		if (x.Item1 > y.Item1)
			return -1;

		if (x.Item1 < y.Item1)
			return 1;

		// 타입비교
		if (x.Item2 < y.Item2)
			return -1;

		if (x.Item2 > y.Item2)
			return 1;

		return 0;
	}



	public void RefreshRegistCount()
	{
		int registed = 0;
		listSlotPair.ForEach(item =>
		{
			if (item.isRegisted)
				registed++;
		});

		txtRegistCount.text = UICommon.GetProgressText(registed, table.QuestSlotCount, false);

		btnDispatch.interactable = registed == table.QuestSlotCount;
	}

	public List<uint> GetRegistedList(int idx)
	{
		List<uint> listRegist = new List<uint>();

		for (int i = 0; i < listSlotPair.Count; i++)
		{
			if (i == idx)
				continue;

			if (listSlotPair[i].isRegisted == false)
				continue;

			listRegist.Add(listSlotPair[i].registedTid);
		}

		return listRegist;
	}

	public void OnClickRegist(int idx)
	{
		listSlotPair[idx].OnClickRegist();
	}

	public void OnClickAutoRegist()
	{
		bool isSuccess = true;

		foreach (var iter in listSlotPair)
		{
			if (iter.IsOn == false)
				continue;

			if (iter.SetAuto() == false)
			{
				isSuccess = false;
				break;
			}
		}

		if (isSuccess == false)
		{
			UIMessagePopup.ShowPopupOk(DBLocale.GetText("Change_Dispatch_Error_NotEnough_Auto"));
			return;
		}
	}
}