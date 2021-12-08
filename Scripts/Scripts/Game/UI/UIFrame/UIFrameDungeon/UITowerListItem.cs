using GameDB;
using System;
using UnityEngine;

public class UITowerListItem : MonoBehaviour
{
	[SerializeField] private ZText stageNumText;
	[SerializeField] private GameObject selectImage;
	[SerializeField] private GameObject stageLockImage;

	private uint _stageID;
	private Action<uint> selectEvent = null;

	public void DoInit(uint stageID, Action<uint> itemEvent)
	{
		_stageID = stageID;

		gameObject.SetActive(true);

		if (selectEvent == null) {
			selectEvent = itemEvent;
		}
	}

	public void DoUpdate(uint stageTid)
	{
		var stageTable = DBStage.Get(_stageID);
		uint currentLevel = 0;
		
		if(DBStage.TryGet(ZNet.Data.Me.CurCharData.InstanceDungeonStageTID, out Stage_Table stage))
		{
			currentLevel = stage.StageLevel;
		}

		stageNumText.text = DBLocale.GetText(stageTable.StageTextID);

		selectImage.SetActive(_stageID == stageTid);

		bool isLock = true;

		if (ZNet.Data.Me.CurCharData.LastLevel >= stageTable.InMinLevel && currentLevel >= stageTable.StageLevel - 1) {
			isLock = false;
		}

		stageLockImage.SetActive(isLock);

		if (isLock) {
			stageNumText.color = new Color32(142, 142, 142, 125);
		}
		else {
			stageNumText.color = Color.white;
		}
	}

	public void UIListItemClick()
	{
		ZLog.Log(ZLogChannel.Default, $"UIListItemClick {_stageID}");
		if (selectEvent != null) {
			selectEvent(_stageID);
		}
	}

}
