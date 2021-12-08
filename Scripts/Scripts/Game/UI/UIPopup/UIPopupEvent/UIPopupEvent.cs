using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZNet.Data;

/// <summary>
/// 이벤트 리스트 관리, 꺼지면 관련컨텐츠 모두제거
/// </summary>
public class UIPopupEvent : ZUIFrameBase
{
	/// <summary>
	/// 서브ui 컨텐츠 타입
	/// </summary>
	private enum E_ContentUIType
	{
		AttendFourWeek, // 출석 28
		AttendOneWeek,  // 출석 7 
		AttendCash,     // 출석(레벨업) 유료
		BannerOnly,     // 핫타임
		BattlePass,     // 배틀패스
		Gathering,      // 수집
	}

	[SerializeField] private UIEventViewListAdapter osaEventList;

	[SerializeField] private RectTransform contentRoot;

	[SerializeField] private GameObject objLocalIndicator;

	private List<OSA_UIEventData> listEvent = new List<OSA_UIEventData>();

	private Dictionary<E_ContentUIType, UIEventContentBase> dicContent = new Dictionary<E_ContentUIType, UIEventContentBase>();

	public void InitEvent(List<OSA_UIEventData> data)
	{
		dicContent.Clear();
		listEvent.Clear();

		listEvent.AddRange(data);

		ZPoolManager.Instance.Spawn(E_PoolType.UI, nameof(UIEventViewListItem), delegate
		{
			osaEventList.Initialize(OnClickEventSlot);
			osaEventList.ResetListData(listEvent);

			if (listEvent.Count > 0)
				osaEventList.SelectFirst();// onclickeventslot 호출됨
		});
	}


	public void OnClickClose()
	{
		Release();
		UIManager.Instance.Close<UIPopupEvent>(true);
	}

	public void Release()
	{
		foreach (var iter in dicContent.Values)
			iter.Release();

		dicContent.Clear();
	}

	private void OnClickEventSlot(OSA_UIEventData _data)
	{
		objLocalIndicator.SetActive(true);

		foreach (var iter in dicContent.Values)
			iter.Close();

		switch (_data.eventData.SubCategory)
		{
			case WebNet.E_ServerEventSubCategory.BattlePass:            //------->> 배틀패스
				SetContent(E_ContentUIType.BattlePass, _data.eventData);
				break;


			case WebNet.E_ServerEventSubCategory.GoldUpEvent:           //------->> 배너
			case WebNet.E_ServerEventSubCategory.ExpUpEvent:
			case WebNet.E_ServerEventSubCategory.ItemDropRateUpEvent:
				SetContent(E_ContentUIType.BannerOnly, _data.eventData);
				break;

			case WebNet.E_ServerEventSubCategory.AttendEvent:           //------->>출석 관련
			case WebNet.E_ServerEventSubCategory.AttendEventNewUser:
			case WebNet.E_ServerEventSubCategory.AttendEventComeback:
			case WebNet.E_ServerEventSubCategory.AttendEventComulative:
			case WebNet.E_ServerEventSubCategory.AttendEventContiniuity:
				SetAttendContent(_data);
				break;

			case WebNet.E_ServerEventSubCategory.AttendEventPaidDaily:
			case WebNet.E_ServerEventSubCategory.AttendEventPaidLevelUp:
				SetContent(E_ContentUIType.AttendCash, _data.eventData);
				break;

			case WebNet.E_ServerEventSubCategory.Collect:
				SetContent(E_ContentUIType.Gathering, _data.eventData);
				break;

			case WebNet.E_ServerEventSubCategory.EventDungeon:          //------>> 안쓰는놈들 가시화
			case WebNet.E_ServerEventSubCategory.QuestEvent:
			case WebNet.E_ServerEventSubCategory.BannerPopUp:           //- 배너 관련
			case WebNet.E_ServerEventSubCategory.Colosseum:
			case WebNet.E_ServerEventSubCategory.ScenarioDungeon:
			case WebNet.E_ServerEventSubCategory.BlackMarket:           //- hud단에서 예외처리해서 안들어옴
			case WebNet.E_ServerEventSubCategory.EventPopup:
			case WebNet.E_ServerEventSubCategory.Max:
			case WebNet.E_ServerEventSubCategory.ItemDropEvent:
			default:
				ZLog.Log(ZLogChannel.Event, $"등록안된 이벤트 {_data.eventData.SubCategory}, {_data.eventData.groupId}");
				/// 등록되지 않은 이벤트 ㅇ{외처리
				break;
		}
	}

	private void SetContent(E_ContentUIType _type, IngameEventInfoConvert _data)
	{
		if (_data == null)// 데이터 없음
		{
			ZLog.Log(ZLogChannel.Event, $"데이터가 음슴 type : {_type}");

			return;
		}

		if (dicContent.TryGetValue(_type, out var content))
		{
			content.SetContent(_data, delegate
			{
				objLocalIndicator.SetActive(false);
			});
			return;
		}

		string addressableKey = string.Empty;

		switch (_type)
		{
			case E_ContentUIType.AttendFourWeek:
				addressableKey = nameof(UIEventContentAttendFourWeek);
				break;
			case E_ContentUIType.AttendOneWeek:
				addressableKey = nameof(UIEventContentAttendOneWeek);
				break;
			case E_ContentUIType.AttendCash:
				addressableKey = nameof(UIEventContentAttendCash);
				break;
			case E_ContentUIType.BannerOnly:
				addressableKey = nameof(UIEventContentBannerOnly);
				break;
			case E_ContentUIType.BattlePass:
				addressableKey = nameof(UIEventContentBattlePass);
				break;
			case E_ContentUIType.Gathering:
				addressableKey = nameof(UIEventContentGathering);
				break;
		}

		if (string.IsNullOrEmpty(addressableKey))
		{
			ZLog.Log(ZLogChannel.Event, $"어드레서블 음슴 type : {_type}");

			return;
		}

		ZPoolManager.Instance.Spawn(E_PoolType.UI, addressableKey, obj =>
		 {
			 if (obj == null)
			 {
				 ZLog.Log(ZLogChannel.Event, $"로드되긴했는데 오브젝트가 없음 key : {addressableKey}");

				 return;
			 }

			 if (dicContent.ContainsKey(_type))
			 {
				 ZPoolManager.Instance.Return(obj);
				 ZLog.Log(ZLogChannel.Event, $"있는놈 로드로인해 반환함 key : {addressableKey}");

				 return;
			 }

			 var rt = obj.GetComponent<RectTransform>();
			 rt.SetParent(contentRoot);
			 rt.SetLocalTRS(Vector3.zero, Quaternion.identity, Vector3.one);
			 rt.SetAnchor(AnchorPresets.StretchAll, 0, 0);
			 rt.offsetMin = Vector2.zero;
			 rt.offsetMax = Vector2.zero;

			 content = obj.GetComponent<UIEventContentBase>();
			 content.Close();

			 dicContent.Add(_type, content);

			 if (osaEventList.selectedData.eventData.groupId == _data.groupId)
			 {
				 content.SetContent(_data, delegate
				 {
					 objLocalIndicator.SetActive(false);
				 });
			 }
		 });
	}

	private void SetAttendContent(OSA_UIEventData _data)
	{
		// todo_ljh : 시간체크

		E_ContentUIType type = E_ContentUIType.AttendOneWeek;

		if (Me.CurUserData.GetAttendData(_data.eventData.groupId, out var attendData) == false)
		{
			ZLog.Log(ZLogChannel.Event, $"테이블 없음 attendtable {_data.eventData.groupId}");
		}
		else
		{
			if (DBIngameEvent.GetAttendGroupDataFirst(attendData.groupId, out var table))
			{
				switch (table.AttendBoardType)
				{
					case GameDB.E_AttendBoardType.OneWeek:
						type = E_ContentUIType.AttendOneWeek;
						break;
					case GameDB.E_AttendBoardType.FourWeek:
						type = E_ContentUIType.AttendFourWeek;
						break;
				}
			}
		}

		SetContent(type, _data.eventData);
	}
}
