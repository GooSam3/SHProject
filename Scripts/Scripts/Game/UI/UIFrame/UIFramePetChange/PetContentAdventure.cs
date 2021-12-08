using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZNet.Data;

/// <summary>
/// 주의사항!!
/// UIPetAdventureMapPoint 프리팹은 사전 UIFramePet프리팹에서 생성!
/// 프리팹에 targetAdventureTid 반드시 기입
/// 테이블과 비교 후 해당 id 없을시 해당 프리팹 off
/// </summary>
public class PetContentAdventure : PCRContentBase
{
	[SerializeField] private List<UIPetAdventureMapPoint> listMapPoint;

	[SerializeField] private UIPetAdventureScrollAdapter osaAdventure;

	[SerializeField] private UIPetAdventureInfo adventureInfo;

	[SerializeField] private ZButton btnRewardAll;
	[SerializeField] private ZButton btnAutoRegist;

	private UIFramePetChangeBase.C_ContentUseObject Checker;

	// tid : mappoint
	private Dictionary<uint, UIPetAdventureMapPoint> dicMapPoint = new Dictionary<uint, UIPetAdventureMapPoint>();

	public void Initialize(UIFramePetChangeBase.C_ContentUseObject checker)
	{
		Checker = checker;

		adventureInfo.Initialize();

		ZPoolManager.Instance.Spawn(E_PoolType.UI, nameof(UIPetAdventureListItem), obj=>
		{
			osaAdventure.Initialize(OnClickAdventureListItem);
			ZPoolManager.Instance.Return(obj);
		});

		listMapPoint.ForEach(item => item.gameObject.SetActive(false));
		dicMapPoint.Clear();
		adventureInfo.Release();
	}

	public override void ShowContent()
	{
		base.ShowContent();

		// 갱신
		ZWebManager.Instance.WebGame.REQ_GetPetAdventureInfo(delegate
		{
			dicMapPoint.Clear();

			var listAdventureData = new List<OSA_AdventureData>();

			foreach (var iter in Me.CurCharData.GetPetAdventureList())
			{
				listAdventureData.Add(new OSA_AdventureData(iter));
			}

			// 등록된 프리팹 비교
			foreach (var iter in listMapPoint)
			{
				var data = listAdventureData.Find(item => item.table.AdventureID == iter.TargetTID);

				if (data == null)
				{
					iter.gameObject.SetActive(false);
					continue;
				}

				iter.gameObject.SetActive(true);


				if (dicMapPoint.ContainsKey(data.table.AdventureID) == false)
				{
					dicMapPoint.Add(data.table.AdventureID, iter);
					iter.Initialize(data, OnClickMapPoint);
				}
			}

			osaAdventure.ResetListData(listAdventureData);

			osaAdventure.SelectFirst();
			btnAutoRegist.interactable = osaAdventure.selectedData.advData.status == WebNet.E_PetAdvStatus.Wait;

			if (dicMapPoint.TryGetValue(osaAdventure.selectedData.table.AdventureID, out var point))
			{
				point.SetRadioOn();
			}

			adventureInfo.SetInfoPanel(osaAdventure.selectedData);

		});
		btnRewardAll.interactable = false;
		InvokeRepeating(nameof(RefreshRemainTime), 0, 1f);
	}

	// 탭전환
	public override void HideContent()
	{
		CancelInvoke();

		adventureInfo.Release();

		foreach (var iter in dicMapPoint.Values)
			iter.gameObject.SetActive(false);

		dicMapPoint.Clear();

		osaAdventure.ResetListData(new List<OSA_AdventureData>());

		UIManager.Instance.Close<UIPopupRegistPetAdventure>(true);

		base.HideContent();
	}

	// 프레임 켜짐
	public void OnShowFrame()
	{
		adventureInfo.OnShowFrame();
	}

	// 프레임 꺼짐
	public void OnHideFrame()
	{
		//씬/모델해제
		adventureInfo.OnCloseFrame();
	}

	private void RefreshRemainTime()
	{
		if (osaAdventure.selectedData == null)
			return;


		//-------------- 쿨타임이 끝날때 리스트요청 다시해야함

		bool rewardable = false;

		foreach (var iter in Me.CurCharData.GetPetAdventureList())
		{
			if (iter.status == WebNet.E_PetAdvStatus.Cancel || iter.status == WebNet.E_PetAdvStatus.Reward)
			{
				if (iter.EndDt < TimeManager.NowSec)
				{
					CancelInvoke();

					ZWebManager.Instance.WebGame.REQ_GetPetAdventureInfo((recvPacket, recvMsgPacket) =>
					{
						foreach (var advData in Me.CurCharData.GetPetAdventureList())
						{
							var resetData = osaAdventure.Data.List.Find(item => item.advData.AdvTid == advData.AdvTid);

							if (resetData != null)
							{
								resetData.Reset(advData);
							}

						}
						OnClickAdventureListItem(osaAdventure.selectedData);

						InvokeRepeating(nameof(RefreshRemainTime), 0, 1f);
					});

					return;
				}
			}
			else if (iter.status == WebNet.E_PetAdvStatus.Start)
			{
				if (iter.EndDt < TimeManager.NowSec)
					rewardable = true;
			}

		}

		btnRewardAll.interactable = rewardable;

		osaAdventure.RefreshData();
		//--------------

		//현재켜진놈들 갱신
		long remainTimeDt = (long)osaAdventure.selectedData.advData.EndDt - (long)TimeManager.NowSec;

		foreach (var iter in dicMapPoint.Values)
		{
			iter.RefrshRemainTime();
		}

		adventureInfo.RefreshRemainTime(remainTimeDt);
	}

	// 탭으로 접근
	private void OnClickAdventureListItem(OSA_AdventureData data)
	{
		if (dicMapPoint.TryGetValue(data.table.AdventureID, out var point))
		{
			point.SetRadioOn();
		}

		btnAutoRegist.interactable = osaAdventure.selectedData.advData.status == WebNet.E_PetAdvStatus.Wait;
		adventureInfo.SetInfoPanel(osaAdventure.selectedData);
	}

	// 핀으로 접근
	private void OnClickMapPoint(OSA_AdventureData data)
	{
		osaAdventure.SetSelectedData(data);

		btnAutoRegist.interactable = osaAdventure.selectedData.advData.status == WebNet.E_PetAdvStatus.Wait;
		adventureInfo.SetInfoPanel(osaAdventure.selectedData);
	}

	// 간편모험(자동등록)
	public void OnClickAutoAdventure()
	{
		var data = adventureInfo.Data;

		if (data == null)
			return;

		List<ulong> list = new List<ulong>();
		uint advPower = 0;

		foreach (var iter in DBPetAdventure.GetPossibleAdventurePetList())
		{
			var pet = Me.CurCharData.GetPetData(iter.tid);

			if (pet == null)
				continue;

			if (list.Count >= ZUIConstant.PET_ADVENTRUE_PET_COUNT_MAX)
				break;

			var id = Me.CurCharData.GetPetData(iter.tid)?.PetId ?? 0;

			if (id <= 0)
				continue;

			list.Add(id);
			advPower += iter.adventurePower;
		}

		if (advPower < data.table.NeedPetPower)
		{
			// 엥
			UIMessagePopup.ShowPopupOk(DBLocale.GetText("Pet_Adventure_Error_AutoregistFail"));
			return;
		}

		StartPetAdventure(list);
	}

	private void StartPetAdventure(List<ulong> listID)
	{
		if (listID.Count <= 0)
		{
			UIMessagePopup.ShowPopupOk(DBLocale.GetText("Pet_Adv_Error_None_Registed"));
			return;
		}

		UIMessagePopup.ShowPopupOkCancel(DBLocale.GetText("PetAdventur_Action_Des"), () =>
		{
			var capt = osaAdventure.selectedData;

			ZWebManager.Instance.WebGame.REQ_StartPetAdventure(capt.advData.AdvId, listID, (recvPacket, recvMsgPacket) =>
			{
				if (recvMsgPacket.AdvInfo.HasValue)
					capt.advData.Reset(recvMsgPacket.AdvInfo.Value);

				if (capt.advData.AdvTid == osaAdventure.selectedData.advData.AdvTid)
				{
					osaAdventure.SetSelectedData(capt);
					OnClickAdventureListItem(capt);
				}

				RefreshRemainTime();
			});

			adventureInfo.BlockInteractible();
		}, delegate { });
	}

	// 모두완료
	public void OnClickRewardAll()
	{
		adventureInfo.BlockInteractible();

		Queue<ulong> qData = new Queue<ulong>();

		foreach (var iter in osaAdventure.Data)
		{
			if (iter.advData.status != WebNet.E_PetAdvStatus.Start)
				continue;

			if (iter.advData.EndDt > TimeManager.NowSec)
				continue;

			qData.Enqueue(iter.advData.AdvId);
		}
		List<WebNet.ItemInfo> listGainInfo = new List<WebNet.ItemInfo>();

		REQ_RewardPetAdventureAll(qData, (reward) =>
		{
			listGainInfo.AddRange(reward);
		},
		() =>
		 {
			 UIManager.Instance.Open<UIFrameItemRewardShot>((str, frame) =>
			 {
				 frame.AddItem(listGainInfo);
			 });

			 RefreshRemainTime();

			 adventureInfo.BlockInteractible(false);
		 });

	}

	// 펫 모험 보상 모두받기
	private void REQ_RewardPetAdventureAll(Queue<ulong> queueAdvId, Action<List<WebNet.ItemInfo>> onReward, Action onRecvAll)
	{
		if (queueAdvId.Count <= 0)
		{
			onRecvAll?.Invoke();
			return;
		}

		var id = queueAdvId.Dequeue();
		var capt = osaAdventure.selectedData;

		List<WebNet.ItemInfo> listGainInfo = new List<WebNet.ItemInfo>();

		ZWebManager.Instance.WebGame.REQ_RewardPetAdventure(id, (recvPacket, recvMsgPacket) =>
		{
			if (recvMsgPacket.AdvInfo.HasValue)
			{
				var data = recvMsgPacket.AdvInfo.Value;

				var osaData = osaAdventure.Data.List.Find(item => item.advData.AdvId == data.AdvId);
				if (osaData != null)
					osaData.advData.Reset(recvMsgPacket.AdvInfo.Value);
			}

			if (recvMsgPacket.GetItems.HasValue)
			{
				listGainInfo.Add(recvMsgPacket.GetItems.Value);
			}

			onReward?.Invoke(listGainInfo);

			if (capt.advData.AdvTid == osaAdventure.selectedData.advData.AdvTid)
			{
				osaAdventure.SetSelectedData(capt);
				OnClickAdventureListItem(capt);
			}

			REQ_RewardPetAdventureAll(queueAdvId, onReward, onRecvAll);
		});
	}

	// 모험 시작
	public void OnClickAdventure()
	{
		var listID = adventureInfo.GetRegistedPetList();

		StartPetAdventure(listID);

	}

	// 중지
	public void OnClickCanelAdventure()
	{
		if (osaAdventure.selectedData == null)
			return;

		UIMessagePopup.ShowPopupOkCancel(DBLocale.GetText("PetAdventure_Stop"), () =>
			 {
				 // 혹시 완료됬을수도있음
				 if (osaAdventure.selectedData.advData.EndDt < TimeManager.NowSec)
				 {
					 UIManager.Instance.Close<UIMessagePopupDefault>(true);
					 UIMessagePopup.ShowPopupOk(DBLocale.GetText("PetAdventur_AlreadyEnd"), () =>
					 {
						 adventureInfo.BlockInteractible(false);
					 });
					 return;
				 }

				 ZWebManager.Instance.WebGame.REQ_CancelPetAdventure(osaAdventure.selectedData.advData.AdvId, (recvPacket, recvMsgPacket) =>
				 {
					 var capt = osaAdventure.selectedData;

					 if (recvMsgPacket.AdvInfo.HasValue)
						 capt.advData.Reset(recvMsgPacket.AdvInfo.Value);

					 if (capt.advData.AdvTid == osaAdventure.selectedData.advData.AdvTid)
					 {
						 osaAdventure.SetSelectedData(capt);
						 OnClickAdventureListItem(capt);
					 }

					 RefreshRemainTime();
				 });
			 }, delegate { });

		adventureInfo.BlockInteractible();
	}

	// 보상받기
	public void OnClickRewardAdventure()
	{
		if (osaAdventure.selectedData == null)
			return;

		ZWebManager.Instance.WebGame.REQ_RewardPetAdventure(osaAdventure.selectedData.advData.AdvId, (recvPacket, recvMsgPacket) =>
		{
			UIManager.Instance.Open<UIFrameItemRewardShot>((str, frame) =>
			{
				frame.AddItem(recvMsgPacket.GetItems);
			});

			var capt = osaAdventure.selectedData;

			if (recvMsgPacket.AdvInfo.HasValue)
				capt.advData.Reset(recvMsgPacket.AdvInfo.Value);

			if (capt.advData.AdvTid == osaAdventure.selectedData.advData.AdvTid)
			{
				osaAdventure.SetSelectedData(capt);
				OnClickAdventureListItem(capt);
			}

			RefreshRemainTime();
		});

		adventureInfo.BlockInteractible();
	}

	// 쿨타임 초기화
	public void OnClickResetCoolTime()
	{
		if (osaAdventure.selectedData == null)
			return;

		var data = osaAdventure.selectedData;

		if (ConditionHelper.CheckCompareCost(data.table.ResetItemID, data.table.ResetItemCnt) == false)
			return;

		UIMessagePopup.ShowCostPopup(DBLocale.GetText("Pet_Adv_Reset_Title"),
									 DBLocale.GetText("Pet_Adv_Reset_Desc"),
									 data.table.ResetItemID,
									 data.table.ResetItemCnt,
									 () =>
									 {
										 // 혹시 쿨타임 끝났을수도있음
										 if (osaAdventure.selectedData.advData.EndDt < TimeManager.NowSec)
										 {
											 UIManager.Instance.Close<UIMessagePopupCost>();
											 UIMessagePopup.ShowPopupOk(DBLocale.GetText("Pet_Adv_Error_Already_CooltimeEnd"), () =>
											 {
												 adventureInfo.BlockInteractible(false);
											 });
											 return;
										 }

										 ZWebManager.Instance.WebGame.REQ_ResetPetAdventure(data.advData.AdvId, data.table.ResetItemID, (recvPacket, recvMsgPacket) =>
										 {
											 var capt = osaAdventure.selectedData;

											 if (recvMsgPacket.AdvInfo.HasValue)
												 capt.advData.Reset(recvMsgPacket.AdvInfo.Value);

											 if (capt.advData.AdvTid == osaAdventure.selectedData.advData.AdvTid)
											 {
												 osaAdventure.SetSelectedData(capt);
												 OnClickAdventureListItem(capt);
											 }

											 RefreshRemainTime();
										 });

									 });
		adventureInfo.BlockInteractible();
	}
}
