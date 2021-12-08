using GameDB;
using System.Collections.Generic;
using UnityEngine;
using uTools;
using ZNet.Data;

public sealed class UISubHUDGodLand : ZUIFrameBase
{
	[SerializeField] private UISubHUDGodLandItem myItem;
	[SerializeField] private UISubHUDGodLandItem enemyItem;
	[SerializeField] private UIPopupGodLandResult resultPopup;

	[SerializeField] private ZText remainderTime;

	private bool isTimerStart;
	private float secondCheckTimer;
	private ulong endServerTime;

	protected override void OnShow(int _LayerOrder)
	{
		ZLog.Log(ZLogChannel.Default, "UISubHUDGodLand.OnShow()");

		base.OnShow(_LayerOrder);

		var stateTable = DBStage.Get(ZGameModeManager.Instance.StageTid);
		remainderTime.text = $"{TimeHelper.GetRemainFullTime(stateTable.ClearLimitTime)}";

		var gamemode = ZGameModeManager.Instance.CurrentGameMode<ZGameModeGodLand>();
		gamemode.SetRemainTimeCallback(() => {
			ulong nowSec = TimeManager.NowSec;
			if (Me.CurCharData.GodLandContainer.RemainTimeTargetTime > nowSec) {
				StartRaminTime(Me.CurCharData.GodLandContainer.RemainTimeTargetTime);
			}
		});

		ZPawnManager.Instance.DoAddEventCreateMyEntity(OnCreatedMyEntity);
	}

	private void Update()
	{
		if (isTimerStart == false) {
			return;
		}

		secondCheckTimer += Time.deltaTime;
		if (secondCheckTimer >= 0.95f) {
			secondCheckTimer = 0;

			ulong nowSec = TimeManager.NowSec;
			ulong reaminTime = endServerTime - nowSec;

			if (endServerTime <= nowSec) {
				reaminTime = 0;
			}

			string time = TimeHelper.GetRemainFullTime(reaminTime);
			remainderTime.text = $"{time}";

			if (reaminTime < 10) {
				remainderTime.GetComponent<uTweenColor>().enabled = true;
			}
		}
	}

	protected override void OnHide()
	{
		ZPawnManager.Instance.DoRemoveEventCreateMyEntity(OnCreatedMyEntity);

		myItem.Hide();
		enemyItem.Hide();
		resultPopup.Hide();
	}

	private void OnCreatedMyEntity()
	{
		var gamemode = ZGameModeManager.Instance.CurrentGameMode<ZGameModeGodLand>();
		gamemode.SetUserListCallback((userList) => {
			ShowCharacterHudItems(userList);
		});
	}

	private void ShowCharacterHudItems(List<GodLandStatInfoConverted> userList)
	{
		for (int i = 0; i < userList.Count; ++i) {
			var userInfo = userList[i];
			if (userInfo.ObjectID == ZPawnManager.Instance.MyEntityId) {
				myItem.Initialize(userInfo);
				myItem.Show();
			}
			else {
				enemyItem.Initialize(userInfo);
				enemyItem.Show();
			}
		}
	}

	public void SetResult(uint godLandTid, bool isWin)
	{
		resultPopup.ShowResultPopup(godLandTid, isWin);
	}

	public void StartRaminTime(ulong _endServerTime)
	{
		isTimerStart = true;

		endServerTime = _endServerTime;
	}

	public void StopRamainTime()
	{
		isTimerStart = false;
	}
}