using System;
using UnityEngine;

public class UIColosseumStageItem : MonoBehaviour
{
	[SerializeField] private ZToggle radioButton;
	[SerializeField] private ZText stageText;

	//[SerializeField] private GameObject lockObj;
	//[SerializeField] private ZText timeText;

	private uint _stageID;
	private Action<uint> selectEvent = null;

	public void DoRefresh(uint stageID, Action<uint> itemEvent, bool toggleOn)
	{
		_stageID = stageID;

		gameObject.SetActive(true);

		if (selectEvent == null) {
			selectEvent = itemEvent;
		}

		var stageTable = DBStage.Get(stageID);
		stageText.text = DBLocale.GetText(stageTable.StageTextID);

		if (toggleOn) {
			radioButton.isOn = true;
		}
	}

	public void UIListItemClick()
	{
		radioButton.isOn = true;

		//ZLog.Log(ZLogChannel.Default, $"UIListItemClick {_stageID}");
		//selectEvent?.Invoke(_stageID);
	}
}
