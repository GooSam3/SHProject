using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ZNet.Data;

public class UIEventContentAttendFourWeek : UIEventContentBase
{
	[SerializeField] private List<UIAttendFourWeekListItem> listAttendItemSlot;

	[SerializeField] private Text txtNotice;

	private IngameEventInfoConvert inGameEventData;

	private UIAttendFourWeekListItem attendTargetSlot;

	public override void Open()
	{
		base.Open();

		Me.CurUserData.OnUpdateAttendEvent -= OnUpdateAttendEvent;
		Me.CurUserData.OnUpdateAttendEvent += OnUpdateAttendEvent;
	}

	public override void Close()
	{
		Me.CurUserData.OnUpdateAttendEvent -= OnUpdateAttendEvent;

		base.Close();
	}

	private void OnUpdateAttendEvent(ZDefine.AttendEventData data)
	{
		if (inGameEventData?.groupId == data.groupId)
			listAttendItemSlot.ForEach(item => item.Refresh());
	}

	protected override bool SetContent(IngameEventInfoConvert _eventData)
	{
		inGameEventData = _eventData;

		if (DBIngameEvent.GetAttendDataGroup(inGameEventData.groupId, out var list) == false)
		{
			ZLog.Log(ZLogChannel.Event, $"attend table 리스트 없음 key : {inGameEventData.groupId}");
			return false;
		}

		if (Me.CurUserData.GetAttendData(inGameEventData.groupId, out var attendData) == false)
		{
			ZLog.Log(ZLogChannel.Event, $"서버에서 초기화된 출석데이터 없음 : {inGameEventData.groupId}");
			return false;
		}

		attendTargetSlot = null;

		for (int i = 0; i < listAttendItemSlot.Count; i++)
		{
			if (list.Count <= i)
			{
				listAttendItemSlot[i].SetSlot(null);
				continue;
			}

			var data = list[i];

			if (listAttendItemSlot[i].SetSlot(data))
				attendTargetSlot = listAttendItemSlot[i];
		}

		txtNotice.text = DBLocale.GetText("DailyAttend_Notice_Txt");

		if (attendTargetSlot != null)
		{
			var data = attendTargetSlot.Data;
			if (Me.CurUserData.GetAttendData(data.GroupID, out var AttendEventData) == false)
				return true;

			var a = TimeHelper.ParseTimeStamp((long)AttendEventData.rewardDt);

			ZWebManager.Instance.WebGame.REQ_GetAttendEventReward(AttendEventData.mainType, data.GroupID, true, (recvPacket, recvMsgPacket) =>
			{
				attendTargetSlot.Refresh();
			});
		}

		return true;
	}

	protected override void ReleaseContent()
	{
	}
}
