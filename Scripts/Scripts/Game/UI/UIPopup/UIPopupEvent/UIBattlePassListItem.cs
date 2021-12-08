using GameDB;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ZNet.Data;

public class UIBattlePassDataHolder : ZAdapterHolderBase<QuestEvent_Table>
{
	private UIBattlePassListItem listItem;
	private Action<QuestEvent_Table> onClick;

	public override void SetSlot(QuestEvent_Table data)
	{
		listItem.SetSlot(data);
	}

	public override void CollectViews()
	{
		base.CollectViews();

		listItem = root.GetComponent<UIBattlePassListItem>();
	}

	public void SetAction(Action<QuestEvent_Table> _onclick)
	{
		onClick = _onclick;
		listItem.SetAction(onClick);
	}
}

public class UIBattlePassListItem : MonoBehaviour
{
	[SerializeField, Header("Progress Hex Color")] private string HEX_INPROGRESS = "#FFFFFF";
	[SerializeField] private string HEX_REWARD = "#64DAFF";
	[SerializeField] private string HEX_COMPLETE = "#707480";

	[SerializeField, Space(10)] private Text txtTitle;
	[SerializeField] private ZToggle toggleTitle;

	[SerializeField] private Text txtProgress;

	[SerializeField] private ZButton btnConfirm;
	[SerializeField] private Text txtConfirm;

	[SerializeField] private GameObject objRedDot;

	private QuestEvent_Table data;
	private Action<QuestEvent_Table> onClickConfirm;

	public void SetSlot(QuestEvent_Table _data)
	{
		data = _data;
		var myData = Me.CurCharData.GetEventData(_data.GroupID, _data.EventQuestID);

		int curValue = (int)(myData?.Value ?? 0);
		int targetValue = (int)_data.TargetCount;
		string progress = UICommon.GetProgressText(curValue, targetValue, false);

		txtTitle.text = DBLocale.GetText(_data.EventQuestTextID);

		toggleTitle.interactable = true;
		btnConfirm.interactable = false;
		objRedDot.SetActive(false);

		if (myData != null)
		{
			string hex = string.Empty;
			switch (myData.State)
			{
				case WebNet.E_QuestState.None:
					hex = HEX_INPROGRESS;
					txtConfirm.text = DBLocale.GetText("Event_BattlePass_State_Progress");
					break;
				case WebNet.E_QuestState.Reward:
					hex = HEX_REWARD;
					btnConfirm.interactable = true;
					txtConfirm.text = DBLocale.GetText("Event_BattlePass_State_Reward");
					objRedDot.SetActive(true);	
					break;
				case WebNet.E_QuestState.Complete:
					hex = HEX_COMPLETE;
					toggleTitle.interactable = false;
					txtConfirm.text = DBLocale.GetText("Event_BattlePass_State_Completed");
					break;
			}

			txtProgress.text = UICommon.GetColoredText(hex, progress);
		}
		else
		{
			txtProgress.text = UICommon.GetColoredText(HEX_INPROGRESS, progress);
		}
	}

	public void SetAction(Action<QuestEvent_Table> _onClick)
	{
		onClickConfirm = _onClick;
	}

	public void OnClickConfirm()
	{
		onClickConfirm?.Invoke(data);
	}
}
