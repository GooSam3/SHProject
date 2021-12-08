using GameDB;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ZNet.Data;

//레벨업이 올수도있음
public class UIEventContentAttendCash : UIEventContentBase
{
	[Serializable]
	private class ContentSlotClassify
	{
		public enum E_ClassifyType
		{
			One,
			Two,
		}

		// one, two
		[SerializeField, Header("WIDTH, x : one, y : two")] private Vector2 REWARD_NORMAL_SIZE = new Vector2(256f, 128f);
		[SerializeField] private Vector2 REWARD_SPECIAL_SIZE = new Vector2(320f, 256f);
		[SerializeField] private Vector2 REWARD_TEXT_SIZE = new Vector2(200f, 168f);

		[SerializeField, Space(10)] private RectTransform rtRewardNormal;  // 출석보상 사이즈 변경용
		[SerializeField] private RectTransform rtRewardSpecial; // 스페셜보상 사이즈 변경용
		[SerializeField] private RectTransform rtRewardText;    // 스페셜보상 텍스트 사이즈 변경용

		[SerializeField] private GameObject objSecondColumn;

		public void SetClassify(E_ClassifyType type)
		{
			rtRewardNormal.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,
				type == E_ClassifyType.One ? REWARD_NORMAL_SIZE.x : REWARD_NORMAL_SIZE.y);
			rtRewardSpecial.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,
				type == E_ClassifyType.One ? REWARD_SPECIAL_SIZE.x : REWARD_SPECIAL_SIZE.y);
			rtRewardText.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,
				type == E_ClassifyType.One ? REWARD_TEXT_SIZE.x : REWARD_TEXT_SIZE.y);

			objSecondColumn.SetActive(type == E_ClassifyType.Two);
		}
	}

	[SerializeField] private Text txtEventTimeRange;                    // 이벤트 기간
	[SerializeField] private Text txtEventRewardRange;                  // 보상 수령 기간

	[SerializeField] private GameObject objLockPassOne;
	[SerializeField] private GameObject objLockPassTwo;

	[SerializeField] private ContentSlotClassify contentClassify;       // 첫번째 열 ( 분류 메뉴..? )
	[SerializeField] private UIAttendCashListAdapter osaAttendList;     // osa

	private List<EventReward_Table> listAttendEvent = new List<EventReward_Table>(); // 두고보자 

	private IngameEventInfoConvert eventData;

	protected override bool SetContent(IngameEventInfoConvert _eventData)
	{
		eventData = _eventData;

		listAttendEvent.Clear();

		if (DBIngameEvent.GetCashEventDataGroup(_eventData.groupId, out var list) == false)
		{
			ZLog.Log(ZLogChannel.Event, $"attend table 리스트 없음 key : {_eventData.groupId}");
			return false;
		}

		if (Me.CurUserData.GetAttendData(_eventData.groupId, out var attendData) == false)
		{
			ZLog.Log(ZLogChannel.Event, $"서버에서 초기화된 출석데이터 없음 : {_eventData.groupId}");
			return false;
		}

		if (list.Count > 0)
			contentClassify.SetClassify(list[0].Two_Pass_ItemID_1 > 0 ?
										ContentSlotClassify.E_ClassifyType.Two :
										ContentSlotClassify.E_ClassifyType.One);

		objLockPassOne.SetActive(!attendData.isOwnPassOne);
		objLockPassTwo.SetActive(!attendData.isOwnPassTwo);

		listAttendEvent.AddRange(list);

		//-- 컬럼타입 설정

		if (osaAttendList.IsInitialized == false)
		{
			ZPoolManager.Instance.Spawn(E_PoolType.UI, nameof(UIAttendCashListItem), delegate
			{
				osaAttendList.Initialize(OnClickReward);
				osaAttendList.ResetListData(listAttendEvent);

				IsLoadSuccess = true;
			});

			return false;
		}
		else
		{
			osaAttendList.ResetListData(listAttendEvent);
		}
		return true;
	}

	protected override void ReleaseContent()
	{
	}

	private void OnClickReward(EventReward_Table data)
	{
		if (Me.CurUserData.GetAttendData(data.RewardGroupID, out var attendData) == false)
		{
			ZLog.Log(ZLogChannel.Event, $"서버에서 초기화된 출석데이터 없음 : {data.RewardGroupID}");
			return;
		}

		// 한번더 체크

		// 보상받을수있는가?
		bool isRewadable = attendData.rewardNormalPos <= data.TypeCount || !attendData.isOwnPassOne|| !attendData.isOwnPassTwo;

		if (isRewadable)
		{
			// 이미 일반보상은 받음
			if (attendData.rewardNormalPos >= data.TypeCount)
			{
				uint passId = 0;

				if (attendData.isOwnPassOne == false)
				{//스상1~
					passId = GetPassTid(true);

					if (passId == 0)
					{
						ZLog.Log(ZLogChannel.Event, $"시즌패스1 아이디 재확인!!, : {eventData.groupId}");
						return;
					}

				}
				else if (attendData.isOwnPassTwo == false)
				{//스상2~
					passId = GetPassTid(false);

					if (passId == 0)
					{
						ZLog.Log(ZLogChannel.Event, $"시즌패스2 아이디 재확인!!, : {eventData.groupId}");
						return;
					}
				}
				else
				{
					ZLog.Log(ZLogChannel.Event, $"받을 보상 없는데 눌리면안됨: {data.RewardGroupID}");
					return;
				}
				UIManager.Instance.Close<UIPopupEvent>(true);
				UIManager.Instance.Open<UIFrameSpecialShop>((str, popup) =>
				{
					popup.SetPostInitAction(() => popup.ShortCut(passId, true));
				});
			}
			else
			{// 일반보상~ 더주면좋고
				ZWebManager.Instance.WebGame.REQ_GetAttendEventReward(WebNet.E_ATTEND_TYPE.PAID_ATTEND, attendData.groupId, true, delegate
				{
					osaAttendList.Refresh();
				});
			}
		}
		else
		{
			ZLog.Log(ZLogChannel.Event, $"받을 보상 없는데 눌리면안됨: {data.RewardGroupID}");
			return;
		}
	}

	private uint GetPassTid(bool isSeasonOne)
	{
		uint passId = 0;

		if (eventData.Args != null)
		{
			if (isSeasonOne)
			{
				passId = eventData.Args["pass_1"]?.Make<uint>() ?? 0;
			}
			else
			{
				passId = eventData.Args["pass_2"]?.Make<uint>() ?? 0;
			}
		}

		return passId;
	}

#if UNITY_EDITOR

	bool b;
	[ContextMenu("SWITCH")]
	public void A()
	{
		b = !b;
		contentClassify.SetClassify(b ? ContentSlotClassify.E_ClassifyType.One : ContentSlotClassify.E_ClassifyType.Two);
	}
#endif

}
