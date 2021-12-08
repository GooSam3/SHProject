using GameDB;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ZNet.Data;

// 120/120
// 200/120
public class UIAttendFourWeekListItem : UIAttendOneWeekListItem
{
	[SerializeField] private GameObject objRedDot;
	[SerializeField] private Image imgGrade;

	public override bool SetSlot(AttendEvent_Table eventTable)
	{
		base.SetSlot(eventTable);

		if (this.gameObject.activeSelf == false)
			return false;

		DBItem.GetItem(eventTable.RewardItemID, out var table);
		imgGrade.sprite = table != null ? UICommon.GetGradeSprite(table.Grade) : null;

		txtAttendDay.text = DBLocale.GetText("Day_Text", eventTable.AttendEventNumber);

		return isAttendTarget;
	}

	protected override void SetCheckState(AttendEvent_Table table)
	{
		bool isOwn = Me.CurUserData.GetAttendData(table.GroupID, out var attendData);

		if (isOwn == false)
		{
			objCheck.SetActive(false);
			return;
		}

		// 다음 대상인가
		bool isNextRewardTarget = table.AttendEventNumber == attendData.rewardNormalPos + 1;

		// + 보상을 받은 시간이 오전5시 이전인가 ? => 받을수있음
		bool isRewardReady = false;

		if (isNextRewardTarget)
			isRewardReady = attendData.rewardDt < TimeHelper.GetTodayResetTime();

		isAttendTarget = isNextRewardTarget && isRewardReady;
		objCheck.SetActive(table.AttendEventNumber <= attendData.rewardNormalPos && !isRewardReady);

		objRedDot.SetActive(isAttendTarget);
	}

	protected override void SetAttendDayText(AttendEvent_Table table)
	{
		txtAttendDay.text = DBLocale.GetText("Day_Text", table.AttendEventNumber);
	}
}
