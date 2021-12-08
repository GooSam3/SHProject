using GameDB;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ZNet.Data;

public class UIAttendCashDataHolder : ZAdapterHolderBase<EventReward_Table>
{
	private UIAttendCashListItem listItem;
	private Action<EventReward_Table> onClick;

	public override void SetSlot(EventReward_Table data)
	{
		listItem.SetSlot(data);
	}

	public override void CollectViews()
	{
		base.CollectViews();

		listItem = root.GetComponent<UIAttendCashListItem>();
	}

	public void SetAction(Action<EventReward_Table> _onclick)
	{
		onClick = _onclick;
		listItem.SetAction(onClick);
	}
}

/// <summary>
/// 출석 및 레벨업
/// </summary>
public class UIAttendCashListItem : MonoBehaviour
{
	[Serializable]
	private class PassCashRewardGroup
	{
		[Header("CHECK ITEM-CHECK-LOCK COUNT!!, COUNT MUST BE SAME")]

		[SerializeField] private List<UIItemSlot> listItemReward;
		[SerializeField] private List<GameObject> objCheck;
		[SerializeField] private List<GameObject> objLock;

		/// <summary>
		/// 패스 보상 설정
		/// </summary>
		/// <param name="listItemInfo"> list (tid:count) </param>
		/// <param name="hasPass"> 패스 소유중인가? </param>
		/// <param name="rewarded"> 보상 받았는가? </param>
		public void SetSlot((uint, ulong)[] listItemInfo, bool hasPass, bool rewarded)
		{
			for (int i = 0; i < listItemReward.Count; i++)
			{
				if (listItemInfo.Length <= i || listItemInfo[i].Item1 <= 0)// 아이템정보 없음
				{
					listItemReward[i].gameObject.SetActive(false);
					objCheck[i].SetActive(false);
					objLock[i].SetActive(false);
					continue;
				}
				listItemReward[i].gameObject.SetActive(true);

				listItemReward[i].SetItem(listItemInfo[i].Item1, listItemInfo[i].Item2);
				listItemReward[i].SetPostSetting(UIItemSlot.E_Item_PostSetting.LockOff | UIItemSlot.E_Item_PostSetting.ShadowOff);

				objCheck[i].SetActive(rewarded);
				objLock[i].SetActive(!hasPass);
			}
		}
	}


	public enum E_ColumnCountType
	{
		One,
		Two,
	}

	public enum E_SlotDataType // 여차하면 분리
	{
		None,
		Attend,
		LevelUp
	}

	#region # Dynamic Size Change
	[SerializeField, Header("WIDTH, x : one, y : two")] private Vector2 REWARD_NORMAL_SIZE = new Vector2(128f, 256f);
	[SerializeField] private Vector2 REWARD_SPECIAL_SIZE = new Vector2(320f, 256f);
	[SerializeField] private Vector2 REWARD_BUTTON_SIZE = new Vector2(320f, 300f);

	[SerializeField] private RectTransform rtRewardNormal;
	[SerializeField] private RectTransform rtRewardSpecial;
	[SerializeField] private RectTransform rtButton;
	[SerializeField] private ZButton btnConfirmOne;
	[SerializeField] private ZButton btnConfirmTwo;
	[SerializeField] private GameObject objSeconcdColumn;
	private void SetSlotByType(E_ColumnCountType type)
	{
		countType = type;

		rtRewardNormal.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,
			type == E_ColumnCountType.One ? REWARD_NORMAL_SIZE.x : REWARD_NORMAL_SIZE.y);

		rtRewardSpecial.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,
			type == E_ColumnCountType.One ? REWARD_SPECIAL_SIZE.x : REWARD_SPECIAL_SIZE.y);

		rtButton.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,
			type == E_ColumnCountType.One ? REWARD_BUTTON_SIZE.x : REWARD_BUTTON_SIZE.y);

		btnConfirmOne.gameObject.SetActive(type == E_ColumnCountType.One);
		btnConfirmTwo.gameObject.SetActive(type == E_ColumnCountType.Two);
		objSeconcdColumn.SetActive(type == E_ColumnCountType.Two);
	}

	#endregion Dynamic Size Change #

	private const float ALPHA_ON = 1f;
	private const float ALPHA_OFF = .4f;//아직도달못함

	[SerializeField] private Text txtTitle;                             // 슬롯 타이틀 ( -일차, 레벨-달성 )

	[SerializeField] private PassCashRewardGroup RewardNormal;          // 기본 보상

	[SerializeField] private PassCashRewardGroup RewardPassOne;         // 패스1 보상
	[SerializeField] private PassCashRewardGroup RewardPassTwo;         // 패스2 보상

	[SerializeField] private Text txtConfirmOne;
	[SerializeField] private Text txtConfirmTwo;

	[SerializeField] private GameObject objFocus;

	[SerializeField] private CanvasGroup canvasGroup;

	private E_ColumnCountType countType;

	private Action<EventReward_Table> onClickReward;
	private EventReward_Table data;

	public void SetSlot(EventReward_Table _data)
	{
		data = _data;

		SetSlotByType(data.Two_Pass_ItemID_1 > 0 ? E_ColumnCountType.Two : E_ColumnCountType.One);

		Me.CurUserData.GetAttendData(data.RewardGroupID, out var attendData);

		var dataType = SetDataByType(_data.EventType, attendData);

		if (dataType == E_SlotDataType.None)
		{
			ZLog.Log(ZLogChannel.Event, $"레벨업, 출석이 아닌 데이터 들어옴  : {dataType}, groupId : {_data.RewardGroupID}");
			return;
		}

		// -- 보상설정
		RewardNormal.SetSlot(new (uint, ulong)[] { (data.No_Pass_ItemID_1, data.No_Pass_ItemCount_1),
												   (data.No_Pass_ItemID_2, data.No_Pass_ItemCount_2) },
												   true, data.TypeCount <= attendData.rewardNormalPos);


		RewardPassOne.SetSlot(new (uint, ulong)[] { (data.One_Pass_ItemID_1, data.One_Pass_ItemCount_1),
													(data.One_Pass_ItemID_2, data.One_Pass_ItemCount_2) },
													attendData.isOwnPassOne, data.TypeCount <= attendData.rewardOnePos);

		if (countType == E_ColumnCountType.Two)
		{
			RewardPassTwo.SetSlot(new (uint, ulong)[] { (data.Two_Pass_ItemID_1, data.Two_Pass_ItemCount_1),
														(data.Two_Pass_ItemID_2, data.Two_Pass_ItemCount_2) },
														attendData.isOwnPassTwo, data.TypeCount <= attendData.rewardTwoPos);
		}
	}

	public void SetAction(Action<EventReward_Table> _onClick)
	{
		onClickReward = _onClick;
	}

	private E_SlotDataType SetDataByType(E_ServerEventSubCategory category, ZDefine.AttendEventData attendData)
	{
		var dataType = E_SlotDataType.None;
		bool focusState = false;

		SetButtonState(false, attendData);

		switch (data.EventType)
		{
			case E_ServerEventSubCategory.AttendEventPaidDaily:
				dataType = E_SlotDataType.Attend;

				focusState = data.TypeCount == attendData.attendPos;

				canvasGroup.alpha = data.TypeCount > attendData.attendPos ? ALPHA_OFF : ALPHA_ON;

				break;

			case E_ServerEventSubCategory.AttendEventPaidLevelUp:
				dataType = E_SlotDataType.LevelUp;

				var eventTargetTable = DBIngameEvent.GetCashEventTargetLevel(data.RewardGroupID);

				focusState = eventTargetTable?.RewardID == data.RewardID;

				canvasGroup.alpha = !focusState && Me.CurCharData.Level < data.TypeCount ? ALPHA_OFF : ALPHA_ON;
				
				if (focusState && eventTargetTable?.TypeCount > Me.CurCharData.Level)
					focusState = false;

				break;
			default:
				return dataType;
		}

		objFocus.SetActive(focusState);
		SetButtonState(focusState, attendData);

		string strTitle = dataType == E_SlotDataType.Attend ? "Day_Text" : "Event_Cash_LevelUp_Title";
		txtTitle.text = DBLocale.GetText(strTitle, data.TypeCount);

		return dataType;
	}

	private void SetButtonState(bool focusState, ZDefine.AttendEventData attendData)
	{
		if (focusState == false)
		{
			btnConfirmOne.gameObject.SetActive(false);
			btnConfirmTwo.gameObject.SetActive(false);
			return;
		}
		string confirmTxt = DBLocale.GetText("Event_Cash_Reward_Normal");

		bool isRewadable = attendData.rewardNormalPos < data.TypeCount;
		bool isRemainPass = false; // 시즌패스 계속받기 체크용
		 // 보상을 받았을수있음 (남은게 시즌패스뿐일수도)

		if (attendData.isOwnPassOne == false || attendData.isOwnPassTwo == false)
		{// 보상을 받았는데 시즌패스1,2를 안샀다? 계속받기~

			if (isRewadable == false)
			{
				confirmTxt = DBLocale.GetText("Event_Cash_Reward_Continue");
				isRemainPass = true;
			}
		}

		switch (countType)
		{
			case E_ColumnCountType.One:
				btnConfirmOne.gameObject.SetActive(isRewadable || isRemainPass);
				txtConfirmOne.text = confirmTxt;
				break;
			case E_ColumnCountType.Two:
				btnConfirmTwo.gameObject.SetActive(isRewadable || isRemainPass);
				txtConfirmTwo.text = confirmTxt;
				break;
		}
	}

	public void OnClickReward()
	{
		onClickReward?.Invoke(data);
	}

#if UNITY_EDITOR

	bool b;
	[ContextMenu("SWITCH")]
	public void A()
	{
		b = !b;
		SetSlotByType(b ? E_ColumnCountType.One : E_ColumnCountType.Two);
	}
#endif
}
