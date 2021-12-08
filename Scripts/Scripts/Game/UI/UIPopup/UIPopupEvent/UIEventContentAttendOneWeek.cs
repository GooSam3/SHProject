using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ZNet.Data;

public class UIEventContentAttendOneWeek : UIEventContentBase
{
	[SerializeField] private List<UIAttendOneWeekListItem> listAttendItemSlot;

	[SerializeField] private GameObject objCurAttendDay;
	[SerializeField] private Text txtCurAttendDay;

	[SerializeField] private Text txtEventTimeRange;
	[SerializeField] private Text txtNotice;

	private IngameEventInfoConvert inGameEventData;

	private UIAttendOneWeekListItem attendTargetSlot;

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
		
		bool showCurAttend = DBIngameEvent.IsViewAttendCount(_eventData.groupId);

		objCurAttendDay.SetActive(showCurAttend);
		
		if(showCurAttend)
			txtCurAttendDay.text = $"출석 : {attendData.rewardNormalPos}일";

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

		if (attendTargetSlot != null)
		{
			var data = attendTargetSlot.Data;
			if (Me.CurUserData.GetAttendData(data.GroupID, out var AttendEventData) == false)
				return true;

			var a = TimeHelper.ParseTimeStamp((long)AttendEventData.rewardDt);

			ZWebManager.Instance.WebGame.REQ_GetAttendEventReward(AttendEventData.mainType, data.GroupID, true,(recvPacket, recvMsgPacket) =>
			{
				attendTargetSlot.Refresh();

				if(objCurAttendDay.activeSelf)
					txtCurAttendDay.text = $"출석 : {attendData.rewardNormalPos}일";

			});
		}

		return true;
	}

	protected override void ReleaseContent()
	{
	}
}
