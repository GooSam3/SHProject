using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZNet.Data;
using UnityEngine.UI;

// 오늘에 대한것만 저장
public class BlackMarketData
{
	public IngameEventInfoConvert EventData { get; private set; }

	public ulong StartDt { get; private set; }
	public ulong EndDt { get; private set; }
	public uint GroupId { get; private set; }

	public bool IsOpen => TimeManager.NowSec > StartDt && TimeManager.NowSec < EndDt;
	public BlackMarketData(IngameEventInfoConvert _data)
	{
		Reset(_data);
	}

	public void Reset(IngameEventInfoConvert _data)
	{
		EventData = _data;
		StartDt = 0;
		EndDt = 0;
		GroupId = 0;

		if (EventData.Args != null)
		{
			string dayIdx = ((int)DateTime.Today.DayOfWeek).ToString();

			if (EventData.Args[dayIdx] != null)
			{
				if (EventData.Args[dayIdx] is TinyJSON.ProxyArray dayInfo)
				{
					var todayStartTime = TimeHelper.GetTodayStartTime();

					if (dayInfo.Count <= 0)
						return;

					StartDt = (dayInfo[0]["start_sec"]?.Make<ulong>() ?? 0) + todayStartTime;
					EndDt = (dayInfo[0]["end_sec"]?.Make<ulong>() ?? 0) + todayStartTime;
					GroupId = dayInfo[0]["group_id"]?.Make<uint>() ?? 0;
				}
			}
		}
	}

	public string GetRemainString()
	{
		var span = TimeHelper.GetCompareTimeSpan(EndDt);
		return span.ToString(@"hh\:mm\:ss");
	}
}

public class UISubHUDEvent : ZUIFrameBase
{

	[SerializeField] private ZButton btnEvent;
	[SerializeField] private ZButton btnBlackMarket;
	[SerializeField] private Text txtBlackMarketRemainTime;

	// 이벤트 갱신을 위한 데이터 여기에 저장, netdata에 저장하기엔 휘발성이 강함
	private List<OSA_UIEventData> listEvent = new List<OSA_UIEventData>();

	private BlackMarketData blackMarketData = null;

	// TEST

	public bool OpenEventRewardAlways = false;

	// hud가 초기화될때 한번 데이터 저장, 그 후 이벤트 패널이 켜질때마다 데이터 갱신
	protected override void OnInitialize()
	{
		base.OnInitialize();

		// 접속시 한번만 들어오면됨
		ZPawnManager.Instance.DoAddEventSceneLoaded(OnGameSceneLoaded);
	}

	protected override void OnShow(int _LayerOrder)
	{
		base.OnShow(_LayerOrder);

		InvokeRepeating(nameof(RefreshRemainTime), 0f, 1f);
	}

	protected override void OnHide()
	{
		ZPawnManager.Instance.DoRemoveEventSceneLoaded(OnGameSceneLoaded);

		CancelInvoke();

		base.OnHide();
	}

	protected override void OnRemove()
	{
		ZPawnManager.Instance.DoRemoveEventSceneLoaded(OnGameSceneLoaded);

		CancelInvoke();

		base.OnRemove();
	}

	private void OnGameSceneLoaded()
	{
		// 여기선 이벤트 리스트 저장 후 레드닷만 검사
		RefreshEventList(false);

		if (Me.CurUserData.GetLoginEvent()?.isHaveMail ?? false || OpenEventRewardAlways)
		{
			UIManager.Instance.Open<UIPopupEventReward>();
		}
	}

	private void RefreshRemainTime()
	{
		if (blackMarketData != null)
		{
			var isOpen = blackMarketData.IsOpen;

			if (btnBlackMarket.gameObject.activeSelf != isOpen)
				btnBlackMarket.gameObject.SetActive(isOpen);

			txtBlackMarketRemainTime.text = blackMarketData.GetRemainString();
		}
	}

	public void RefreshEventList(bool forceRefresh, Action onRecv = null)
	{
		UIManager.Instance.ShowGlobalIndicator(true);
		btnEvent.interactable = false;
		btnBlackMarket.interactable = false;

		Me.CurCharData.ServerEventContainer.REQ_GetServerEventList((_listEvent) =>
		{
			listEvent.Clear();
			blackMarketData = null;

			foreach (var iter in _listEvent)
			{
				if (iter.Category != WebNet.E_ServerEventCategory.Event)
					continue;

				if ((iter is IngameEventInfoConvert eventData) == false)
					continue;

				eventData = iter as IngameEventInfoConvert;

				switch (eventData.SubCategory)
				{
					case WebNet.E_ServerEventSubCategory.BattlePass:            //------->> 배틀패스
						break;

					case WebNet.E_ServerEventSubCategory.GoldUpEvent:           //------->> 배너
					case WebNet.E_ServerEventSubCategory.ExpUpEvent:
					case WebNet.E_ServerEventSubCategory.ItemDropRateUpEvent:

						break;

					case WebNet.E_ServerEventSubCategory.Collect:               //------->> 수집
						{
							if (eventData.StartDt > TimeManager.NowSec || eventData.EndDt < TimeManager.NowSec)
								continue;
						}

						break;

					case WebNet.E_ServerEventSubCategory.AttendEvent:           //------->>출석 관련
					case WebNet.E_ServerEventSubCategory.AttendEventNewUser:
					case WebNet.E_ServerEventSubCategory.AttendEventComeback:
					case WebNet.E_ServerEventSubCategory.AttendEventComulative:
					case WebNet.E_ServerEventSubCategory.AttendEventContiniuity:
					case WebNet.E_ServerEventSubCategory.AttendEventPaidDaily:
					case WebNet.E_ServerEventSubCategory.AttendEventPaidLevelUp:
						{
							if (Me.CurUserData.GetAttendData(eventData.groupId, out var attData) == false)
								continue;
						}
						break;

					case WebNet.E_ServerEventSubCategory.BlackMarket:           //- hud단에서 예외처리해서 안들어옴
						if (blackMarketData == null)
							blackMarketData = new BlackMarketData(eventData);
						else
							blackMarketData.Reset(eventData);
						continue;
					case WebNet.E_ServerEventSubCategory.BannerPopUp:           //- 배너 관련
						continue;

					case WebNet.E_ServerEventSubCategory.EventDungeon:          //------>> 안쓰는놈들 가시화
					case WebNet.E_ServerEventSubCategory.QuestEvent:
					case WebNet.E_ServerEventSubCategory.Colosseum:
					case WebNet.E_ServerEventSubCategory.ScenarioDungeon:
					case WebNet.E_ServerEventSubCategory.EventPopup:
					case WebNet.E_ServerEventSubCategory.Max:
					case WebNet.E_ServerEventSubCategory.ItemDropEvent:
					default:
						continue;
				}

				listEvent.Add(new OSA_UIEventData(eventData));
			}

			btnBlackMarket.gameObject.SetActive(blackMarketData?.IsOpen ?? false);

			RefreshRemainTime();

			btnEvent.gameObject.SetActive(listEvent.Count > 0);

			btnEvent.interactable = true;
			btnBlackMarket.interactable = true;

			UIManager.Instance.ShowGlobalIndicator(false);

			onRecv?.Invoke();
		}, forceRefresh);
	}

	public void OnClickEvent()
	{
		// 이벤트 갱신
		RefreshEventList(true, () =>
		{
			// 구매제한 갱신 후 들어감
			ZWebManager.Instance.WebGame.REQ_GetBuyLimitList(delegate
			{
				UIManager.Instance.Open<UIPopupEvent>((str, popup) =>
				{
					popup.InitEvent(listEvent);
				});
			});
		});
	}

	public void OnClickBlackMarket()
	{
		ZWebManager.Instance.WebGame.REQ_GetBuyLimitList(delegate
		{
			// 구매제한 갱신 후 들어감
			UIManager.Instance.Open<UIFrameBlackMarket>((str, popup) =>
			{
				popup.SetMarketData(blackMarketData);
			});
		});
	}
}
