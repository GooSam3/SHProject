using GameDB;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ZNet.Data;

public class UIAttendOneWeekListItem : MonoBehaviour
{
    [SerializeField] protected Text txtAttendDay;

    [SerializeField] protected Image imgRewardItem;

    [SerializeField] protected Text txtRewardItem;
    [SerializeField] protected Text txtRewardCount;

    [SerializeField] protected GameObject objCheck;

    public AttendEvent_Table Data { get; private set; }

    protected bool isAttendTarget = false; // 출석 대상인가?! ( + 출석 받을수있는지 여부까지 )

    /// <summary>
    /// 슬롯 세팅
    /// </summary>
    public virtual bool SetSlot(AttendEvent_Table eventTable)
	{
        Data = eventTable;

        isAttendTarget = false;

        if (eventTable == null)
        {
            this.gameObject.SetActive(false);
            return false;
        }
        else
            this.gameObject.SetActive(true);

        SetAttendDayText(eventTable);
        SetCheckState(eventTable);

        if(DBItem.GetItem(eventTable.RewardItemID, out var table))
		{
            imgRewardItem.sprite = UICommon.GetSprite(table.IconID);
            txtRewardCount.text = eventTable.RewardItemCount.ToString("N0");

            if (txtRewardItem)
                txtRewardItem.text = DBUIResouce.GetItemGradeFormat(DBLocale.GetText(table.ItemTextID),table.Grade);
        }
        else
		{
            imgRewardItem.sprite = null;
            txtRewardCount.text = string.Empty;

            if (txtRewardItem)
                txtRewardItem.text = string.Empty;

            return false;
		}

        return isAttendTarget;
	}

    protected virtual void SetAttendDayText(AttendEvent_Table table)
	{
        txtAttendDay.text = DBLocale.GetText("QuestEvent_Day", table.PurposeDay);
    }

    protected virtual void SetCheckState(AttendEvent_Table table)
	{
        bool isOwn = Me.CurUserData.GetAttendData(table.GroupID, out var attendData);

        if(isOwn == false)
		{
            objCheck.SetActive(false);
            return;
		}

        // 다음 대상인가
        bool isNextRewardTarget = table.AttendEventNumber ==  attendData.rewardNormalPos + 1;

        // + 보상을 받은 시간이 오전5시 이전인가 ? => 받을수있음
        bool isRewardReady = false;

        if(isNextRewardTarget)
            isRewardReady = attendData.rewardDt < TimeHelper.GetTodayResetTime();

        isAttendTarget = isNextRewardTarget && isRewardReady;

        objCheck.SetActive(table.PurposeDay <= attendData.rewardNormalPos && !isRewardReady);
    }

    public virtual void Refresh()
	{
        SetSlot(Data);
    }
}
